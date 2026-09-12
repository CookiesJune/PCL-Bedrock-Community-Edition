src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModOtherGames.cs', encoding='utf-8-sig', errors='ignore').read()
i = src.find('private static bool IsVersionDir')
print(src[i:i+900].replace(chr(13),''))
