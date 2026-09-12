src = open(r'Plain Craft Launcher 2/Pages/PageSelectLeft.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 Rename 处理
import re
for m in re.finditer(r'case "Rename"|Rename.*Click|Menu_Click.*Rename|private.*Rename', src):
    print(f'{m.start()}: {src[max(0,m.start()-50):m.start()+400]}')
    print('---')
