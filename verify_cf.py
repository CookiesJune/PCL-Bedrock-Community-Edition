src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 检查 null 检查
checks = [
    ('gameId=78022', 'gameId=78022' in src),
    ('null check 1 (slug search)', 'if (json is null || json[\"data\"] is not JsonArray dataArray' in src),
    ('null check 2 (filtered slug)', 'if (filteredJson is null || filteredJson[\"data\"] is not JsonArray filteredDatas' in src),
    ('null check 3 (single project)', 'if (json is null || json[\"data\"] is not JsonObject dataObj' in src),
    ('null check 4 (file list)', 'if (response is null || response[\"data\"] is not JsonArray dataArr)' in src),
    ('null check 5 (batch deps)', 'if (response is null || response[\"data\"] is not JsonArray projArr)' in src),
    ('null check 6 (GetListByIds)', 'if (response is null || response[\"data\"] is not JsonArray rawProjectsData)' in src),
]
for name, ok in checks:
    print(f'  {\"OK\" if ok else \"MISSING\"}: {name}')
# 检查主搜索路径的 null 检查
import re
m = re.search(r'var json = ModDownload\.DlModRequest<JsonObject>\(curseForgeUrl\);.*?return;', src, re.DOTALL)
if m:
    print('  OK: main search path has null check')
else:
    print('  CHECK: main search path')
