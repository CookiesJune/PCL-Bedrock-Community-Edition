src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs'.replace('\\\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 在 CompProjectsGet 方法内（87145之后）找 CurseForge 结果解析
import re
# 找 curseForgeAddress 或 GetCurseForgeAddress 调用
for kw in ['GetCurseForgeAddress', 'curseForgeAddress', 'api.curseforge', 'JsonArray.*data', 'foreach.*data', 'curseForgeTotal']:
    j = src.find(kw, 87145)
    if j > 0:
        print(f'=== {kw} at {j} ===')
        print(src[max(0,j-100):j+400])
        print()
