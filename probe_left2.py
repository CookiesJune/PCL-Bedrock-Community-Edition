src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadLeft.xaml', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 读 BedrockResources 完整定义
i = src.find('ItemBedrockResources')
print(src[i:i+500])
print('===')
# 找整合包项
j = src.find('整合包')
if j < 0:
    j = src.find('Modpack')
if j < 0:
    # 找 Tag= 对应的项
    for m in __import__('re').finditer(r'Title="[^"]*包"', src):
        print(f'Found: {m.group()} at {m.start()}')
else:
    print(src[max(0,j-200):j+200])
