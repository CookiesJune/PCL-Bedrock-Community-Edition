src = open(r'Plain Craft Launcher 2/Pages/PageDownload/PageDownloadBedrock.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
# mcappx 按钮渲染（第2处 mcappx 之后）
i = src.find('mcappx')
i2 = src.find('mcappx', i+1)
print('=== mcappx 按钮渲染 ===')
print(src[i2-1000:i2+600].replace(chr(13),'')[-1400:])
