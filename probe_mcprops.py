src = open(r'Plain Craft Launcher 2/Controls/MyCard.cs', encoding='utf-8-sig', errors='ignore').read()
import re
for prop in ['IsSwapped', 'CanSwap', 'UseAnimation']:
    n = len(re.findall(re.escape(prop), src))
    print(f'{prop}: {n} occurrences')
