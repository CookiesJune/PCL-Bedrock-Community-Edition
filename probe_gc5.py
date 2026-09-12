src = open(r'Plain Craft Launcher 2/Pages/PageTools/PageToolsGameLink.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 BE 联机 UI 初始化
import re
for kw in ['_beMode', 'PanGravityCone', 'LabGcCliStatus', 'BtnGcDownload', 'BtnGcStart']:
    idxs = [m.start() for m in re.finditer(kw, src)]
    print(f'{kw}: {len(idxs)} occurrences, first at {idxs[0] if idxs else -1}')
    if idxs:
        # 找第一个出现位置的上下文
        i = idxs[0]
        print(f'  context: {src[max(0,i-50):i+150]}')
# 找 BE 模式切换时的 UI 更新
i = src.find('_beMode = true')
if i >= 0:
    print(f'=== _beMode = true at {i} ===')
    print(src[max(0,i-200):i+500])
