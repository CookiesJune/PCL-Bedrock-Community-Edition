src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
for i, m in enumerate(__import__('re').finditer(r'File\.Delete', src)):
    s = src[max(0,m.start()-350):m.start()+150].replace(chr(13),'')
    print(f'--- File.Delete {i} ---')
    print(s.strip()[-420:])
