src = open(r'PCL.Core/Utils/Exts/StringExt.cs', encoding='utf-8-sig', errors='ignore').read().replace(chr(13),'')
i = src.find('ReplaceNullOrEmpty')
if i < 0:
    # 找其他文件
    import subprocess
    r = subprocess.run(['grep', '-rl', 'ReplaceNullOrEmpty', 'PCL.Core'], capture_output=True, text=True)
    print(r.stdout)
else:
    print(src[max(0,i-50):i+300])
