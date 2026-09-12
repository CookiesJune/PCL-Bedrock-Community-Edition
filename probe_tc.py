src = open(r'Plain Craft Launcher 2/Pages/PageSetup/PageSetupUI.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('ThemeColors =>')
print(src[i:i+500])
