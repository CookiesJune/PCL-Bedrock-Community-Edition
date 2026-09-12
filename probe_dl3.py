src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
j = src.find('void AddTaskRow')
print(src[j-100:j+800].replace(chr(13),''))
