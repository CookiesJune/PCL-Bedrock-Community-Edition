src = open(r'Plain Craft Launcher 2/Modules/Network/Loaders/LoaderDownload.cs', encoding='utf-8-sig', errors='ignore').read()
# 读 Progress 相关 400-750
print(src[400:800].replace(chr(13),''))
print('======= Downloaded 相关 =======')
print(src[4000:4500].replace(chr(13),''))
