import threading
import time
import tkinter as tk
from tkinter import filedialog, messagebox
from pathlib import Path
from typing import Dict, List, Optional

from organizer import start_watcher, create_test_files, normalize_rules, DEFAULT_RULES

class App:
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title("Watch Organizer - Cross-Platform GUI")
        self.root.geometry("700x520")
        self.root.configure(bg="#ffffff")

        self.observer = None

        # Path
        tk.Label(root, text="Watch Directory:", font=("Segoe UI", 10, "bold"), bg="#ffffff").place(x=20, y=20)
        self.path_var = tk.StringVar(value=str(Path.home() / "Downloads"))
        tk.Entry(root, textvariable=self.path_var, width=55).place(x=20, y=50)
        tk.Button(root, text="Browse", command=self.browse_path).place(x=580, y=48)

        # Unknown target
        tk.Label(root, text="Unknown File Target Folder:", font=("Segoe UI", 10, "bold"), bg="#ffffff").place(x=20, y=90)
        self.unknown_var = tk.StringVar(value="Other")
        tk.Entry(root, textvariable=self.unknown_var, width=25).place(x=20, y=120)

        # Quiet mode
        self.quiet_var = tk.BooleanVar(value=False)
        tk.Checkbutton(root, text="Quiet Mode (errors only)", variable=self.quiet_var, bg="#ffffff").place(x=220, y=118)

        # Rules
        tk.Label(root, text="Custom File Type Rules (optional):", font=("Segoe UI", 10, "bold"), bg="#ffffff").place(x=20, y=160)
        tk.Label(root, text="Format: Category = ext1,ext2,ext3 (one per line)", bg="#ffffff").place(x=20, y=185)
        self.rules_text = tk.Text(root, width=80, height=8)
        self.rules_text.place(x=20, y=210)
        self.rules_text.insert("1.0", "Images = jpg,jpeg,png\nVideos = mp4,mov,mkv,avi\nAudio = mp3,wav,flac,aac\nDocuments = pdf,doc,docx,txt\n")

        # Status
        self.status_var = tk.StringVar(value="Ready")
        tk.Label(root, textvariable=self.status_var, anchor="w", width=90, bg="#f0f9ff").place(x=20, y=360)

        # Buttons
        tk.Button(root, text="Start Watcher", bg="#10b981", fg="#ffffff", command=self.start_watcher_clicked).place(x=20, y=400, width=140, height=35)
        tk.Button(root, text="Test Sample Files", bg="#3b82f6", fg="#ffffff", command=self.create_test_files_clicked).place(x=170, y=400, width=140, height=35)
        tk.Button(root, text="Exit", bg="#6b7280", fg="#ffffff", command=self.exit_clicked).place(x=320, y=400, width=140, height=35)

        # Cleanup on close
        self.root.protocol("WM_DELETE_WINDOW", self.exit_clicked)

    def browse_path(self):
        p = filedialog.askdirectory(title="Select directory to watch")
        if p:
            self.path_var.set(p)

    def parse_rules(self) -> Optional[Dict[str, List[str]]]:
        text = self.rules_text.get("1.0", tk.END).strip()
        if not text:
            return None
        rules: Dict[str, List[str]] = {}
        for line in text.splitlines():
            line = line.strip()
            if not line or line.startswith("#"):
                continue
            if "=" not in line:
                continue
            cat, exts = line.split("=", 1)
            cat = cat.strip()
            ext_list = [e.strip() for e in exts.split(",") if e.strip()]
            rules[cat] = ext_list
        return rules if rules else None

    def start_watcher_clicked(self):
        path = Path(self.path_var.get().strip())
        if not path.exists() or not path.is_dir():
            messagebox.showerror("Error", "Path does not exist or is not a directory.")
            return
        unknown = self.unknown_var.get().strip() or "Other"
        quiet = bool(self.quiet_var.get())
        rules = self.parse_rules()

        def run_watcher():
            try:
                self.observer = start_watcher(path, rules, unknown, quiet)
            except Exception as e:
                self.status_var.set(f"ERROR: {e}")
                return
            self.status_var.set("Watcher started. Close window to stop.")
            try:
                while self.observer and self.observer.is_alive():
                    time.sleep(0.5)
            except KeyboardInterrupt:
                pass

        t = threading.Thread(target=run_watcher, daemon=True)
        t.start()

    def create_test_files_clicked(self):
        path = Path(self.path_var.get().strip())
        if not path.exists() or not path.is_dir():
            messagebox.showerror("Error", "Path does not exist or is not a directory.")
            return
        td = create_test_files(path)
        self.status_var.set(f"Test files created in: {td}")
        messagebox.showinfo("Test Files Ready", f"Created in:\n\n{td}\n\nStart the watcher to see them organize!")

    def exit_clicked(self):
        try:
            if self.observer:
                self.observer.stop()
                self.observer.join(timeout=3)
        except Exception:
            pass
        self.root.destroy()


def main():
    root = tk.Tk()
    App(root)
    root.mainloop()


if __name__ == "__main__":
    main()
