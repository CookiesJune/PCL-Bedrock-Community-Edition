src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadInstall.xaml', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('BtnBack')
print('=== BtnBack in XAML ===')
print(src[max(0,i-100):i+300])
