src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadInstall.xaml', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 PanSelect 的完整结构
i = src.find('x:Name=\"PanSelect\"')
# 找 PanSelect 的结束标签
end = src.find('</StackPanel>', i+500)
if end < 0: end = i + 5000
print(src[i:end+20])
