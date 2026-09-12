import re, os
for root, dirs, files in os.walk('.'):
    for f in files:
        if f.endswith('.cs'):
            p = os.path.join(root, f)
            try:
                src = open(p, encoding='utf-8-sig', errors='ignore').read()
            except: continue
            if 'Exiting program with status' in src or 'status: 1' in src or 'Environment.Exit(1)' in src:
                for m in re.finditer(r'.{200}Exiting program with status.{100}', src):
                    print(p)
                    print('  CTX:', m.group(0).replace(chr(13),' ')[-300:])
                    print('---')
