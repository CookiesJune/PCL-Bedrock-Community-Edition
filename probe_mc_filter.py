src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs'.replace('\\\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 CompProjectsGet 方法
i = src.find('CompProjectsGet')
print(f'CompProjectsGet at {i}')
# 找 bedrockCategoryFilter 在结果过滤中的使用
j = src.find('bedrockCategoryFilter', 75000)
while j >= 0 and j < 90000:
    print(f'=== at {j} ===')
    print(src[max(0,j-100):j+400])
    print()
    j = src.find('bedrockCategoryFilter', j+1)
