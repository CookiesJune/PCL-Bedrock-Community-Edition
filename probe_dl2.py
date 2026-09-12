src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
# 读 StartDownloadAsync 完整
i = src.find('private async Task StartDownloadAsync')
if i < 0: i = src.find('StartDownloadAsync')
print(src[i:i+2500].replace(chr(13),''))
