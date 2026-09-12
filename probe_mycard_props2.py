src = open(r'Plain Craft Launcher 2/Controls/MyCard.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
# 找所有 public bool / public double 属性定义
import re
for m in re.finditer(r'public (?:bool|double|string|UIElement|FrameworkElement) \w+', src):
    print(src[m.start():m.start()+80])
