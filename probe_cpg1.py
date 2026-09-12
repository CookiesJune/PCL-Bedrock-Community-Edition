src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs'.replace('\\\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 读 CompProjectsGet 方法（87145 开始，4000 字符）
print(src[87145:91500])
