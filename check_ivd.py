import re
src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModOtherGames.cs', encoding='utf-8-sig', errors='ignore').read()
for m in re.finditer(r'IsVersionDir', src):
    s = src[max(0,m.start()-100):m.start()+100].replace(chr(13),'')
    print('IVD:', s.strip())
for m in re.finditer(r'appx', src, re.I):
    s = src[max(0,m.start()-150):m.start()+150].replace(chr(13),'')
    print('APPX:', s.strip())
    print('---')
