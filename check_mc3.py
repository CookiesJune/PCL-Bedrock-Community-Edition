import re
src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
# 找所有 mcappx 上下文（定位是按钮还是 URL 构造）
for i, m in enumerate(re.finditer(r'mcappx', src)):
    s = src[max(0,m.start()-120):m.start()+120].replace(chr(13),'')
    print(f'--- occurrence {i} ---')
    print(s.strip())
