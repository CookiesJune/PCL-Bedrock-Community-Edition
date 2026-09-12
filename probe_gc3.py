src = open(r'Plain Craft Launcher 2/Pages/PageTools/PageToolsGameLink.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 读 BtnGcDownload_Click 完整方法
i = src.find('private async void BtnGcDownload_Click')
print('=== BtnGcDownload_Click ===')
print(src[i:i+1200])
print()
# 找 PageLinkLobby_OnPageEnter
j = src.find('PageLinkLobby_OnPageEnter')
if j >= 0:
    print('=== PageLinkLobby_OnPageEnter ===')
    print(src[j:j+800])
