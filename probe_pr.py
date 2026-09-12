src = open(r'Plain Craft Launcher 2/Pages/PageSelectRight.xaml.cs', encoding='utf-8-sig', errors='ignore').read()
print('LEN:', len(src))
import re
for kw in ['添加BE', 'AddBe', 'BeFolder', 'BE文件夹', 'ClassifyBe', 'AddBeGroup', 'RefreshBeList', '预览版', 'Preview', 'Bedrock']:
    idxs = [m.start() for m in re.finditer(re.escape(kw), src)]
    print(f'{kw}: {len(idxs)} at {idxs[:6]}')
