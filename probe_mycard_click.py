src = open(r'Plain Craft Launcher 2/Controls/MyCard.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找点击处理
for kw in ['MyCard_MouseLeftButtonUp', 'IsSwapped', 'SwapControl', 'AniHeight', 'Height =']:
    i = src.find(kw)
    if i >= 0:
        print(f'=== {kw} at {i} ===')
        print(src[max(0,i-50):i+400])
        print()
