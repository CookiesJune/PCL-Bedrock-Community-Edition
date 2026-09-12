src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 CompProjectsGet 中 CurseForge 请求和异常处理
i = src.find('CompProjectsGet(ModLoader.LoaderTask')
# 找 try-catch 和 CurseForge 请求
import re
for m in re.finditer(r'try|catch|CurseForge.*Get|GetStringAsync|curseforge\.com|api\.curseforge', src[i:i+20000]):
    pos = i + m.start()
    print(f'{pos}: {src[max(0,pos-60):pos+120]}')
    print('---')
