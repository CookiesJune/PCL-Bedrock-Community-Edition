src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 _BuildFromCurseForge 调用位置
import re
for m in re.finditer(r'_BuildFromCurseForge', src):
    if 'private static' not in src[m.start()-20:m.start()]:
        print(f'{m.start()}: {src[max(0,m.start()-150):m.start()+100]}')
        print('---')
