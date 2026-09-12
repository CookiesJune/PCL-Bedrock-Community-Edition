import re
src = open(r'PCL.Core/App/IoC/LifecycleFlow.cs', encoding='utf-8-sig', errors='ignore').read()
# CurrentApplication 定义
for m in re.finditer(r'CurrentApplication[^;\n]{0,200}', src):
    print('CA:', m.group(0)[:200])
    print('---')
# Run() 上下文
idx = src.find('statusCode = CurrentApplication.Run()')
print('=== Run ctx ===')
print(src[max(0,idx-2000):idx+500].replace(chr(13),'')[-2400:])
