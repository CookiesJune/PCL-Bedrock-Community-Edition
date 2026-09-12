src = open(r'Plain Craft Launcher 2/Pages/PageDownload/Comp/PageComp.xaml.cs'.replace('\\\\','/'), encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('private void FillBedrockSourceCombo')
if i < 0: i = src.find('void FillBedrockSourceCombo')
print(src[i:i+1500])
