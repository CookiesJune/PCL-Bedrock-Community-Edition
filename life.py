import re
src = open(r'PCL.Core/App/IoC/LifecycleFlow.cs', encoding='utf-8-sig', errors='ignore').read()
print('=== LifecycleFlow.cs 关键 ===')
for m in re.finditer(r'Exiting program with status', src):
    s = src[max(0,m.start()-1200):m.start()+200]
    print(s.replace(chr(13),'')[-1300:])
    print('======')
