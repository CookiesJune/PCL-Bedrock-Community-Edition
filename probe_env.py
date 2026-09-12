src = open(r'C:\Users\CookieJune\Doubao\chats\2026-09-02\new-chat\PCL2-CE-official\PCL.Core\Utils\OS\EnvironmentInterop.cs'.replace('\\\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('GetSecret')
print(src[max(0,i-200):i+1500])
