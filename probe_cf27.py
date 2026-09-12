src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 CurseForge API 请求
import re
for m in re.finditer(r'api\.curseforge\.com|GetStringAsync.*curse|curseforge.*GetString|HttpGet.*curse|NetworkService.*curse', src, re.IGNORECASE):
    print(f'{m.start()}: {src[max(0,m.start()-150):m.start()+200]}')
    print('---')
# 找包含 curseforge 的方法
for m in re.finditer(r'(private|public|internal).*\(', src):
    method_start = m.start()
    method_end = src.find('{', method_start)
    if method_end > 0:
        method_body = src[method_start:min(method_end+5000, len(src))]
        if 'curseforge' in method_body.lower() and ('getstringasync' in method_body.lower() or 'httpclient' in method_body.lower() or 'network' in method_body.lower()):
            print(f'Method at {method_start}: {src[method_start:method_start+100]}')
