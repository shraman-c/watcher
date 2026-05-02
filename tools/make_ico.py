from PIL import Image

src_path = 'watcher-icon.png'
out_path = 'watcher-icon-fixed.ico'

img = Image.open(src_path).convert('RGBA')
sizes = [(16,16),(32,32),(48,48),(64,64),(128,128),(256,256)]
# Composite onto white background to remove alpha, ensuring BMP layers in ICO
rgb = Image.new('RGB', img.size, (255,255,255))
rgb.paste(img, mask=img.split()[3])
rgb.save(out_path, sizes=sizes)
print(f'Generated {out_path} with sizes: {sizes}')
