import re, os
for f in ['Modules/Minecraft/ModOtherGames.cs','Pages/PageDownload/PageDownloadBedrock.xaml.cs','Pages/PageSelectRight.xaml.cs']:
    print('FILE:', f, os.path.getsize(f))
src = open(r'Modules/Minecraft/ModOtherGames.cs', encoding='utf-8-sig').read()
print('TOTAL LINES:', src.count(chr(10)))
for m in re.finditer(r'(internal|public|private|protected)\s+[\w<>\[\],\.\? ]+?\b(\w+)\s*\(', src):
    line = src[:m.start()].count(chr(10))+1
    print(line, m.group(1), m.group(2))
