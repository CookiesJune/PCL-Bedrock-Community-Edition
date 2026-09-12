src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadInstall.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 BtnBack 的所有操作
import re
for m in re.finditer(r'BtnBack', src):
    print(f'{m.start()}: {src[max(0,m.start()-80):m.start()+120]}')
    print('---')
