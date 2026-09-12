src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 读 GetCurseForgeAddress
i = src.find('public string GetCurseForgeAddress()')
print(src[i:i+1200])
