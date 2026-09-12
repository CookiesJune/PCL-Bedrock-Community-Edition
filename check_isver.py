src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModOtherGames.cs', encoding='utf-8-sig', errors='ignore').read()
i = src.find('public static bool IsVersionDir')
print(src[i-200:i+1200].replace(chr(13),''))
