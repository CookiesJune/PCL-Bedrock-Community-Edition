import re, os
for root, dirs, files in os.walk('.'):
    for f in files:
        if f.endswith('.cs'):
            p = os.path.join(root, f)
            try:
                src = open(p, encoding='utf-8-sig', errors='ignore').read()
            except: continue
            for kw in ['Exiting program', 'Pending logs', 'LastPending', 'Welcome to Plain']:
                if kw in src:
                    print(p, '->', kw)
                    for m in re.finditer(r'.{150}' + re.escape(kw) + r'.{150}', src):
                        print('   CTX:', m.group(0).replace(chr(13),' ')[-250:])
                    print('---')
