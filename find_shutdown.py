import re, os
pats = ['Shutdown(1)', 'ForceShutdown(1)', '_Exit(1)', 'Exit(1)', 'statusCode = 1', 'ExitWithCode', 'RequestExit']
for root, dirs, files in os.walk('.'):
    for f in files:
        if f.endswith('.cs'):
            p = os.path.join(root, f)
            try:
                src = open(p, encoding='utf-8-sig', errors='ignore').read()
            except: continue
            for kw in pats:
                for m in re.finditer(re.escape(kw), src):
                    ctx = src[max(0,m.start()-180):m.start()+60].replace(chr(13),'')
                    print(p.split('/')[-1], '->', kw, '::', ctx.strip().replace(chr(10),' ')[-200:])
                    print('  ---')
