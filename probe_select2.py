src = open(r'Plain Craft Launcher 2/Pages/PageSelectLeft.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 Add_Click 方法和新建文件夹按钮
import re
for m in re.finditer(r'Add_Click|itemAdd|新建.*文件夹|folder.*plus|plus.*folder|lucide/folder', src):
    print(f'{m.start()}: {src[max(0,m.start()-80):m.start()+200]}')
    print('---')
