src = open(r'Plain Craft Launcher 2/Pages/PageSelectLeft.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('public void Rename_Click')
print(src[i:i+1200])
