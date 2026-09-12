src = open(r'Plain Craft Launcher 2/Pages/PageTools/PageToolsGameLink.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 GravityCone 相关
import re
for m in re.finditer(r'GravityCone|gravitycone|gravity_cone', src, re.IGNORECASE):
    print(f'{m.start()}: {src[max(0,m.start()-80):m.start()+200]}')
    print('---')
