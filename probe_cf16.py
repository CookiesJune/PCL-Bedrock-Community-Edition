src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 CompProjectsGet 中解析 CurseForge 响应的部分
i = src.find('CompProjectsGet(ModLoader.LoaderTask')
# 找 data/pagination 解析
import re
for m in re.finditer(r'(data|pagination)\]\s*(as|is|\[)', src[i:i+20000]):
    pos = i + m.start()
    print(f'{pos}: {src[max(0,pos-60):pos+120]}')
    print('---')
