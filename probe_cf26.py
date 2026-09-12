src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('CompProjectsGet(ModLoader.LoaderTask')
# 在 CompProjectsGet 方法中找 API 请求
import re
for m in re.finditer(r'GetStringAsync|GetAsync|SendAsync|HttpClient|api\.curseforge|modrinth\.com', src[i:i+30000]):
    pos = i + m.start()
    print(f'{pos}: {src[max(0,pos-100):pos+200]}')
    print('---')
