src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
i = src.find('private void InstallGdkLoose')
print(src[i:i+3200].replace(chr(13),''))
