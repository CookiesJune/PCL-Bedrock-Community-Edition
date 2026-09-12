src = open(r'Plain Craft Launcher 2/Pages/PageDownload/Comp/PageComp.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('FillBedrockSourceCombo')
# 找方法定义
j = src.rfind('void', 0, i)
print(src[j:j+1500])
