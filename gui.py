
"""Watcher - PyQt6 based desktop application for automated file organization."""

from __future__ import annotations

import json
import importlib.util
import subprocess
import platform
import sys
from datetime import datetime
from enum import Enum
from pathlib import Path
from typing import Any, Dict, List, Optional

BASE_DIR = Path(__file__).resolve().parent
REQUIREMENTS_PATH = BASE_DIR / "requirements.txt"
REQUIRED_DEPENDENCIES = {
    "PyQt6": "PyQt6>=6.7.0",
    "watchdog": "watchdog>=4.0.0",
}


def _ensure_dependencies() -> None:
    missing = [spec for module_name, spec in REQUIRED_DEPENDENCIES.items() if importlib.util.find_spec(module_name) is None]

    if not missing:
        return

    print(f"Missing dependencies detected: {', '.join(missing)}", file=sys.stderr)
    print("Installing required packages from requirements.txt...", file=sys.stderr)

    pip_command = [sys.executable, "-m", "pip", "install", "--disable-pip-version-check"]
    if sys.prefix == getattr(sys, "base_prefix", sys.prefix):
        pip_command.append("--user")
    pip_command.extend(["-r", str(REQUIREMENTS_PATH)])

    result = subprocess.run(pip_command, check=False)
    if result.returncode != 0:
        raise RuntimeError("Automatic dependency installation failed. Please run: pip install -r requirements.txt")

    still_missing = [spec for module_name, spec in REQUIRED_DEPENDENCIES.items() if importlib.util.find_spec(module_name) is None]

    if still_missing:
        raise RuntimeError(
            "Some dependencies are still missing after installation: " + ", ".join(still_missing)
        )


_ensure_dependencies()

from PyQt6.QtCore import QTimer, pyqtSignal
from PyQt6.QtGui import QFont
from PyQt6.QtWidgets import (
    QApplication,
    QCheckBox,
    QFileDialog,
    QFrame,
    QHBoxLayout,
    QLabel,
    QLineEdit,
    QMainWindow,
    QMessageBox,
    QPushButton,
    QSplitter,
    QStatusBar,
    QTabWidget,
    QTableWidget,
    QTableWidgetItem,
    QTextEdit,
    QVBoxLayout,
    QWidget,
)
from watchdog.events import FileSystemEventHandler
from watchdog.observers import Observer

CONFIG_PATH = Path.home() / ".watcher_config.json"

DEFAULT_RULES: Dict[str, List[str]] = {
    "Images": ["jpg", "jpeg", "png", "gif", "bmp", "svg", "webp"],
    "Videos": ["mp4", "avi", "mkv", "mov", "flv", "wmv", "webm"],
    "Audio": ["mp3", "wav", "flac", "aac", "m4a", "wma", "ogg"],
    "Documents": ["pdf", "doc", "docx", "txt", "xls", "xlsx", "ppt", "pptx"],
    "Archives": ["zip", "rar", "7z", "tar", "gz", "bz2"],
    "Executables": ["exe", "msi", "app", "dmg"],
    "Code": ["py", "js", "ts", "java", "cpp", "c", "go", "rb", "php"],
    "Fonts": ["ttf", "otf", "woff", "woff2"],
}


class LogLevel(Enum):
    INFO = "INFO"
    SUCCESS = "SUCCESS"
    WARNING = "WARNING"
    ERROR = "ERROR"


class FileSystemObserverHandler(FileSystemEventHandler):
    """Handle file creation events and move files by extension rules."""

    def __init__(
        self,
        rules: Dict[str, List[str]],
        unknown_target: str = "Other",
        quiet: bool = False,
        log_callback=None,
    ) -> None:
        self.rules = rules
        self.unknown_target = unknown_target
        self.quiet = quiet
        self.log_callback = log_callback
        self.processed: set[str] = set()
        self.file_count = 0

    def on_created(self, event):
        if not event.is_directory and event.src_path not in self.processed:
            self.processed.add(event.src_path)
            self._organize_file(Path(event.src_path))

    def _organize_file(self, file_path: Path) -> None:
        try:
            ext = file_path.suffix.lstrip(".").lower()
            target = self.unknown_target

            for category, extensions in self.rules.items():
                if ext in extensions:
                    target = category
                    break

            target_dir = file_path.parent / target
            target_dir.mkdir(exist_ok=True)

            final_path = target_dir / file_path.name
            if file_path != final_path and file_path.exists():
                file_path.rename(final_path)
                self.file_count += 1
                if not self.quiet and self.log_callback:
                    self.log_callback(f"Organized: {file_path.name} -> {target}/", LogLevel.SUCCESS)
        except Exception as e:
            if self.log_callback:
                self.log_callback(f"Error organizing {file_path.name}: {e}", LogLevel.ERROR)


def start_watcher(
    path: Path,
    rules: Optional[Dict[str, List[str]]] = None,
    unknown_target: str = "Other",
    quiet: bool = False,
    log_callback=None,
) -> Any:
    observer = Observer()
    handler = FileSystemObserverHandler(
        rules or DEFAULT_RULES,
        unknown_target=unknown_target,
        quiet=quiet,
        log_callback=log_callback,
    )
    observer.schedule(handler, str(path), recursive=False)
    observer.start()
    observer.handler = handler
    return observer


def organize_existing_files(
    path: Path,
    rules: Dict[str, List[str]],
    unknown_target: str = "Other",
    quiet: bool = False,
    log_callback=None,
) -> int:
    handler = FileSystemObserverHandler(
        rules=rules,
        unknown_target=unknown_target,
        quiet=quiet,
        log_callback=log_callback,
    )
    processed = 0

    try:
        for item in path.iterdir():
            if item.is_file():
                handler._organize_file(item)
                processed += 1
    except Exception as e:
        if log_callback:
            log_callback(f"Error scanning existing files in {path}: {e}", LogLevel.ERROR)

    return processed


def parse_rules_text(text: str) -> Dict[str, List[str]]:
    if not text.strip():
        return DEFAULT_RULES.copy()

    try:
        parsed = json.loads(text)
        return parsed if isinstance(parsed, dict) else DEFAULT_RULES.copy()
    except json.JSONDecodeError:
        rules: Dict[str, List[str]] = {}
        for line in text.splitlines():
            line = line.strip()
            if line and "=" in line:
                category, exts = line.split("=", 1)
                category = category.strip()
                extensions = [e.strip() for e in exts.split(",") if e.strip()]
                if category and extensions:
                    rules[category] = extensions
        return rules if rules else DEFAULT_RULES.copy()


def rules_to_text(rules: Dict[str, List[str]]) -> str:
    return "\n".join(f"{cat} = {','.join(exts)}" for cat, exts in sorted(rules.items()))


class WatcherApp(QMainWindow):
    log_signal = pyqtSignal(str, object)

    def __init__(self) -> None:
        super().__init__()
        self.setWindowTitle("Watcher - Automate File Organization")
        self.resize(1200, 800)
        self.setMinimumSize(900, 580)

        self.settings = self.load_settings()
        self.folders: List[Dict[str, Any]] = self.normalize_folders(self.settings.get("folders", []))
        self.rules_dict = parse_rules_text(self.settings.get("rules_text", ""))
        self.observers: List[Any] = []
        self.stats = {"total_errors": 0, "uptime": datetime.now()}

        self.log_signal.connect(self._append_log)

        self.build_ui()
        self.apply_styles()
        self.refresh_folder_list()
        self.update_status()
        self.update_stats_display()

        self.stats_timer = QTimer(self)
        self.stats_timer.timeout.connect(self.update_stats_display)
        self.stats_timer.start(1000)

    def build_ui(self) -> None:
        central = QWidget(self)
        self.setCentralWidget(central)
        root = QVBoxLayout(central)
        root.setContentsMargins(8, 8, 8, 8)

        self.tabs = QTabWidget()
        root.addWidget(self.tabs)

        self.build_folders_tab()
        self.build_rules_tab()
        self.build_monitoring_tab()
        self.build_advanced_tab()

        self.status_bar = QStatusBar(self)
        self.setStatusBar(self.status_bar)
        self.status_label = QLabel("Ready")
        self.stats_label = QLabel("Files organized: 0 | Errors: 0")
        self.status_bar.addWidget(self.status_label)
        self.status_bar.addPermanentWidget(self.stats_label)

    def build_folders_tab(self) -> None:
        tab = QWidget()
        self.tabs.addTab(tab, "Folders")

        layout = QVBoxLayout(tab)
        splitter = QSplitter()
        layout.addWidget(splitter)

        left = QWidget()
        left_layout = QVBoxLayout(left)
        left_layout.addWidget(QLabel("Monitored Folders"))

        self.folder_table = QTableWidget(0, 4)
        self.folder_table.setHorizontalHeaderLabels(["Path", "Unknown Target", "Quiet", "Status"])
        self.folder_table.horizontalHeader().setStretchLastSection(True)
        self.folder_table.setSelectionBehavior(QTableWidget.SelectionBehavior.SelectRows)
        self.folder_table.setSelectionMode(QTableWidget.SelectionMode.SingleSelection)
        self.folder_table.itemSelectionChanged.connect(self.on_folder_selected)
        left_layout.addWidget(self.folder_table)

        self.folder_count_label = QLabel("Total: 0")
        left_layout.addWidget(self.folder_count_label)

        right = QWidget()
        right_layout = QVBoxLayout(right)

        form = QFrame()
        form_layout = QVBoxLayout(form)

        form_layout.addWidget(QLabel("Folder Path"))
        path_row = QHBoxLayout()
        self.path_input = QLineEdit()
        browse_btn = QPushButton("Browse")
        browse_btn.clicked.connect(self.on_browse)
        path_row.addWidget(self.path_input)
        path_row.addWidget(browse_btn)
        form_layout.addLayout(path_row)

        form_layout.addWidget(QLabel("Unknown Target"))
        self.unknown_input = QLineEdit("Other")
        form_layout.addWidget(self.unknown_input)

        self.quiet_checkbox = QCheckBox("Quiet Mode (reduce logs)")
        form_layout.addWidget(self.quiet_checkbox)

        right_layout.addWidget(form)

        actions = QHBoxLayout()
        add_btn = QPushButton("Add")
        update_btn = QPushButton("Update")
        remove_btn = QPushButton("Remove")
        add_btn.clicked.connect(self.on_add_folder)
        update_btn.clicked.connect(self.on_update_folder)
        remove_btn.clicked.connect(self.on_remove_folder)
        actions.addWidget(add_btn)
        actions.addWidget(update_btn)
        actions.addWidget(remove_btn)
        right_layout.addLayout(actions)
        right_layout.addStretch(1)

        splitter.addWidget(left)
        splitter.addWidget(right)
        splitter.setSizes([760, 360])

    def build_rules_tab(self) -> None:
        tab = QWidget()
        self.tabs.addTab(tab, "Rules")
        layout = QVBoxLayout(tab)

        btn_row = QHBoxLayout()
        load_btn = QPushButton("Load Defaults")
        validate_btn = QPushButton("Validate")
        export_btn = QPushButton("Export")
        load_btn.clicked.connect(self.on_load_default_rules)
        validate_btn.clicked.connect(self.on_validate_rules)
        export_btn.clicked.connect(self.on_export_rules)
        btn_row.addWidget(load_btn)
        btn_row.addWidget(validate_btn)
        btn_row.addWidget(export_btn)
        btn_row.addStretch(1)

        layout.addLayout(btn_row)

        self.rules_editor = QTextEdit()
        self.rules_editor.setPlainText(rules_to_text(self.rules_dict))
        layout.addWidget(self.rules_editor)

    def build_monitoring_tab(self) -> None:
        tab = QWidget()
        self.tabs.addTab(tab, "Monitoring")
        layout = QVBoxLayout(tab)

        controls = QHBoxLayout()
        start_btn = QPushButton("Start Watchers")
        stop_btn = QPushButton("Stop Watchers")
        clear_btn = QPushButton("Clear Logs")
        copy_btn = QPushButton("Copy Logs")
        test_btn = QPushButton("Create Test Files")

        start_btn.clicked.connect(self.on_start_watchers)
        stop_btn.clicked.connect(self.on_stop_watchers)
        clear_btn.clicked.connect(self.on_clear_logs)
        copy_btn.clicked.connect(self.on_copy_logs)
        test_btn.clicked.connect(self.on_create_test_files)

        controls.addWidget(start_btn)
        controls.addWidget(stop_btn)
        controls.addWidget(clear_btn)
        controls.addWidget(copy_btn)
        controls.addWidget(test_btn)
        controls.addStretch(1)

        self.log_widget = QTextEdit()
        self.log_widget.setReadOnly(True)

        layout.addLayout(controls)
        layout.addWidget(self.log_widget)

    def build_advanced_tab(self) -> None:
        tab = QWidget()
        self.tabs.addTab(tab, "Advanced")
        layout = QVBoxLayout(tab)

        self.system_info_widget = QTextEdit()
        self.system_info_widget.setReadOnly(True)
        self.system_info_widget.setPlainText(self.get_system_info())

        refresh_btn = QPushButton("Refresh System Info")
        refresh_btn.clicked.connect(lambda: self.system_info_widget.setPlainText(self.get_system_info()))

        layout.addWidget(refresh_btn)
        layout.addWidget(self.system_info_widget)

    def apply_styles(self) -> None:
        app = QApplication.instance()
        if app:
            app.setFont(QFont("Segoe UI", 10))

        self.setStyleSheet(
            """
            QWidget {
                font-family: "Segoe UI", "Inter", "Noto Sans", sans-serif;
                font-size: 13px;
                background: #12141b;
                color: #e6e9ef;
            }

            QMainWindow {
                background: #12141b;
            }

            QTabWidget::pane {
                border: 1px solid #2b2f3a;
                background: #181c25;
                top: -1px;
            }

            QTabBar::tab {
                background: #151922;
                color: #a8b0bf;
                border: 1px solid #2b2f3a;
                border-bottom: none;
                border-top-left-radius: 8px;
                border-top-right-radius: 8px;
                padding: 8px 12px;
                margin-right: 4px;
            }

            QTabBar::tab:selected {
                background: #1f2430;
                color: #f1f4fa;
            }

            QTabBar::tab:hover {
                background: #232938;
            }

            QPushButton {
                border: none;
                background: #2f6feb;
                color: #ffffff;
                border-radius: 8px;
                padding: 8px 12px;
            }
            QPushButton:hover {
                background: #3b82f6;
            }
            QPushButton:pressed {
                background: #1f4f9e;
            }
            QPushButton:disabled {
                background: #2a3140;
                color: #7e8798;
            }

            QLineEdit, QTextEdit, QTableWidget {
                border: 1px solid #323848;
                border-radius: 8px;
                background: #1a1f2b;
                color: #e6e9ef;
                selection-background-color: #2f6feb;
                selection-color: #ffffff;
            }

            QLineEdit:focus, QTextEdit:focus, QTableWidget:focus {
                border: 1px solid #4d8dff;
            }

            QHeaderView::section {
                background: #232938;
                color: #d8deea;
                border: none;
                border-right: 1px solid #2e3544;
                border-bottom: 1px solid #2e3544;
                padding: 6px;
                font-weight: 600;
            }

            QTableCornerButton::section {
                background: #232938;
                border: none;
                border-right: 1px solid #2e3544;
                border-bottom: 1px solid #2e3544;
            }

            QTableWidget::item {
                border-bottom: 1px solid #252b39;
                padding: 4px;
            }

            QTableWidget::item:selected {
                background: #2f6feb;
                color: #ffffff;
            }

            QSplitter::handle {
                background: #2b3140;
            }

            QCheckBox {
                color: #dbe1ed;
            }

            QCheckBox::indicator {
                width: 14px;
                height: 14px;
                border: 1px solid #4a5568;
                border-radius: 3px;
                background: #1a1f2b;
            }

            QCheckBox::indicator:checked {
                background: #2f6feb;
                border: 1px solid #2f6feb;
            }

            QStatusBar {
                background: #171b24;
                color: #cfd6e3;
                border-top: 1px solid #2b2f3a;
            }

            QMessageBox {
                background: #181c25;
            }

            QLabel {
                background: transparent;
            }
            """
        )

    def normalize_folders(self, folders: Any) -> List[Dict[str, Any]]:
        normalized: List[Dict[str, Any]] = []
        if not isinstance(folders, list):
            return normalized

        for item in folders:
            if isinstance(item, str):
                path_value = item
                unknown_value = "Other"
                quiet_value = False
            elif isinstance(item, dict):
                path_value = str(item.get("path") or item.get("folder") or item.get("source") or "").strip()
                unknown_value = str(item.get("unknown") or item.get("target") or item.get("unknown_target") or "Other").strip() or "Other"
                quiet_value = bool(item.get("quiet", item.get("silent", False)))
            else:
                continue

            if not path_value:
                continue

            normalized.append(
                {
                    "path": str(Path(path_value).expanduser()),
                    "unknown": unknown_value,
                    "quiet": quiet_value,
                }
            )

        return normalized

    def add_log(self, message: str, level: LogLevel = LogLevel.INFO) -> None:
        self.log_signal.emit(message, level)

    def _append_log(self, message: str, level_obj: object) -> None:
        level = level_obj if isinstance(level_obj, LogLevel) else LogLevel.INFO
        if level == LogLevel.ERROR:
            self.stats["total_errors"] += 1

        prefix = {
            LogLevel.INFO: "[INFO]",
            LogLevel.SUCCESS: "[OK]",
            LogLevel.WARNING: "[WARN]",
            LogLevel.ERROR: "[ERR]",
        }.get(level, "[INFO]")

        ts = datetime.now().strftime("%H:%M:%S")
        self.log_widget.append(f"[{ts}] {prefix} {message}")

    def refresh_folder_list(self) -> None:
        self.folder_table.setRowCount(0)
        status = "Active" if self.observers else "Idle"

        for folder in self.folders:
            row = self.folder_table.rowCount()
            self.folder_table.insertRow(row)
            self.folder_table.setItem(row, 0, QTableWidgetItem(folder.get("path", "")))
            self.folder_table.setItem(row, 1, QTableWidgetItem(folder.get("unknown", "Other")))
            self.folder_table.setItem(row, 2, QTableWidgetItem("Yes" if folder.get("quiet", False) else "No"))
            self.folder_table.setItem(row, 3, QTableWidgetItem(status))

        self.folder_count_label.setText(f"Total: {len(self.folders)}")

    def selected_folder_index(self) -> Optional[int]:
        row = self.folder_table.currentRow()
        if row < 0 or row >= len(self.folders):
            return None
        return row

    def on_folder_selected(self) -> None:
        idx = self.selected_folder_index()
        if idx is None:
            return

        folder = self.folders[idx]
        self.path_input.setText(folder.get("path", ""))
        self.unknown_input.setText(folder.get("unknown", "Other"))
        self.quiet_checkbox.setChecked(bool(folder.get("quiet", False)))

    def on_browse(self) -> None:
        folder = QFileDialog.getExistingDirectory(self, "Select folder")
        if folder:
            self.path_input.setText(folder)

    def on_add_folder(self) -> None:
        path_text = self.path_input.text().strip()
        if not path_text:
            QMessageBox.warning(self, "Error", "Folder path is required")
            return

        path = Path(path_text).expanduser()
        if not path.exists() or not path.is_dir():
            QMessageBox.warning(self, "Error", f"Folder does not exist:\n{path}")
            return

        self.folders.append(
            {
                "path": str(path),
                "unknown": self.unknown_input.text().strip() or "Other",
                "quiet": self.quiet_checkbox.isChecked(),
            }
        )
        self.refresh_folder_list()
        self.save_settings()
        self.add_log(f"Added folder: {path}", LogLevel.SUCCESS)

    def on_update_folder(self) -> None:
        idx = self.selected_folder_index()
        if idx is None:
            QMessageBox.warning(self, "Error", "Select a folder to update")
            return

        path_text = self.path_input.text().strip()
        if not path_text:
            QMessageBox.warning(self, "Error", "Folder path is required")
            return

        path = Path(path_text).expanduser()
        self.folders[idx] = {
            "path": str(path),
            "unknown": self.unknown_input.text().strip() or "Other",
            "quiet": self.quiet_checkbox.isChecked(),
        }
        self.refresh_folder_list()
        self.save_settings()
        self.add_log(f"Updated folder: {path}", LogLevel.SUCCESS)

    def on_remove_folder(self) -> None:
        idx = self.selected_folder_index()
        if idx is None:
            QMessageBox.warning(self, "Error", "Select a folder to remove")
            return

        removed = self.folders.pop(idx)
        self.refresh_folder_list()
        self.save_settings()
        self.add_log(f"Removed folder: {removed.get('path', '')}", LogLevel.INFO)

    def on_load_default_rules(self) -> None:
        self.rules_editor.setPlainText(rules_to_text(DEFAULT_RULES))
        self.rules_dict = DEFAULT_RULES.copy()
        self.save_settings()
        self.add_log("Loaded default rules", LogLevel.SUCCESS)

    def on_validate_rules(self) -> None:
        try:
            rules = parse_rules_text(self.rules_editor.toPlainText())
            categories = len(rules)
            ext_count = sum(len(v) for v in rules.values())
            self.rules_dict = rules
            self.save_settings()
            QMessageBox.information(self, "Validation", f"Rules are valid.\n\nCategories: {categories}\nExtensions: {ext_count}")
            self.add_log(f"Validated rules: {categories} categories, {ext_count} extensions", LogLevel.SUCCESS)
        except Exception as e:
            QMessageBox.critical(self, "Validation Error", f"Invalid rules:\n{e}")
            self.add_log(f"Rules validation failed: {e}", LogLevel.ERROR)

    def on_export_rules(self) -> None:
        filename, _ = QFileDialog.getSaveFileName(
            self,
            "Export rules",
            "rules.json",
            "JSON files (*.json);;Text files (*.txt);;All files (*.*)",
        )
        if not filename:
            return

        try:
            Path(filename).write_text(self.rules_editor.toPlainText(), encoding="utf-8")
            self.add_log(f"Exported rules to: {filename}", LogLevel.SUCCESS)
            QMessageBox.information(self, "Success", f"Rules exported to:\n{filename}")
        except Exception as e:
            QMessageBox.critical(self, "Export Error", f"Failed to export rules:\n{e}")
            self.add_log(f"Rules export failed: {e}", LogLevel.ERROR)

    def on_start_watchers(self) -> None:
        if self.observers:
            QMessageBox.information(self, "Info", "Watchers already running")
            return

        if not self.folders:
            QMessageBox.warning(self, "Error", "No folders configured")
            return

        self.rules_dict = parse_rules_text(self.rules_editor.toPlainText())
        started = 0

        for folder in self.folders:
            p = Path(folder.get("path", "")).expanduser()
            if not p.exists() or not p.is_dir():
                self.add_log(f"Invalid folder: {p}", LogLevel.WARNING)
                continue

            unknown_target = folder.get("unknown") or folder.get("target") or "Other"
            quiet_mode = bool(folder.get("quiet", False))

            scanned = organize_existing_files(
                p,
                self.rules_dict if self.rules_dict else DEFAULT_RULES,
                unknown_target,
                quiet_mode,
                log_callback=self.add_log,
            )
            if scanned and not quiet_mode:
                self.add_log(f"Scanned existing files in {p}: {scanned}", LogLevel.INFO)

            try:
                obs = start_watcher(
                    p,
                    self.rules_dict if self.rules_dict else DEFAULT_RULES,
                    unknown_target,
                    quiet_mode,
                    log_callback=self.add_log,
                )
                self.observers.append(obs)
                started += 1
                self.add_log(f"Started watching: {p}", LogLevel.SUCCESS)
            except Exception as e:
                self.add_log(f"Error starting watcher for {p}: {e}", LogLevel.ERROR)

        self.refresh_folder_list()
        self.update_status()

        if started:
            QMessageBox.information(self, "Watchers", f"Started {started} watcher(s)")

    def on_stop_watchers(self) -> None:
        if not self.observers:
            self.add_log("No watchers running", LogLevel.INFO)
            return

        for obs in self.observers:
            try:
                obs.stop()
            except Exception:
                pass

        for obs in self.observers:
            try:
                obs.join(timeout=5)
            except Exception:
                pass

        self.observers.clear()
        self.refresh_folder_list()
        self.update_status()
        self.add_log("All watchers stopped", LogLevel.INFO)

    def on_clear_logs(self) -> None:
        self.log_widget.clear()

    def on_copy_logs(self) -> None:
        QApplication.clipboard().setText(self.log_widget.toPlainText())
        QMessageBox.information(self, "Copied", "Logs copied to clipboard")

    def on_create_test_files(self) -> None:
        idx = self.selected_folder_index()
        if idx is None:
            QMessageBox.warning(self, "Error", "Select a folder")
            return

        p = Path(self.folders[idx].get("path", "")).expanduser()
        if not p.exists() or not p.is_dir():
            QMessageBox.warning(self, "Error", f"Folder does not exist: {p}")
            return

        samples = [
            "test_image.jpg",
            "test_video.mp4",
            "test_audio.mp3",
            "test_document.pdf",
            "test_archive.zip",
            "test_code.py",
            "test_unknown.xyz",
        ]

        try:
            for name in samples:
                (p / name).write_bytes(b"test content")
            self.add_log(f"Created {len(samples)} test files in {p}", LogLevel.SUCCESS)
            QMessageBox.information(self, "Success", f"Created {len(samples)} test files in:\n{p}")
        except Exception as e:
            self.add_log(f"Failed to create test files: {e}", LogLevel.ERROR)
            QMessageBox.critical(self, "Error", f"Failed to create test files:\n{e}")

    def update_status(self) -> None:
        running = len(self.observers)
        count = len(self.folders)
        if running:
            self.status_label.setText(f"ACTIVE: {running} watcher(s) monitoring {count} folder(s)")
        else:
            self.status_label.setText(f"IDLE: {count} folder(s) configured")

    def update_stats_display(self) -> None:
        total_files = sum(obs.handler.file_count for obs in self.observers if hasattr(obs, "handler"))
        self.stats_label.setText(f"Files organized: {total_files} | Errors: {self.stats['total_errors']}")

    def get_system_info(self) -> str:
        return (
            "System Information\n"
            "------------------------------\n"
            f"Python: {sys.version.split()[0]}\n"
            f"Platform: {platform.system()} {platform.release()}\n"
            f"OS: {sys.platform}\n\n"
            "Configuration\n"
            "------------------------------\n"
            f"Config File: {CONFIG_PATH}\n"
            f"Default Rule Categories: {len(DEFAULT_RULES)}\n"
            f"Folders Configured: {len(self.folders)}\n"
            f"Watchers Active: {len(self.observers)}\n"
        )

    def load_settings(self) -> Dict[str, Any]:
        if CONFIG_PATH.exists():
            try:
                loaded = json.loads(CONFIG_PATH.read_text(encoding="utf-8"))
                return loaded if isinstance(loaded, dict) else {}
            except Exception as e:
                print(f"Error loading settings: {e}")
        return {"folders": [], "rules_text": rules_to_text(DEFAULT_RULES)}

    def save_settings(self) -> None:
        data = {
            "folders": self.folders,
            "rules_text": self.rules_editor.toPlainText() if hasattr(self, "rules_editor") else rules_to_text(self.rules_dict),
        }
        try:
            CONFIG_PATH.write_text(json.dumps(data, indent=2), encoding="utf-8")
        except Exception as e:
            self.add_log(f"Error saving settings: {e}", LogLevel.ERROR)

    def closeEvent(self, event) -> None:
        self.on_stop_watchers()
        self.save_settings()
        super().closeEvent(event)


# Backward-compatible class name used by existing scripts/tests.
class WatcherGUI(WatcherApp):
    pass


def main() -> None:
    app = QApplication(sys.argv)
    app.setApplicationName("Watcher")
    window = WatcherApp()
    window.show()
    sys.exit(app.exec())


if __name__ == "__main__":
    main()
