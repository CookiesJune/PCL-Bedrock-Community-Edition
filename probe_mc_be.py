src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs'.replace('\\\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# BE 搜索完整逻辑
i = src.find('if (isBedrock)')
# 找第一个在搜索URL构建附近的
while i >= 0 and i < 75000:
    print(f'=== at {i} ===')
    print(src[max(0,i-100):i+800])
    print()
    i = src.find('if (isBedrock)', i+1)
    if i > 75000: break
