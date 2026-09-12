src = open(r'PCL.Core/App/ConfigEnums.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('ColorTheme')
print(src[max(0,i-50):i+1500])
