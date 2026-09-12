src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModOtherGames.cs', encoding='utf-8-sig', errors='ignore').read()
idx = src.find('FindBedrockVersionFolders')
print(src[idx-400:idx+1600].replace(chr(13),''))
