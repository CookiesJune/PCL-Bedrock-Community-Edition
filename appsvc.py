import re
src = open(r'PCL.Core/App/Essentials/ApplicationService.cs', encoding='utf-8-sig', errors='ignore').read()
print('LEN:', len(src))
for m in re.finditer(r'public.*?Run\(.*?\{', src):
    s = src[m.start():m.start()+1500].replace(chr(13),'')
    print('=== Run() ===')
    print(s[:1500])
