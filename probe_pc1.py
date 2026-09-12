src = open(r'Plain Craft Launcher 2/Pages/PageDownload/Comp/PageComp.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 FillBedrockSourceCombo 和源切换
import re
for kw in ['FillBedrockSourceCombo', 'IsBedrock', 'SourceCombo', 'ComboSource', 'CurseForge']:
    i = src.find(kw)
    if i >= 0:
        print(f'=== {kw} at {i} ===')
        print(src[max(0,i-50):i+300])
        print()
