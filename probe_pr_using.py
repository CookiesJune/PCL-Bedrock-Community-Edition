src = open(r'Plain Craft Launcher 2/Pages/PageSelectRight.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
print('MyCard count:', src.count('MyCard'))
# using 头
print(src[:800].replace(chr(13),''))
