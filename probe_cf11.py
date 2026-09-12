src = open(r'Plain Craft Launcher 2/Modules/Minecraft/ModComp.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 bedrockCategoryFilter 软过滤
import re
for m in re.finditer(r'bedrockCategoryFilter', src):
    print(f'{m.start()}: {src[max(0,m.start()-100):m.start()+200]}')
    print('---')
