src = open(r'Plain Craft Launcher 2/Pages/PageSetup/PageSetupUI.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找主题色相关
import re
for m in re.finditer(r'(ColorTheme|ThemeColor|主题|配色|TetoGreen|FestiveRed|PineBrown|VitalityOrange|DreamPurple|SkyBlue)', src):
    print(f'{m.start()}: {src[max(0,m.start()-60):m.start()+150]}')
    print('---')
