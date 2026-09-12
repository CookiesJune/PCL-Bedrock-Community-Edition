src = open(r'Plain Craft Launcher 2/Pages/PageDownload/Comp/PageComp.xaml.cs'.replace('\\\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 CompProjectRequest 创建或 isBedrock 赋值
import re
for m in re.finditer(r'CompProjectRequest|isBedrock\s*=|new.*CompProjectRequest|LoaderInput', src):
    print(f'at {m.start()}: {src[max(0,m.start()-80):m.start()+200]}')
    print()
