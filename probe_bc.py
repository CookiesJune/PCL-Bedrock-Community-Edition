import re
src = open(r'C:\Users\CookieJune\Doubao\chats\2026-09-02\new-chat\PCL2-CE-official\Plain Craft Launcher 2\Modules\BedrockLauncher\BedrockLauncher.Core\BedrockCore.cs'.replace('\\','/'), encoding='utf-8-sig', errors='ignore').read()
# 找 InstallPackageAsync 签名和进度
for m in re.finditer(r'InstallPackageAsync', src):
    s = src[max(0,m.start()-200):m.start()+400].replace(chr(13),'')
    print('===')
    print(s[-500:])
