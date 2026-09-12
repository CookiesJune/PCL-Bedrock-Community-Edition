src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 读 CompFile 构造函数的第二个分支（CurseForge API 数据）
i = src.find('public CompFile(JsonObject data, CompType defaultType)')
print(src[i+1500:i+3000])
