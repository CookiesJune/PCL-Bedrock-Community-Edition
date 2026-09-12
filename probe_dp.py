src = open(r''.replace('\\','/'), encoding='utf-8-sig', errors='ignore').read()
import re
for m in re.finditer(r'namespace\s+[\w.]+|class DecompressProgress', src):
    print(m.group(0))
