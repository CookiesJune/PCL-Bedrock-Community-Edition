src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs'.replace('\\\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 读 CompProjectsGet 结果解析（75105 之后 3000 字符）
print(src[75105:78500])
