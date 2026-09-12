src = open(r'Plain Craft Launcher 2/Controls/MyCard.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# IsSwapped 依赖属性
i = src.find('IsSwappedProperty')
print('=== IsSwappedProperty ===')
print(src[max(0,i-50):i+600])
print()
# SwapedHeight 依赖属性
j = src.find('SwapedHeightProperty')
print('=== SwapedHeightProperty ===')
print(src[max(0,j-50):j+400])
