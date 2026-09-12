src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadInstall.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 BtnBack 可见性设置
import re
for m in re.finditer(r'BtnBack\.(Visibility|IsEnabled|Opacity)', src):
    print(f'{m.start()}: {src[max(0,m.start()-50):m.start()+150]}')
    print('---')
# 找 ExitSelectPage
i = src.find('void ExitSelectPage')
if i >= 0:
    print('=== ExitSelectPage ===')
    print(src[i:i+400])
