import re
src = open(r'PCL.Core/App/IoC/LifecycleFlow.cs', encoding='utf-8-sig', errors='ignore').read()
for kw in ['MainWindow', 'Loading', '_RunService', 'LifecycleState.Loading', 'Show()', 'Run()']:
    idxs = [m.start() for m in re.finditer(re.escape(kw), src)]
    print(kw, '->', len(idxs), 'at', idxs[:6])
