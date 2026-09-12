src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadInstall.xaml', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('BtnBack')
# 读更多上下文，看父容器
print(src[max(0,i-500):i+200])
