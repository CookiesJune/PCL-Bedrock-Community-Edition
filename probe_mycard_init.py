src = open(r'Plain Craft Launcher 2/Controls/MyCard.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 Init 方法
i = src.find('private void Init()')
if i < 0: i = src.find('void Init()')
print(src[i:i+3000])
