src = open(r'Plain Craft Launcher 2/Pages/PageSetup/PageSetupUI.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找主题色列表
import re
for kw in ['喜庆红', '松木棕', '元气橙', '水墨绿', '天空蓝', '梦幻紫', 'ColorTheme', '主题色']:
    i = src.find(kw)
    if i >= 0:
        print(f'=== {kw} at {i} ===')
        print(src[max(0,i-80):i+200])
        print()
