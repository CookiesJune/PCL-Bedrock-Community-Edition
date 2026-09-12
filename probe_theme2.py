src = open(r'PCL.Core/UI/Theme/ThemeService.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 FestiveRed 和 PineBrown
import re
for kw in ['FestiveRed', 'PineBrown', '喜庆红', '松木棕']:
    i = src.find(kw)
    if i >= 0:
        print(f'=== {kw} at {i} ===')
        print(src[max(0,i-100):i+400])
        print()
