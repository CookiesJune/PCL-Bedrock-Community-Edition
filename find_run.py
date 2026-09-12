import re, os
# 找 CurrentApplication 定义
for root, dirs, files in os.walk('.'):
    for f in files:
        if f.endswith('.cs'):
            p = os.path.join(root, f)
            try:
                src = open(p, encoding='utf-8-sig', errors='ignore').read()
            except: continue
            if 'CurrentApplication' in src and ('class' in src):
                for m in re.finditer(r'CurrentApplication[^=;]{0,60}', src):
                    pass
                print('FILE:', p)
# 找 Run() 方法返回 1 的位置
for root, dirs, files in os.walk('.'):
    for f in files:
        if f.endswith('.cs'):
            p = os.path.join(root, f)
            try:
                src = open(p, encoding='utf-8-sig', errors='ignore').read()
            except: continue
            for m in re.finditer(r'return 1;', src):
                ctx = src[max(0,m.start()-200):m.start()+50].replace(chr(13),'')
                print('RET1:', p, '->', ctx.strip().replace(chr(10),' ')[-200:])
