src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadInstall.xaml', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('CanSwap')
print(src[max(0,i-300):i+1500])
