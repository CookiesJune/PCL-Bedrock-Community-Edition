path = r'PCL.Core/App/Essentials/SingleInstanceService.cs'
raw = open(path,'rb').read()
src = raw.decode('utf-8-sig').replace('\r\n','\n')
orig = src
old = """        try
        {
            var stream = File.Open(_LockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);"""
new = """        try
        {
            // 小teto定制：确保单例锁父目录存在（修复 %LOCALAPPDATA%\\PCLCE 目录缺失导致启动即退出的问题）
            var lockDir = Path.GetDirectoryName(_LockFilePath);
            if (!string.IsNullOrEmpty(lockDir)) Directory.CreateDirectory(lockDir);
            var stream = File.Open(_LockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);"""
assert old in src, 'anchor not found'
src = src.replace(old, new)
open(path,'wb').write(src.replace('\n','\r\n').encode('utf-8-sig'))
print('SingleInstanceService patched OK, changed:', orig != src)
