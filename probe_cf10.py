src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 CompProjectsGet 方法
i = src.find('public static List<CompProject> CompProjectsGet')
if i < 0:
    i = src.find('CompProjectsGet')
print(f'CompProjectsGet at {i}')
print(src[i:i+200])
