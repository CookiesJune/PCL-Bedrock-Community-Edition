src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('CompProjectsGet(ModLoader.LoaderTask')
# 找 new CompProject 调用
import re
for m in re.finditer(r'new CompProject', src[i:i+30000]):
    pos = i + m.start()
    print(f'{pos}: {src[max(0,pos-120):pos+150]}')
    print('---')
