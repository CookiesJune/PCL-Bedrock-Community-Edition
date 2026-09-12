import re
src = open(r'Plain Craft Launcher 2/Modules/Base/ModBase.cs', encoding='utf-8-sig', errors='ignore').read()
m = re.search(r'enum LoadState\s*\{[^}]*\}', src)
if m: print(m.group(0).replace(chr(13),''))
else:
    # 可能 LoadState 在别处
    for root, dirs, files in __import__('os').walk('.'):
        for f in files:
            if f.endswith('.cs'):
                s = open(__import__('os').path.join(root,f), encoding='utf-8-sig', errors='ignore').read()
                mm = re.search(r'enum LoadState\s*\{[^}]*\}', s)
                if mm:
                    print(__import__('os').path.join(root,f))
                    print(mm.group(0).replace(chr(13),''))
                    break
