src = open(r'C:\Users\CookieJune\Doubao\chats\2026-09-02\new-chat\PCL2-CE-official\Plain Craft Launcher 2\Modules\Minecraft\ModOtherGames.cs'.replace('\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('class BedrockVersion')
print(src[i:i+1600])
