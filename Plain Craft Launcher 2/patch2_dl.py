path = r'Pages/PageDownload/PageDownloadBedrock.xaml.cs'
raw = open(path,'rb').read()
src = raw.decode('utf-8-sig').replace('\r\n','\n')
orig = src

# A) BuildVersionRow: 加 mcappx 按钮
old_row = """        var btn = new MyButton
        {
            Text = "下载",
            MinWidth = 80,
            Margin = new Thickness(10, 0, 0, 0),
        };
        btn.Click += (_, _) =>
        {
            selectedVersion = v;
            StartDownloadAsync(v);
        };
        Grid.SetColumn(btn, 2);
        row.Children.Add(btn);"""
new_row = """        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal };
        var btn = new MyButton
        {
            Text = "下载",
            MinWidth = 80,
        };
        btn.Click += (_, _) =>
        {
            selectedVersion = v;
            StartDownloadAsync(v);
        };
        btnPanel.Children.Add(btn);
        // 小teto定制：从 mcappx 版本库用浏览器下载（UWP 旧版等官方源已关闭的包）
        var mcBtn = new MyButton
        {
            Text = "mcappx",
            MinWidth = 84,
            Margin = new Thickness(8, 0, 0, 0),
        };
        mcBtn.Click += (_, _) =>
        {
            try
            {
                var url = McAppxVersionPage(v.Version);
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                HintInstalled.Text = "已用浏览器打开 mcappx 版本页。下载完成后把 .appx 文件放进版本文件夹（bedrock_versions\\版本名\\）即可识别。";
            }
            catch (Exception ex)
            {
                HintInstalled.Text = "打开 mcappx 页面失败：" + ex.Message;
            }
        };
        btnPanel.Children.Add(mcBtn);
        Grid.SetColumn(btnPanel, 2);
        row.Children.Add(btnPanel);"""
assert old_row in src, 'BuildVersionRow anchor not found'
src = src.replace(old_row, new_row)

# B) InstallGdkLoose: 解密完整后删除源包
old_gdk = """            ModOtherGames.WriteBedrockMarker(dest);
            // 解包完整性校验：PCLCE 解包若被中断（如中途关机/关闭）会漏解 data 资源，导致启动黑屏崩溃
            var ok = ModOtherGames.IsBedrockVersionComplete(dest);"""
new_gdk = """            ModOtherGames.WriteBedrockMarker(dest);
            // 解包完整性校验：PCLCE 解包若被中断（如中途关机/关闭）会漏解 data 资源，导致启动黑屏崩溃
            var ok = ModOtherGames.IsBedrockVersionComplete(dest);
            // 小teto定制：解密完整后自动删除源包（.msixvc），避免占用磁盘空间
            if (ok)
                try { if (File.Exists(msixvcPath)) File.Delete(msixvcPath); } catch { }"""
assert old_gdk in src, 'InstallGdkLoose anchor not found'
src = src.replace(old_gdk, new_gdk)

# C) InstallUwpLoose: 注册成功后删除源包
old_uwp = """            if (p.ExitCode == 0)
            {
                ModBase.RunInUi(() =>
                {
                    HintInstalled.Text = "注册成功！点击「启动基岩版」即可游玩。";
                    RefreshInstalled();
                });
            }"""
new_uwp = """            if (p.ExitCode == 0)
            {
                // 小teto定制：注册成功后自动删除源包（.appx）
                try { if (File.Exists(appxPath)) File.Delete(appxPath); } catch { }
                ModBase.RunInUi(() =>
                {
                    HintInstalled.Text = "注册成功！点击「启动基岩版」即可游玩。";
                    RefreshInstalled();
                });
            }"""
assert old_uwp in src, 'InstallUwpLoose anchor not found'
src = src.replace(old_uwp, new_uwp)

# D) 加 McAppxVersionPage 映射方法（放在 MajorGroup 方法后）
old_major = """    private static string MajorGroup(string version)
    {
        var parts = version.Split('.');
        if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0])) return version;
        var major = int.TryParse(parts[0], out var m) ? m : -1;
        if (major >= 10) return parts[0] + ".x"; // 年度版本号
        if (parts.Length >= 2) return parts[0] + "." + parts[1];
        return parts[0];
    }"""
new_major = old_major + """

    /// <summary>
    ///     小teto定制：构造 mcappx 版本库的版本页 URL。
    ///     mcappx 按大版本分页：26.x 按十位分组（26.45→26.40），1.x 按 major.minor（1.21.131→1.21）。
    /// </summary>
    private static string McAppxVersionPage(string version)
    {
        var parts = version.Split('.');
        if (parts.Length >= 2 && int.TryParse(parts[0], out var major) && major >= 10)
        {
            var minor = int.TryParse(parts[1], out var m2) ? m2 : 0;
            var group = (minor / 10) * 10;
            return $"https://www.mcappx.com/bedrock/{major}.{group}/{version}/";
        }
        if (parts.Length >= 2)
            return $"https://www.mcappx.com/bedrock/{parts[0]}.{parts[1]}/{version}/";
        return $"https://www.mcappx.com/bedrock/{version}/";
    }"""
assert old_major in src, 'MajorGroup anchor not found'
src = src.replace(old_major, new_major)

open(path,'wb').write(src.replace('\n','\r\n').encode('utf-8-sig'))
print('PageDownloadBedrock.xaml.cs patched OK, changed:', orig != src)
