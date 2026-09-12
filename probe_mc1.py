src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs'.replace('\\\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
import re
for m in re.finditer(r'IsBedrock|Bedrock|gameId|GameId|432|minecraft-bedrock', src):
    print(f'at {m.start()}: {src[max(0,m.start()-60):m.start()+150]}')
    print()
