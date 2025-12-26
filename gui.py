import json
import threading
import time
import tkinter as tk
from tkinter import filedialog, messagebox
from pathlib import Path
from typing import Dict, List, Optional

from organizer import start_watcher, create_test_files


CONFIG_PATH = Path(__file__).with_name("settings.json")
DEFAULT_RULES_TEXT = """Images = jpg,jpeg,png
Videos = mp4,mov,mkv,avi
Audio = mp3,wav,flac,aac
Documents = pdf,doc,docx,txt
"""


def load_settings():
    if CONFIG_PATH.exists():
        try:
            return json.loads(CONFIG_PATH.read_text())
        except Exception:
            return {}
    return {}


def save_settings(data):
    try:
        CONFIG_PATH.write_text(json.dumps(data, indent=2))
    except Exception as e:
        print(f"[WARN] Could not save settings: {e}")


def parse_rules_text(text: str) -> Optional[Dict[str, List[str]]]:
    text = text.strip()
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
        if ext_list:
            rules[cat] = ext_list
    return rules if rules else None


class App:
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title("Watcher - Cross-Platform GUI")
        self.root.geometry("760x580")
        self.root.configure(bg="#ffffff")

        self.observers: List = []

        # Load settings
        data = load_settings()
        self.folders = data.get("folders", [])  # list of {path, unknown, quiet}
        if not self.folders:
            self.folders = [{"path": str(Path.home() / "Downloads"), "unknown": "Other", "quiet": False}]
        self.rules_text_default = data.get("rules_text", DEFAULT_RULES_TEXT)

        # Folder list
        tk.Label(root, text="Folders to Watch (top-level only):", font=("Segoe UI", 10, "bold"), bg="#ffffff").place(x=20, y=20)
        self.listbox = tk.Listbox(root, height=6, width=80)
        self.listbox.place(x=20, y=50)
        self.listbox.bind("<<ListboxSelect>>", self.on_select)

        tk.Button(root, text="Add/Update", command=self.add_or_update).place(x=620, y=50, width=100)
        tk.Button(root, text="Remove", command=self.remove_selected).place(x=620, y=85, width=100)

        # Path field
        tk.Label(root, text="Path", bg="#ffffff").place(x=20, y=170)
        self.path_var = tk.StringVar()
        tk.Entry(root, textvariable=self.path_var, width=70).place(x=100, y=170)
        tk.Button(root, text="Browse", command=self.browse_path).place(x=620, y=168)

        # Unknown target
        tk.Label(root, text="Unknown target", bg="#ffffff").place(x=20, y=200)
        self.unknown_var = tk.StringVar(value="Other")
        tk.Entry(root, textvariable=self.unknown_var, width=25).place(x=140, y=200)

        # Quiet checkbox
        self.quiet_var = tk.BooleanVar(value=False)
        tk.Checkbutton(root, text="Quiet (errors only)", variable=self.quiet_var, bg="#ffffff").place(x=320, y=198)

        # Rules
        tk.Label(root, text="Custom File Type Rules (optional)", font=("Segoe UI", 10, "bold"), bg="#ffffff").place(x=20, y=240)
        tk.Label(root, text="Format: Category = ext1,ext2,ext3 (one per line)", bg="#ffffff").place(x=20, y=265)
        self.rules_textbox = tk.Text(root, width=90, height=7)
        self.rules_textbox.place(x=20, y=290)
        self.rules_textbox.insert("1.0", self.rules_text_default)

        # Status
        self.status_var = tk.StringVar(value="Ready")
        tk.Label(root, textvariable=self.status_var, anchor="w", width=100, bg="#f0f9ff").place(x=20, y=470)

        # Buttons bottom
        tk.Button(root, text="Start Watchers", bg="#10b981", fg="#ffffff", command=self.start_watchers_clicked).place(x=20, y=510, width=140, height=32)
        tk.Button(root, text="Test Sample Files", bg="#3b82f6", fg="#ffffff", command=self.create_test_files_clicked).place(x=170, y=510, width=140, height=32)
        tk.Button(root, text="Save Settings", bg="#f59e0b", fg="#ffffff", command=self.save_settings_clicked).place(x=320, y=510, width=140, height=32)
        tk.Button(root, text="Exit", bg="#6b7280", fg="#ffffff", command=self.exit_clicked).place(x=470, y=510, width=140, height=32)

        self.refresh_list()
        self.load_first_into_form()
        self.root.protocol("WM_DELETE_WINDOW", self.exit_clicked)

    def refresh_list(self):
        self.listbox.delete(0, tk.END)
        for f in self.folders:
            quiet_tag = " (quiet)" if f.get("quiet") else ""
            self.listbox.insert(tk.END, f"{f.get('path')} -> {f.get('unknown','Other')}{quiet_tag}")

    def load_first_into_form(self):
        if self.folders:
            self.populate_form(self.folders[0])
            self.listbox.selection_set(0)

    def on_select(self, event=None):
        if not self.listbox.curselection():
            return
        idx = self.listbox.curselection()[0]
        self.populate_form(self.folders[idx])

    def populate_form(self, folder):
        self.path_var.set(folder.get("path", ""))
        self.unknown_var.set(folder.get("unknown", "Other"))
        self.quiet_var.set(bool(folder.get("quiet", False)))

    def browse_path(self):
        p = filedialog.askdirectory(title="Select directory to watch")
        if p:
            self.path_var.set(p)

    def add_or_update(self):
        path = self.path_var.get().strip()
        if not path:
            messagebox.showerror("Error", "Path is required.")
            return
        unknown = self.unknown_var.get().strip() or "Other"
        quiet = bool(self.quiet_var.get())
        entry = {"path": path, "unknown": unknown, "quiet": quiet}

        # replace if exists
        replaced = False
        for i, f in enumerate(self.folders):
            if f.get("path") == path:
                self.folders[i] = entry
                replaced = True
                break
        if not replaced:
            self.folders.append(entry)

        self.refresh_list()
        self.save_settings_clicked()

    def remove_selected(self):
        if not self.listbox.curselection():
            return
        idx = self.listbox.curselection()[0]
        self.folders.pop(idx)
        self.refresh_list()
        if self.folders:
            self.listbox.selection_set(0)
            self.populate_form(self.folders[0])
        self.save_settings_clicked()

    def save_settings_clicked(self):
        data = {
            "folders": self.folders,
            "rules_text": self.rules_textbox.get("1.0", tk.END)
        }
        save_settings(data)
        self.status_var.set("Settings saved.")

    def parse_rules(self):
        return parse_rules_text(self.rules_textbox.get("1.0", tk.END))

    def start_watchers_clicked(self):
        if not self.folders:
            messagebox.showerror("Error", "Add at least one folder.")
            return
        rules = self.parse_rules()
        observers = []

        def runner():
            try:
                for f in self.folders:
                    path = Path(f.get("path", "")).expanduser()
                    if not path.exists() or not path.is_dir():
                        self.status_var.set(f"Skip (not found): {path}")
                        continue
                    obs = start_watcher(path, rules, f.get("unknown", "Other"), bool(f.get("quiet", False)))
                    observers.append(obs)
                self.observers = observers
                self.status_var.set("Watchers running. Close window to stop.")
                while any(o.is_alive() for o in observers):
                    time.sleep(0.5)
            except Exception as e:
                self.status_var.set(f"ERROR: {e}")

        t = threading.Thread(target=runner, daemon=True)
        t.start()

    def create_test_files_clicked(self):
        if not self.listbox.curselection():
            messagebox.showerror("Error", "Select a folder first.")
            return
        idx = self.listbox.curselection()[0]
        folder = self.folders[idx]
        path = Path(folder.get("path", ""))
        if not path.exists() or not path.is_dir():
            messagebox.showerror("Error", "Path does not exist or is not a directory.")
            return
        td = create_test_files(path)
        self.status_var.set(f"Test files created in: {td}")
        messagebox.showinfo("Test Files Ready", f"Created in:\n\n{td}\n\nStart the watcher to see them organize!")

    def exit_clicked(self):
        try:
            for obs in self.observers:
                obs.stop()
            for obs in self.observers:
                obs.join(timeout=3)
        except Exception:
            pass
        self.root.destroy()


def main():
    root = tk.Tk()
    App(root)
    root.mainloop()


if __name__ == "__main__":
    main()
