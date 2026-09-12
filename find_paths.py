import re, os
for root, dirs, files in os.walk('.'):
    for f in files:
        if f.endswith('.cs'):
            p = os.path.join(root, f)
            try:
                src = open(p, encoding='utf-8-sig', errors='ignore').read()
            except: continue
            if 'SharedLocalData' in src:
                for m in re.finditer(r'SharedLocalData[^\n]{0,150}', src):
                    print(p.split('/')[-1], '::', m.group(0)[:150])
                print('---')
