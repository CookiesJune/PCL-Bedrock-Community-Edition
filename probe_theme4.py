src = open(r'PCL.Core/UI/Theme/ThemeService.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找背景色/深浅模式相关
import re
for kw in ['BackgroundColor', 'BackgroundBrush', 'IsDark', 'DarkMode', 'LightMode', '浅色', '深色', 'TetoGreen', '背景']:
    i = src.find(kw)
    if i >= 0:
        print(f'=== {kw} at {i} ===')
        print(src[max(0,i-80):i+300])
        print()
