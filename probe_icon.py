src = open(r'Plain Craft Launcher 2/Pages/PageSelectRight.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
# iconCombo 707 附近
i = src.find('iconCombo.Items.Add')
print('=== iconCombo ===')
print(src[i-500:i+600].replace(chr(13),''))
