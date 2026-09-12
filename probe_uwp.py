src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
i = src.find('private void InstallUwpLoose')
print('=== InstallUwpLoose ===')
print(src[i:i+2600].replace(chr(13),''))
