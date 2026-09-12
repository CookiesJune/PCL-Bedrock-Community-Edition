src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadLeft.xaml', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 ItemBedrockResources 和 整合包 项
import re
for kw in ['ItemBedrockResources', '整合包', 'ModPack', 'ItemModPack']:
    i = src.find(kw)
    if i >= 0:
        print(f'=== {kw} at {i} ===')
        print(src[max(0,i-200):i+300])
        print()
