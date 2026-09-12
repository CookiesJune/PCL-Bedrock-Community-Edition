src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
i = src.find('private Grid BuildVersionRow')
if i < 0: i = src.find('BuildVersionRow')
print(src[i:i+2800].replace(chr(13),''))
