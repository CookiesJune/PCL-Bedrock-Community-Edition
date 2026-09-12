src = open(r'Plain Craft Launcher 2/Pages/PageTools/PageToolsGameLink.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('void PageLinkLobby_OnPageEnter()')
print(src[i:i+600])
