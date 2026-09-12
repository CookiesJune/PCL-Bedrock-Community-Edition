src = open(r'Plain Craft Launcher 2/Pages/PageTools/PageToolsGameLink.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 BtnBeLink_Click
i = src.find('BtnBeLink_Click')
if i < 0:
    i = src.find('BtnBeLink')
print(src[max(0,i-50):i+1500])
