src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 ToCompItem 方法
import re
for m in re.finditer(r'public.*ToCompItem', src):
    print(f'{m.start()}: {src[m.start():m.start()+100]}')
# 找 ToCompItem 中 BE 相关
i = src.find('ToCompItem')
if i > 0:
    # 找方法中的 isBedrock
    bedrock_in_method = src[i:i+5000].find('isBedrock')
    if bedrock_in_method > 0:
        pos = i + bedrock_in_method
        print(f'BE in ToCompItem at {pos}: {src[max(0,pos-100):pos+300]}')
    else:
        print('No isBedrock in ToCompItem (first 5000 chars)')
