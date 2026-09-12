src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
i = src.find('StartDownloadAsync(v)')
# 从方法体开始读
i = src.find('private async void StartDownloadAsync')
print(src[i:i+5200].replace(chr(13),''))
