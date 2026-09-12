path = r'Pages/PageDownload/PageDownloadBedrock.xaml.cs'
raw = open(path,'rb').read()
src = raw.decode('utf-8-sig').replace('\r\n','\n')
orig = src
old = r"""HintInstalled.Text = "已用浏览器打开 mcappx 版本页。下载完成后把 .appx 文件放进版本文件夹（bedrock_versions\版本名\）即可识别。";"""
new = r"""HintInstalled.Text = "已用浏览器打开 mcappx 版本页。下载完成后把 .appx 文件放进版本文件夹（bedrock_versions 下的版本目录）即可识别。";"""
assert old in src, 'escape anchor not found'
src = src.replace(old, new)
open(path,'wb').write(src.replace('\n','\r\n').encode('utf-8-sig'))
print('escape fixed:', orig != src)
