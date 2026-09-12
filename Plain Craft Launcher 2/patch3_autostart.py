path = r'Modules/Minecraft/ModOtherGames.cs'
raw = open(path,'rb').read()
src = raw.decode('utf-8-sig').replace('\r\n','\n')
orig = src

# A) LaunchBedrockInstance: 版本目录只有 .appx 时自动解压注册
old_launch = """            target ??= FindBedrockVersionFolders(folder).FirstOrDefault();
            if (target is not null)
            {
                var exe = Path.Combine(target, "Minecraft.Windows.exe");
                if (File.Exists(exe))
                {"""
new_launch = """            target ??= FindBedrockVersionFolders(folder).FirstOrDefault();
            if (target is not null)
            {
                var exe = Path.Combine(target, "Minecraft.Windows.exe");
                if (!File.Exists(exe))
                {
                    // 小teto定制：版本目录只有 .appx 安装包（未解包）→ 自动解压注册后启动（放入即识别即可用）
                    if (Directory.EnumerateFiles(target, "*.appx", SearchOption.TopDirectoryOnly).Any() ||
                        Directory.EnumerateFiles(target, "*.msixbundle", SearchOption.TopDirectoryOnly).Any() ||
                        Directory.EnumerateFiles(target, "*.appxbundle", SearchOption.TopDirectoryOnly).Any())
                    {
                        var autoErr = TryAutoInstallAppx(target);
                        if (!string.IsNullOrEmpty(autoErr)) return autoErr;
                    }
                }
                if (File.Exists(exe))
                {"""
assert old_launch in src, 'LaunchBedrockInstance anchor not found'
src = src.replace(old_launch, new_launch)

# B) 新增 TryAutoInstallAppx 方法（放在 IsGdkStructure 前）
old_isgdk = """    private static bool IsGdkStructure(string dir)"""
new_isgdk = """    /// <summary>
    ///     小teto定制：版本目录内只有 .appx 安装包（未解包）时，自动解压为 loose 结构并注册，返回空串表示成功。
    /// </summary>
    public static string TryAutoInstallAppx(string dir)
    {
        try
        {
            var pkg = Directory.EnumerateFiles(dir, "*.appx", SearchOption.TopDirectoryOnly).FirstOrDefault()
                   ?? Directory.EnumerateFiles(dir, "*.msixbundle", SearchOption.TopDirectoryOnly).FirstOrDefault()
                   ?? Directory.EnumerateFiles(dir, "*.appxbundle", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (pkg is null) return "未找到 .appx 安装包。";
            if (!File.Exists(Path.Combine(dir, "AppxManifest.xml")))
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(pkg, dir);
                var sig = Path.Combine(dir, "AppxSignature.p7x");
                if (File.Exists(sig))
                    try { File.Delete(sig); } catch { }
                WriteBedrockMarker(dir);
            }
            var manifest = Path.Combine(dir, "AppxManifest.xml");
            if (!File.Exists(manifest)) return "解压后缺少 AppxManifest.xml，无法注册。";
            if (!IsDeveloperMode())
            {
                Process.Start(new ProcessStartInfo("ms-settings:developers") { UseShellExecute = true });
                return "需要开启 Windows 开发者模式才能注册 UWP 包（设置 → 开发者选项）。已为你打开设置页。";
            }
            var regOk = RegisterLooseAppx(manifest);
            if (!regOk) return "UWP 包注册失败，请确认已开启开发者模式后重试。";
            // 注册成功后删除源包
            try { if (File.Exists(pkg)) File.Delete(pkg); } catch { }
            return "";
        }
        catch (Exception ex)
        {
            return "自动安装失败：" + ex.Message;
        }
    }

    private static bool IsGdkStructure(string dir)"""
assert old_isgdk in src, 'IsGdkStructure anchor not found'
src = src.replace(old_isgdk, new_isgdk)

open(path,'wb').write(src.replace('\n','\r\n').encode('utf-8-sig'))
print('ModOtherGames.cs patched OK (auto-install), changed:', orig != src)
