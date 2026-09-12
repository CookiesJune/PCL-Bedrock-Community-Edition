src = open(r'Plain Craft Launcher 2/Pages/PageDownload/Comp/PageComp.xaml.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找 FillBedrockSourceCombo 方法定义
import re
for m in re.finditer(r'(private|public|void).*FillBedrockSourceCombo', src):
    print(f'{m.start()}: {src[m.start():m.start()+800]}')
