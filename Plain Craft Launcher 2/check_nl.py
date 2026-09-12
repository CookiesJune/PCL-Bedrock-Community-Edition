path = r'Modules/Minecraft/ModOtherGames.cs'
raw = open(path,'rb').read()
crlf = raw.count(b'\r\n'); lf = raw.count(b'\n'); cr = raw.count(b'\r')
print('CRLF:', crlf, 'LF:', lf, 'CR:', cr)
