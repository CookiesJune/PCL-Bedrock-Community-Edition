src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
i = src.find('private void InstallUwpLoose')
# 找方法结束（下一个 private void / private async）
import re
# 方法体到下一个 方法定义 或 3 个连续 }
j = src.find('private void InstallGdkLoose')
print('Method span:', i, j)
print(src[i:j].replace(chr(13),''))
