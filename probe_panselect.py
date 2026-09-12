src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadInstall.xaml', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('PanSelect')
# 读 PanSelect 的完整布局
end = src.find('</StackPanel>', i+500)
if end < 0: end = i + 3000
print(src[i:end+20])
