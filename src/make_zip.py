import zipfile
import os

zip_filename = 'Jellyfin.Plugin.UserRatings.zip'
dll_path = 'src/bin/Release/net8.0/publish/Jellyfin.Plugin.UserRatings.dll'
meta_path = 'src/meta.json'

with zipfile.ZipFile(zip_filename, 'w', zipfile.ZIP_DEFLATED) as z:
    if os.path.exists(dll_path):
        z.write(dll_path, 'Jellyfin.Plugin.UserRatings.dll')
        print(f"Added {dll_path}")
    else:
        print(f"Error: {dll_path} not found")
        
    if os.path.exists(meta_path):
        z.write(meta_path, 'meta.json')
        print(f"Added {meta_path}")
    else:
        print(f"Error: {meta_path} not found")

print(f"Created {zip_filename}")
