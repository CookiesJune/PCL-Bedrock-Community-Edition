src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 CompFile 构造函数
import re
for m in re.finditer(r'public CompFile\(', src):
    print(f'{m.start()}: {src[m.start():m.start()+1500]}')
    print('===')
