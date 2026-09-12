src = open(r'Plain Craft Launcher 2/Modules/BedrockLauncher/BedrockLauncher.Core.Utils/ZipExtractor.cs', encoding='utf-8-sig', errors='ignore').read()
import re
for m in re.finditer(r'namespace\s+[\w.]+', src): print('ZipExtractor ns:', m.group(0))
