src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 CurseForge 解析方法的开始
import re
for m in re.finditer(r'(private|public|internal).*CurseForge.*\(', src):
    print(f'{m.start()}: {src[m.start():m.start()+120]}')
# 也找 _FromCurseForge 或 FromCurseForge
for m in re.finditer(r'.*FromCurseForge.*', src):
    print(f'{m.start()}: {src[m.start():m.start()+120]}')
