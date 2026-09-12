src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadInstall.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('BtnBack_Click')
if i < 0:
    i = src.find('BtnBack')
print(src[max(0,i-50):i+500])
