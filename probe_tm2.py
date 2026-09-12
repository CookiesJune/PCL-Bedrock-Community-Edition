src = open(r'Plain Craft Launcher 2/Modules/UI/Theme/ThemeManager.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('private static void RefreshBackground')
print(src[i:i+1200])
