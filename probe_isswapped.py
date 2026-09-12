src = open(r'Plain Craft Launcher 2/Controls/MyCard.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('public bool IsSwapped')
print(src[i:i+1500])
