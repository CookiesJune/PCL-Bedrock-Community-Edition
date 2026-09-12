import re, os
# 找 CurrentApplication.Run 实现
for root, dirs, files in os.walk('.'):
    for f in files:
        if f.endswith('.cs'):
            p = os.path.join(root, f)
            try:
                src = open(p, encoding='utf-8-sig', errors='ignore').read()
            except: continue
            if 'public' in src and 'Run()' in src and ('CurrentApplication' in src or 'class CurrentApplication' in src or 'static class Application' in src):
                print('CANDIDATE:', p)
# 直接读 Program.cs
print('===== Program.cs =====')
src = open(r'Plain Craft Launcher 2/Program.cs', encoding='utf-8-sig', errors='ignore').read()
print(src[:3000])
