src = open(r'Plain Craft Launcher 2/Pages/PageSelectRight.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
import re
# 找添加 BE 文件夹按钮逻辑
for m in re.finditer(r'(AddBe|BeFolder|添加|弹窗|MsgBox|选择文件夹|SelectFolder)', src):
    s = src[max(0,m.start()-120):m.start()+120].replace(chr(13),'')
    line = src[:m.start()].count(chr(10))+1
    print(f'L{line}: {s.strip()[:200]}')
    print('---')
