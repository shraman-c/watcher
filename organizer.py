import argparse
import os
import time
import threading
import shutil
import json
from pathlib import Path
from typing import Dict, List, Optional, Sequence

from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler

DEFAULT_RULES: Dict[str, List[str]] = {
    "Images":   ["jpg","jpeg","png","gif","bmp","tiff","webp","svg"],
    "Videos":   ["mp4","mov","mkv","avi","wmv","flv","webm","m4v"],
    "Audio":    ["mp3","wav","flac","aac","ogg","m4a","wma"],
    "Documents":["pdf","doc","docx","xls","xlsx","ppt","pptx","txt","rtf"],
    "Archives": ["zip","rar","7z","tar","gz","bz2"],
    "Code":     ["py","js","ts","tsx","jsx","java","cs","cpp","c","h","go","rs","rb","php","html","css","json","yaml","yml","xml","sql","sh","ps1","bat"],
    "Installers":["exe","msi","msix","apk","dmg","pkg"],
    "Fonts":    ["ttf","otf","woff","woff2"],
}


def normalize_rules(rules: Dict[str, List[str]]) -> Dict[str, List[str]]:
    normalized: Dict[str, List[str]] = {}
    for cat, exts in rules.items():
        normalized[cat] = [str(e).lower().lstrip(".") for e in exts if e]
    return normalized


def get_category_for_ext(ext: str, rules: Dict[str, List[str]], unknown_target: str) -> str:
    for cat, exts in rules.items():
        if ext in exts:
            return cat
    return unknown_target


def ensure_directory(path: Path) -> None:
    path.mkdir(parents=True, exist_ok=True)


def wait_for_file_ready(path: Path, max_attempts: int = 40, delay: float = 0.25) -> bool:
    """Try opening the file for reading until it is ready (not locked)."""
    for _ in range(max_attempts):
        try:
            with path.open("rb"):
                return True
        except Exception:
            time.sleep(delay)
    return False


def unique_destination(dest_dir: Path, filename: str) -> Path:
    base = Path(filename).stem
    ext = Path(filename).suffix
    candidate = dest_dir / filename
    counter = 1
    while candidate.exists():
        candidate = dest_dir / f"{base} ({counter}){ext}"
        counter += 1
    return candidate


class CreatedHandler(FileSystemEventHandler):
    def __init__(self, root: Path, rules: Dict[str, List[str]], move_unknown_to: str, quiet: bool = False) -> None:
        super().__init__()
        self.root = root
        self.rules = rules
        self.move_unknown_to = move_unknown_to
        self.quiet = quiet

    def on_created(self, event):
        # Ignore directories
        if event.is_directory:
            return
        src_path = Path(event.src_path)
        # Guard: only handle top-level files (not inside subfolders)
        try:
            rel = src_path.relative_to(self.root)
        except ValueError:
            # outside root
            return
        if len(rel.parts) > 1:
            # file in subfolder; ignore
            return

        name = src_path.name
        # Wait until file is ready
        if not wait_for_file_ready(src_path):
            print(f"[ERROR] File not ready after retries: {name}")
            return

        ext = src_path.suffix.lower().lstrip(".")
        category = self.move_unknown_to if not ext else get_category_for_ext(ext, self.rules, self.move_unknown_to)
        dest_dir = self.root / category
        ensure_directory(dest_dir)
        dest_path = unique_destination(dest_dir, name)
        try:
            shutil.move(str(src_path), str(dest_path))
            if not self.quiet:
                print(f"[INFO] Moved: {name} -> {category}")
        except Exception as e:
            print(f"[ERROR] Failed to move '{name}': {e}")


def start_watcher(path: Path, rules: Optional[Dict[str, List[str]]] = None, move_unknown_to: str = "Other", quiet: bool = False) -> Observer:
    root = path.resolve()
    use_rules = normalize_rules(rules or DEFAULT_RULES)
    handler = CreatedHandler(root, use_rules, move_unknown_to, quiet)
    observer = Observer()
    observer.schedule(handler, str(root), recursive=False)  # top-level only
    observer.start()
    print(f"[INFO] Watching '{root}' (top-level only). Press Ctrl+C to stop.")
    return observer


def start_watchers(paths: Sequence[Path], rules: Optional[Dict[str, List[str]]] = None, move_unknown_to: str = "Other", quiet: bool = False) -> List[Observer]:
    observers: List[Observer] = []
    for p in paths:
        observers.append(start_watcher(p, rules, move_unknown_to, quiet))
    return observers


def create_test_files(path: Path) -> Path:
    test_dir = path / "_test_organize_samples"
    if test_dir.exists():
        shutil.rmtree(test_dir)
    test_dir.mkdir(parents=True, exist_ok=True)
    samples = {
        "sample.jpg": "fake image data",
        "document.pdf": "fake pdf",
        "video.mp4": "fake video",
        "song.mp3": "fake audio",
        "archive.zip": "fake zip",
        "code.py": "print('hello')",
        "unknown.xyz": "unknown type",
    }
    for name, content in samples.items():
        (test_dir / name).write_text(content)
    print(f"[OK] Test files created in: {test_dir}")
    return test_dir


def parse_rules_json(rules_json: Optional[str]) -> Optional[Dict[str, List[str]]]:
    if not rules_json:
        return None
    try:
        data = json.loads(rules_json)
        # Expect mapping of category to list of extensions
        parsed: Dict[str, List[str]] = {}
        for cat, exts in data.items():
            if isinstance(exts, list):
                parsed[str(cat)] = [str(x) for x in exts]
        return parsed if parsed else None
    except Exception as e:
        raise ValueError(f"Invalid rules JSON: {e}")


def main():
    parser = argparse.ArgumentParser(description="Watch folder(s) and organize newly created files by type.")
    parser.add_argument("-p", "--path", dest="paths", required=True, nargs="+", help="One or more directories to watch")
    parser.add_argument("--move-unknown-to", default="Other", help="Target folder for unknown file types")
    parser.add_argument("--quiet", action="store_true", help="Suppress informational logs, show only errors")
    parser.add_argument("--rules-json", help="JSON string mapping category to extensions (e.g., '{\"Images\":[\"jpg\",\"png\"]}')")
    parser.add_argument("--create-test-files", action="store_true", help="Create sample files to test organization in each path")

    args = parser.parse_args()
    roots = []
    for p in args.paths:
        root = Path(p)
        if not root.exists() or not root.is_dir():
            raise SystemExit(f"Path does not exist or is not a directory: {root}")
        roots.append(root)

    rules = parse_rules_json(args.rules_json)

    if args.create_test_files:
        for root in roots:
            create_test_files(root)

    observers = start_watchers(roots, rules, args.move_unknown_to, args.quiet)
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("[INFO] Stopping watcher(s)...")
        for obs in observers:
            obs.stop()
    for obs in observers:
        obs.join()


if __name__ == "__main__":
    main()
