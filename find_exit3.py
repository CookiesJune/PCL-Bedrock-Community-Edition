import re, os
# 搜谁调用 Exit(1) / statusCode
targets = {}
for root, dirs, files in os.walk('.'):
    for f in files:
        if f.endswith('.cs'):
            p = os.path.join(root, f)
            try:
                src = open(p, encoding='utf-8-sig', errors='ignore').read()
            except: continue
            for m in re.finditer(r'\.Exit\(\s*(\d|statusCode|status)', src):
                ctx = src[max(0,m.start()-150):m.start()+80].replace(chr(13),'')
                targets.setdefault(p, []).append(ctx)
for p, cs in targets.items():
    print('FILE:', p)
    for c in cs[:5]:
        print('   ', c.strip().replace(chr(10),' ')[-150:])
    print('---')
