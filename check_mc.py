src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
# 找 mcappx
import re
for kw in ['mcappx', 'McAppxVersionPage', 'InstallGdkLoose', 'InstallUwpLoose', 'File.Delete']:
    n = src.count(kw)
    print(f'{kw}: {n} occurrences')
# 打印 mcappx 按钮代码
i = src.find('mcappx')
print('=== mcappx 上下文 ===')
print(src[i-800:i+800].replace(chr(13),''))
