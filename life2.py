import re
src = open(r'PCL.Core/App/IoC/LifecycleFlow.cs', encoding='utf-8-sig', errors='ignore').read()
# 找 statusCode / Exit 方法定义
for m in re.finditer(r'(public|internal|private).*?(RequestExit|ExitRequest|void Exit|Exit\().*?\{', src):
    s = src[m.start():m.end()+600].replace(chr(13),'')
    print('METHOD:', s[:800])
    print('=====')
# 找 statusCode 赋值/使用
for m in re.finditer(r'statusCode[^\n]{0,80}', src):
    print('ST:', m.group(0)[:90])
