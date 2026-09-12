src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs'.replace('\\\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 CompProjectsGet 方法体（从 74102 开始，找方法结束）
i = 74102
# 找方法内的 JSON 解析和结果过滤
# 搜索 categories 字段处理
j = src.find('categories', 74102)
count = 0
while j >= 0 and j < 85000 and count < 10:
    print(f'=== categories at {j} ===')
    print(src[max(0,j-80):j+250])
    print()
    j = src.find('categories', j+1)
    count += 1
