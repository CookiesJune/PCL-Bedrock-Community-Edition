src = open(r'Plain Craft Launcher 2/Pages/PageSelectLeft.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 读添加BE文件夹按钮
i = src.find('添加 BE 文件夹')
print('=== 添加BE文件夹 ===')
print(src[max(0,i-100):i+400])
print()
# 找新建.minecraft文件夹按钮
j = src.find('新建')
if j < 0:
    j = src.find('NewFolder')
if j < 0:
    # 找 AddFolder 按钮创建
    for m in __import__('re').finditer(r'new MyIconButton|new MyButton|Title.*文件夹', src):
        print(f'Found at {m.start()}: {src[m.start():m.start()+150]}')
else:
    print('=== 新建文件夹 ===')
    print(src[max(0,j-100):j+300])
