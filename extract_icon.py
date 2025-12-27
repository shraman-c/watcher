import base64
import re
from pathlib import Path

# Read gui.py and extract base64 icon
with open('gui.py', 'r', encoding='utf-8') as f:
    content = f.read()

# Find the ICON_BASE64_DEFAULT value
match = re.search(r'ICON_BASE64_DEFAULT = "([^"]+)"', content)

if match:
    icon_base64 = match.group(1)
    icon_data = base64.b64decode(icon_base64)
    
    # Save as PNG
    Path('watcher-icon.png').write_bytes(icon_data)
    print(f'✓ Icon extracted: {len(icon_data)} bytes')
    print('✓ Saved as: watcher-icon.png')
    
    # Convert to ICO using Pillow
    try:
        from PIL import Image
        img = Image.open('watcher-icon.png')
        img.save('watcher-icon.ico', format='ICO', sizes=[(256,256), (128,128), (64,64), (48,48), (32,32), (16,16)])
        print('✓ Converted to: watcher-icon.ico')
    except ImportError:
        print('⚠ Pillow not installed. ICO conversion skipped.')
        print('  You can still use the PNG or install Pillow: pip install pillow')
else:
    print('✗ Could not find ICON_BASE64_DEFAULT in gui.py')
