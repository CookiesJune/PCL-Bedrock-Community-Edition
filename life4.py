import re
src = open(r'PCL.Core/App/IoC/LifecycleFlow.cs', encoding='utf-8-sig', errors='ignore').read()
# CurrentApplication getter 定义
m = re.search(r'CurrentApplication\s*\{', src)
if m:
    print('=== CurrentApplication getter ===')
    print(src[m.start()-300:m.start()+800].replace(chr(13),''))
# Run 调用处完整方法
for m in re.finditer(r'\.Run\(\)', src):
    # 找所属方法
    seg = src[max(0,m.start()-2500):m.start()+200]
    print('=== Run caller ===')
    print(seg.replace(chr(13),'')[-2200:])
    print('########')
