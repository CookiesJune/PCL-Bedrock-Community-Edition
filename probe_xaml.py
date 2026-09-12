src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml', encoding='utf-8-sig', errors='ignore').read()
i = src.find('PanTasks')
print(src[max(0,i-1200):i+400].replace(chr(13),''))
