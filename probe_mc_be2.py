src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs'.replace('\\\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# BE 分类映射完整
i = 73839
print(src[i:i+1200])
print()
# 找 bedrockCategoryFilter 软过滤
j = src.find('bedrockCategoryFilter')
while j >= 0:
    print(f'=== bedrockCategoryFilter at {j} ===')
    print(src[max(0,j-80):j+300])
    print()
    j = src.find('bedrockCategoryFilter', j+1)
