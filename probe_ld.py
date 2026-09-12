import re
src = open(r'Plain Craft Launcher 2/Modules/Network/Loaders/LoaderDownload.cs', encoding='utf-8-sig', errors='ignore').read()
print('LEN:', len(src))
for kw in ['Progress', '进度', 'Downloaded', 'event', 'OnStateChanged', 'percent', 'Percent', 'ProgressChanged']:
    idxs = [m.start() for m in re.finditer(re.escape(kw), src)]
    print(f'{kw}: {len(idxs)} at {idxs[:6]}')
