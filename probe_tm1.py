src = open(r'Plain Craft Launcher 2/Modules/UI/Theme/ThemeManager.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找背景色和主题相关
import re
for kw in ['TetoGreen', 'FestiveRed', 'PineBrown', 'BackgroundColor', 'Background', 'Gradient', '淡绿', '深绿', 'IsDark']:
    i = src.find(kw)
    if i >= 0:
        print(f'=== {kw} at {i} ===')
        print(src[max(0,i-80):i+350])
        print()
