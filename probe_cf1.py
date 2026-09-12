src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 CurseForge 解析相关
import re
for kw in ['curseforge', 'CurseForge', 'logo', 'Logo', 'thumbnail', 'Thumbnail', 'GetCurseForgeAddress']:
    idxs = [m.start() for m in re.finditer(kw, src)]
    if idxs:
        print(f'{kw}: {len(idxs)} occurrences, first at {idxs[0]}')
