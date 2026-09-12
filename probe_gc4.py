src = open(r'Plain Craft Launcher 2/Pages/PageTools/PageToolsGameLink.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 GravityConeCliPath 的所有使用
import re
for m in re.finditer(r'GravityConeCliPath', src):
    print(f'{m.start()}: {src[max(0,m.start()-100):m.start()+200]}')
    print('---')
# 找 File.Exists 检测
for m in re.finditer(r'File.Exists.*Gravity|Gravity.*File.Exists|LabGcCliStatus', src):
    print(f'{m.start()}: {src[max(0,m.start()-80):m.start()+200]}')
    print('---')
