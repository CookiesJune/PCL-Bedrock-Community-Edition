src = open(r'PCL.Core/UI/Theme/ThemeService.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 读颜色计算方法（4900-5600）
print(src[4700:5600])
