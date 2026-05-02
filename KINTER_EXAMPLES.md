# Kinter Dynamic Parameters - Practical Examples

## Example 1: Dynamically Change Button Actions

```python
from gui import WatcherGUI
import tkinter as tk

# Create GUI
gui = WatcherGUI()

# Original action
def action_v1():
    print("Version 1 of the action")

# New action
def action_v2():
    print("Version 2 - Updated!")

# Get button component
start_btn = gui.components['start_btn']

# Change command at runtime
def switch_action():
    start_btn.set_command(action_v2)

# Create button to switch
switch_btn = tk.Button(gui.window.widget, text="Switch Action", command=switch_action)
switch_btn.pack()

gui.window.mainloop()
```

## Example 2: Dynamic Form Validation

```python
from gui import WatcherGUI
from pathlib import Path

gui = WatcherGUI()

# Get form components
path_entry = gui.components['path_entry']
unknown_entry = gui.components['unknown_entry']

def validate_and_add():
    path = path_entry.get().strip()
    
    # Validate path exists
    if not Path(path).exists():
        # Show error and clear field
        from kinter import KinterDialog
        KinterDialog.show_error("Error", f"Path does not exist: {path}", gui.window.widget)
        path_entry.set("")
        return
    
    # Path valid - proceed with add
    gui.on_add_folder()

# Replace the add button command
add_btn = gui.components['add_btn']
add_btn.set_command(validate_and_add)

gui.window.mainloop()
```

## Example 3: Theme Switching

```python
from gui import WatcherGUI
from kinter import KinterButton
import tkinter as tk

gui = WatcherGUI()

# Define themes
THEMES = {
    'dark': {'fg': 'white', 'bg': '#333'},
    'light': {'fg': 'black', 'bg': '#fff'},
    'blue': {'fg': 'white', 'bg': '#0066cc'},
}

def apply_theme(theme_name):
    """Apply theme to all buttons."""
    theme = THEMES.get(theme_name)
    if not theme:
        return
    
    for comp_id, component in gui.components.items():
        if isinstance(component, KinterButton):
            component.update_config(**theme)

# Create theme switcher
theme_frame = tk.Frame(gui.window.widget)
theme_frame.pack(fill=tk.X, padx=8, pady=4)

for theme_name in THEMES.keys():
    tk.Button(theme_frame, text=f"{theme_name.title()} Theme", 
              command=lambda t=theme_name: apply_theme(t)).pack(side=tk.LEFT, padx=2)

gui.window.mainloop()
```

## Example 4: Multi-Step Form with Dynamic Steps

```python
from gui import WatcherGUI
from kinter import KinterButton

gui = WatcherGUI()

# Get form components
path_entry = gui.components['path_entry']
unknown_entry = gui.components['unknown_entry']
quiet_check = gui.components['quiet_check']

step = 1

def step_1():
    """Step 1: Get path"""
    path_entry.set("")
    unknown_entry.set("Other")
    quiet_check.set(False)
    
    add_btn = gui.components['add_btn']
    add_btn.update_config(text="Next: Unknown Target")
    add_btn.set_command(step_2)

def step_2():
    """Step 2: Get unknown target"""
    path = path_entry.get()
    if not path:
        from kinter import KinterDialog
        KinterDialog.show_error("Error", "Path required", gui.window.widget)
        return
    
    add_btn = gui.components['add_btn']
    add_btn.update_config(text="Next: Quiet Mode")
    add_btn.set_command(step_3)

def step_3():
    """Step 3: Set quiet mode and add"""
    add_btn = gui.components['add_btn']
    add_btn.update_config(text="Add")
    add_btn.set_command(gui.on_add_folder)
    
    # Actually add the folder
    gui.on_add_folder()
    
    # Reset
    step_1()

# Start with step 1
step_1()

gui.window.mainloop()
```

## Example 5: Real-time Rule Validation

```python
from gui import WatcherGUI
import json

gui = WatcherGUI()

# Get rules text component
rules_text = gui.components['rules_text']

def validate_rules(event=None):
    """Validate rules as user types."""
    try:
        content = rules_text.get()
        
        # Try to parse as JSON
        if content.strip().startswith('{'):
            json.loads(content)
            status_color = "green"
        else:
            # Try to parse as key=value format
            for line in content.split('\\n'):
                if line.strip() and '=' not in line:
                    raise ValueError(f"Invalid line: {line}")
            status_color = "green"
        
        gui.status_var.set("Rules: Valid ✓")
    except Exception as e:
        gui.status_var.set(f"Rules: Invalid - {str(e)[:50]}")

# Bind validation to rules text changes
original_handler = gui.on_rules_modified
def wrapped_handler(evt=None):
    original_handler(evt)
    validate_rules()

# Replace the handler
import kinter
rules_widget = rules_text.widget
rules_widget.bind("<<Modified>>", wrapped_handler)

gui.window.mainloop()
```

## Example 6: Conditional UI Visibility

```python
from gui import WatcherGUI
import tkinter as tk

gui = WatcherGUI()

# Get checkbox
quiet_check = gui.components['quiet_check']

# Get unknown target entry
unknown_entry = gui.components['unknown_entry']

def toggle_unknown_visibility():
    """Show/hide unknown target based on quiet mode."""
    if quiet_check.get():
        # Hide unknown entry
        if unknown_entry.widget:
            unknown_entry.widget.pack_forget()
    else:
        # Show unknown entry
        if unknown_entry.widget:
            unknown_entry.widget.pack()

# Bind to checkbox
quiet_check.bind_event("toggle", lambda: toggle_unknown_visibility())

# Also handle manual checkbox clicks
if quiet_check.widget:
    quiet_check.widget.configure(command=toggle_unknown_visibility)

gui.window.mainloop()
```

## Example 7: Dynamic Folder Actions

```python
from gui import WatcherGUI
from pathlib import Path

gui = WatcherGUI()

def open_selected_folder():
    """Open selected folder in file explorer."""
    import subprocess
    import platform
    
    folder_tree = gui.components['folder_tree']
    selection = folder_tree.selection()
    
    if not selection:
        return
    
    folder_idx = int(selection[0])
    folder_path = gui.folders[folder_idx]['path']
    
    if Path(folder_path).exists():
        if platform.system() == 'Windows':
            subprocess.Popen(f'explorer "{folder_path}"')
        elif platform.system() == 'Darwin':
            subprocess.Popen(['open', folder_path])
        else:
            subprocess.Popen(['xdg-open', folder_path])

def stats_selected_folder():
    """Show statistics for selected folder."""
    from kinter import KinterDialog
    
    folder_tree = gui.components['folder_tree']
    selection = folder_tree.selection()
    
    if not selection:
        return
    
    folder_idx = int(selection[0])
    folder_path = gui.folders[folder_idx]['path']
    path_obj = Path(folder_path)
    
    if path_obj.exists():
        files = list(path_obj.glob('**/*'))
        file_count = len([f for f in files if f.is_file()])
        folder_count = len([f for f in files if f.is_dir()])
        
        msg = f"Files: {file_count}\\nFolders: {folder_count}"
        KinterDialog.show_info("Folder Stats", msg, gui.window.widget)

# Create context menu
folder_tree = gui.components['folder_tree'].widget

def right_click(event):
    """Show context menu on right-click."""
    menu = tk.Menu(gui.window.widget, tearoff=0)
    menu.add_command(label="Open Folder", command=open_selected_folder)
    menu.add_command(label="Show Stats", command=stats_selected_folder)
    
    try:
        menu.tk_popup(event.x_root, event.y_root)
    finally:
        menu.grab_release()

folder_tree.bind("<Button-3>", right_click)

gui.window.mainloop()
```

## Example 8: Batch Parameter Updates

```python
from gui import WatcherGUI
from kinter import KinterButton, KinterEntry, KinterCheckbutton

gui = WatcherGUI()

def apply_settings_template(template_name):
    """Apply predefined settings templates."""
    templates = {
        'default': {
            'unknown_entry': 'Other',
            'quiet_check': False,
        },
        'quiet': {
            'unknown_entry': 'Misc',
            'quiet_check': True,
        },
        'strict': {
            'unknown_entry': 'Unorganized',
            'quiet_check': False,
        },
    }
    
    if template_name not in templates:
        return
    
    template = templates[template_name]
    
    # Apply all template values
    for component_id, value in template.items():
        if component_id in gui.components:
            component = gui.components[component_id]
            if isinstance(component, (KinterEntry, KinterCheckbutton)):
                component.set(value)

# Create template buttons
templates = ['default', 'quiet', 'strict']
for template_name in templates:
    btn = tk.Button(gui.window.widget, text=f"Apply {template_name.title()} Template",
                   command=lambda t=template_name: apply_settings_template(t))
    btn.pack(fill=tk.X, padx=8, pady=2)

gui.window.mainloop()
```

## Usage Notes

1. **Always check if widget exists** before updating:
   ```python
   if component.widget and component.widget.winfo_exists():
       component.update_config(...)
   ```

2. **Store component references** for quick access:
   ```python
   my_entry = gui.components['path_entry']
   # Reuse my_entry instead of looking it up again
   ```

3. **Chain operations** for cleaner code:
   ```python
   entry = gui.components['path_entry']
   entry.set("value")
   gui.save_settings()
   ```

4. **Test dynamically updated handlers** thoroughly:
   ```python
   button = gui.components['add_btn']
   button.set_command(new_func)  # Function must be defined and valid
   ```

5. **Use meaningful component IDs** for maintainability:
   ```python
   # Good
   'add_folder_btn', 'unknown_target_entry'
   
   # Avoid
   'btn1', 'entry2'
   ```
