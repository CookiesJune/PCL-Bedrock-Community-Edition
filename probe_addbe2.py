src = open(r'Plain Craft Launcher 2/Pages/PageSelectLeft.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
i = src.find('public void AddBeFolder_Click')
print(src[i:i+1800].replace(chr(13),''))
