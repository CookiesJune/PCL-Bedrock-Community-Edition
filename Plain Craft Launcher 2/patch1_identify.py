import io

path = r'Modules/Minecraft/ModOtherGames.cs'
raw = open(path,'rb').read()
src = raw.decode('utf-8-sig').replace('\r\n','\n')
orig = src

old_isdir = """    private static bool IsVersionDir(string dir)
    {
        try
        {
            return File.Exists(Path.Combine(dir, "Minecraft.Windows.exe")) ||
                   File.Exists(Path.Combine(dir, "AppxManifest.xml"));
        }
        catch
        {
            return false;
        }
    }"""
new_isdir = """    private static bool IsVersionDir(string dir)
    {
        try
        {
            return File.Exists(Path.Combine(dir, "Minecraft.Windows.exe")) ||
                   File.Exists(Path.Combine(dir, "AppxManifest.xml")) ||
                   // 小teto定制：目录内含 appx/msixvc/msixbundle 安装包 → 也视为 BE 版本目录（放入即识别）
                   Directory.EnumerateFiles(dir, "*.appx", SearchOption.TopDirectoryOnly).Any() ||
                   Directory.EnumerateFiles(dir, "*.msixvc", SearchOption.TopDirectoryOnly).Any() ||
                   Directory.EnumerateFiles(dir, "*.msixbundle", SearchOption.TopDirectoryOnly).Any() ||
                   Directory.EnumerateFiles(dir, "*.appxbundle", SearchOption.TopDirectoryOnly).Any();
        }
        catch
        {
            return false;
        }
    }"""
assert old_isdir in src, 'IsVersionDir anchor not found'
src = src.replace(old_isdir, new_isdir)

old_bf = """            // 根目录含 AppxManifest / Minecraft.Windows.exe（版本目录本身）
            if (File.Exists(Path.Combine(path, "AppxManifest.xml"))) return true;
            if (File.Exists(Path.Combine(path, "Minecraft.Windows.exe"))) return true;
            // 根目录下 bedrock_versions 的一层版本目录含标记或可执行文件（标准 BE 实例结构）
            var bv = Path.Combine(path, "bedrock_versions");
            if (Directory.Exists(bv))
            {
                foreach (var v in Directory.EnumerateDirectories(bv))
                    if (ContainsMarkerFile(v) ||
                        File.Exists(Path.Combine(v, "Minecraft.Windows.exe")) ||
                        File.Exists(Path.Combine(v, "AppxManifest.xml")))
                        return true;
            }"""
new_bf = """            // 根目录含 AppxManifest / Minecraft.Windows.exe（版本目录本身）
            if (File.Exists(Path.Combine(path, "AppxManifest.xml"))) return true;
            if (File.Exists(Path.Combine(path, "Minecraft.Windows.exe"))) return true;
            // 小teto定制：根目录含 appx/msixvc 安装包 → 视为 BE 版本目录
            if (Directory.EnumerateFiles(path, "*.appx", SearchOption.TopDirectoryOnly).Any() ||
                Directory.EnumerateFiles(path, "*.msixvc", SearchOption.TopDirectoryOnly).Any() ||
                Directory.EnumerateFiles(path, "*.msixbundle", SearchOption.TopDirectoryOnly).Any())
                return true;
            // 根目录下 bedrock_versions 的一层版本目录含标记或可执行文件或安装包（标准 BE 实例结构）
            var bv = Path.Combine(path, "bedrock_versions");
            if (Directory.Exists(bv))
            {
                foreach (var v in Directory.EnumerateDirectories(bv))
                    if (ContainsMarkerFile(v) ||
                        File.Exists(Path.Combine(v, "Minecraft.Windows.exe")) ||
                        File.Exists(Path.Combine(v, "AppxManifest.xml")) ||
                        Directory.EnumerateFiles(v, "*.appx", SearchOption.TopDirectoryOnly).Any() ||
                        Directory.EnumerateFiles(v, "*.msixvc", SearchOption.TopDirectoryOnly).Any())
                        return true;
            }"""
assert old_bf in src, 'IsBedrockFolder anchor not found'
src = src.replace(old_bf, new_bf)

open(path,'wb').write(src.replace('\n','\r\n').encode('utf-8-sig'))
print('ModOtherGames.cs patched OK, changed:', orig != src)
