src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
i = src.find('mcBtn = new MyButton')
print(src[i-300:i+1500].replace(chr(13),''))
