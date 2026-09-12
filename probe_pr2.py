src = open(r'Plain Craft Launcher 2/Pages/PageSelectRight.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
# 读 BE 版本折叠逻辑 18000-21600
print(src[18000:21600].replace(chr(13),''))
