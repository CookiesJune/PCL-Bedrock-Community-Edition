using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PCL.Core.App;

namespace PCL;

public partial class PageDownloadBedrock
{
    private bool isLoad;
    private List<ModOtherGames.BedrockVersion> allVersions = new();
    private ModOtherGames.BedrockVersion? selectedVersion;
    private bool isDownloading;

    public PageDownloadBedrock()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (isLoad) return;
        isLoad = true;
        RefreshInstalled();
        RefreshFolderInfo();
        LoadVersionsAsync();
    }

    private void BtnBackSelect_Click(object sender, MouseButtonEventArgs e) =>
        ModMain.frmDownloadLeft.GoTo(FormMain.PageSubType.DownloadHome);

    private void RefreshFolderInfo()
    {
        // 小teto定制：不显示具体下载地址，只说明下载目标行为（下载并解压到当前 BE 实例文件夹的 bedrock_versions）
        try
        {
            var bv = Path.Combine(ModOtherGames.CurrentBedrockInstanceFolder(), "bedrock_versions");
            TextFolderInfo.Text = "下载并解压到当前所选 BE 实例文件夹的 bedrock_versions 目录";
        }
        catch
        {
            TextFolderInfo.Text = "下载并解压到当前所选 BE 实例文件夹的 bedrock_versions 目录";
        }
    }

    // ---------------- 版本列表 ----------------

    private async void LoadVersionsAsync()
    {
        TextVersionHint.Text = "正在从版本库加载基岩版版本……";
        var versions = await ModOtherGames.GetBedrockVersionsAsync();
        allVersions = versions;
        if (versions.Count == 0)
        {
            TextVersionHint.Text = "加载失败，请检查网络后重试（已尝试多个版本库源）。";
            return;
        }

        TextVersionHint.Text = $"共获取 {versions.Count} 个版本（含 UWP 与 GDK 构建）。下载完成后点击「安装所选 UWP 包」可安装并启动（UWP 需开发者模式）。";
        RenderVersionList();
    }

    // 渲染：顶部最新正式版/预览版 + 三个折叠分区（正式版/预览版/Beta 版），分区内按大版本折叠
    private void RenderVersionList()
    {
        PanLatest.Children.Clear();

        var release = allVersions
            .Where(v => v.Type.Contains("Release", StringComparison.OrdinalIgnoreCase)).ToList();
        var preview = allVersions
            .Where(v => v.Type.Contains("Preview", StringComparison.OrdinalIgnoreCase)).ToList();
        var beta = allVersions
            .Where(v => !v.Type.Contains("Release", StringComparison.OrdinalIgnoreCase)
                        && !v.Type.Contains("Preview", StringComparison.OrdinalIgnoreCase)).ToList();

        AddLatestCard(release, "最新正式版");
        AddLatestCard(preview, "最新预览版");

        RenderGroupCard(CardGroupRelease, release, "正式版");
        RenderGroupCard(CardGroupPreview, preview, "预览版");
        RenderGroupCard(CardGroupBeta, beta, "Beta 版");
    }

    // 顶部最新版本卡片（类似 Java 下载页）
    private void AddLatestCard(List<ModOtherGames.BedrockVersion> list, string title)
    {
        var ordered = list.OrderByDescending(v => v.Version, NaturalVersionComparer).ToList();
        if (ordered.Count == 0) return;
        var v = ordered[0];

        var card = new MyCard { Title = title, UseAnimation = false, Margin = new Thickness(0, 0, 0, 10) };
        var sp = new StackPanel { Margin = new Thickness(20, MyCard.SwapedHeight, 18, 0) };
        sp.Children.Add(BuildVersionRow(v, true));
        card.Children.Add(sp);
        PanLatest.Children.Add(card);
    }

    // 自然数字版本比较（26.2 < 26.13；1.21 > 1.20）
    private static readonly System.Collections.Generic.Comparer<string> NaturalVersionComparer =
        System.Collections.Generic.Comparer<string>.Create(CompareBedrockVersion);

    private static int CompareBedrockVersion(string a, string b)
    {
        var pa = a.Split('.');
        var pb = b.Split('.');
        for (var i = 0; i < Math.Max(pa.Length, pb.Length); i++)
        {
            var x = i < pa.Length && int.TryParse(pa[i], out var n1) ? n1 : 0;
            var y = i < pb.Length && int.TryParse(pb[i], out var n2) ? n2 : 0;
            if (x != y) return x.CompareTo(y);
        }

        return string.Compare(a, b, StringComparison.Ordinal);
    }

    // 大版本归类：26.33/26.11 → 26.x；1.19.0.23 → 1.19
    private static string MajorGroup(string version)
    {
        var parts = version.Split('.');
        if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0])) return version;
        var major = int.TryParse(parts[0], out var m) ? m : -1;
        if (major >= 10) return parts[0] + ".x"; // 年度版本号
        if (parts.Length >= 2) return parts[0] + "." + parts[1];
        return parts[0];
    }

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
    }

    // 渲染一个折叠分区：内部为大版本折叠卡，大版本内为版本行。
    // 注意：MyCard 的 Children 前几个是内部结构（阴影/边框/标题），绝对不能 Clear，只能 Add。
    // 折叠/展开统一走 AttachSafeSwap（PreviewSwap 接管），不触发 MyCard 原生 IsSwapped 切换，
    // 从而彻底避免 StackInstall / 高度动画 / 箭头旋转等路径可能引发的崩溃。
    private void RenderGroupCard(MyCard groupCard, List<ModOtherGames.BedrockVersion> list, string title)
    {
        var ordered = list.OrderByDescending(v => v.Version, NaturalVersionComparer).ToList();
        var groups = ordered.GroupBy(v => MajorGroup(v.Version))
            .OrderByDescending(g => g.Key, NaturalVersionComparer)
            .ToList();

        groupCard.Title = $"{title}（{ordered.Count}）";

        // 移除旧内容（SwapControl 即旧的内容 StackPanel）
        if (groupCard.SwapControl is UIElement oldContent)
            groupCard.Children.Remove(oldContent);
        groupCard.SwapControl = null;

        if (groups.Count == 0)
        {
            var empty = new StackPanel { Margin = new Thickness(20, MyCard.SwapedHeight, 18, 0) };
            empty.Children.Add(new TextBlock { Text = "该分类暂无版本。", Opacity = 0.6, FontSize = 13 });
            groupCard.Children.Add(empty);
            groupCard.SwapControl = empty;
            AttachSafeSwap(groupCard, empty);
            return;
        }

        var stack = new StackPanel { Margin = new Thickness(20, MyCard.SwapedHeight, 18, 0) };
        foreach (var g in groups)
        {
            var subCard = new MyCard
            {
                Title = $"{g.Key}（{g.Count()}）",
                UseAnimation = false,
                Margin = new Thickness(0, 0, 0, 8)
            };
            var subStack = new StackPanel { Margin = new Thickness(20, MyCard.SwapedHeight, 18, 0) };
            foreach (var v in g)
                subStack.Children.Add(BuildVersionRow(v, false));
            subCard.SwapControl = subStack;
            subCard.Children.Add(subStack);
            // 初始折叠：内容隐藏并固定标题高度
            subStack.Visibility = Visibility.Collapsed;
            subCard.Height = MyCard.SwapedHeight;
            AttachSafeSwap(subCard, subStack);
            stack.Children.Add(subCard);
        }

        groupCard.SwapControl = stack;
        groupCard.Children.Add(stack);
        // 初始折叠三分区卡
        stack.Visibility = Visibility.Collapsed;
        groupCard.Height = MyCard.SwapedHeight;
        AttachSafeSwap(groupCard, stack);
    }

    /// <summary>
    ///     通过 PreviewSwap 接管 MyCard 的折叠/展开：设置 e.handled = true 阻止原生 IsSwapped 切换，
    ///     自行切换内容可见性与高度、旋转箭头，彻底绕开 MyCard 折叠相关崩溃路径。
    /// </summary>
    private static void AttachSafeSwap(MyCard card, StackPanel content)
    {
        card.PreviewSwap += (_, e) =>
        {
            e.handled = true;
            try
            {
                var isOpen = content.Visibility != Visibility.Collapsed;
                content.Visibility = isOpen ? Visibility.Collapsed : Visibility.Visible;
                card.Height = isOpen ? MyCard.SwapedHeight : double.NaN;
                if (card.MainSwap is not null)
                    card.MainSwap.RenderTransform = new RotateTransform(isOpen ? 180 : 0);
            }
            catch
            {
                // 折叠/展开期间任何异常都不得导致崩溃
            }
        };
    }

    private Grid BuildVersionRow(ModOtherGames.BedrockVersion v, bool isLatest)
    {
        var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // 小teto定制：预览版使用专用图标（Type=Preview 或 26.x 及以上均为预览版）
        var isPreviewRow = (v.Type ?? "").Contains("Preview", StringComparison.OrdinalIgnoreCase);
        if (!isPreviewRow)
        {
            var m = System.Text.RegularExpressions.Regex.Match(v.Version ?? "", @"^(\d+)\.(\d+)\.(\d+)");
            if (m.Success && int.TryParse(m.Groups[1].Value, out var major) && major >= 26)
                isPreviewRow = true;
        }
        var iconUri = isPreviewRow
            ? "pack://application:,,,/images/Custom/BedrockPreviewIcon.png"
            : "pack://application:,,,/images/Custom/BedrockIconLarge.png";
        var icon = new Image
        {
            Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconUri)),
            Width = 22,
            Height = 22,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.NearestNeighbor);
        Grid.SetColumn(icon, 0);
        row.Children.Add(icon);

        var info = new TextBlock
        {
            Text = $"{v.Version}    {v.Type} / {v.BuildType}", 
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            FontSize = isLatest ? 14 : 13,
            FontWeight = isLatest ? FontWeights.Bold : FontWeights.Normal,
        };
        Grid.SetColumn(info, 1);
        row.Children.Add(info);

        var btnPanel = new StackPanel { Orientation = Orientation.Horizontal };
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
                HintInstalled.Text = "已用浏览器打开 mcappx 版本页。下载完成后把 .appx 文件放进版本文件夹（bedrock_versions 下的版本目录）即可识别。";
            }
            catch (Exception ex)
            {
                HintInstalled.Text = "打开 mcappx 页面失败：" + ex.Message;
            }
        };
        btnPanel.Children.Add(mcBtn);
        Grid.SetColumn(btnPanel, 2);
        row.Children.Add(btnPanel);

        return row;
    }

    private void BtnRefresh_Click(object sender, MouseButtonEventArgs e) => LoadVersionsAsync();

    // ---------------- 下载任务 ----------------

    private async void StartDownloadAsync(ModOtherGames.BedrockVersion v)
    {
        if (isDownloading)
        {
            ModBase.RunInUi(() => HintInstalled.Text = "已有任务在进行中，请等待完成。");
            return;
        }

        isDownloading = true;
        List<string> allUwpUrls = null;
        string router = null;
        try
        {
            // 必须在异步上下文 await，避免同步阻塞 UI 线程导致界面卡死
            if (v.IsUwp)
                allUwpUrls = await ResolveAllUwpUrlsAsync(v);
            else
                router = await ResolveRouterAsync(v);
        }
        catch (Exception ex)
        {
            ModBase.RunInUi(() =>
            {
                AddTaskRow(v.Version, $"解析下载地址失败：{ex.Message}", 0);
                HintInstalled.Text = "解析下载地址失败。";
            });
            isDownloading = false;
            return;
        }

        if (v.IsUwp ? (allUwpUrls == null || allUwpUrls.Count == 0) : string.IsNullOrEmpty(router))
        {
            ModBase.RunInUi(() =>
            {
                AddTaskRow(v.Version, "无法解析该版本的下载地址（UWP 需要 FE3 在线解析，GDK 需要完整 URL）。", 0);
                HintInstalled.Text = "无法解析下载地址。";
            });
            isDownloading = false;
            return;
        }

        // 小teto定制：UWP 包 FE3 可能返回多个链接（主程序包+资源包），必须全部下载解压合并
        if (v.IsUwp && allUwpUrls.Count > 1)
        {
            ModBase.Log("[Bedrock] FE3 返回多个链接(" + allUwpUrls.Count + ")，使用批量下载解压模式");
            DownloadAndExtractAllUwpAsync(v, allUwpUrls);
            return;
        }

        string fullUrl = v.IsUwp ? allUwpUrls[0] : null;

        var folder = ModOtherGames.CurrentBedrockInstanceFolder();
        // 下载地址统一为实例文件夹下的 bedrock_versions（BedrockBoot 式），解压/注册也在此目录
        var bvFolder = Path.Combine(folder, "bedrock_versions");
        Directory.CreateDirectory(bvFolder);
        var fileName = v.IsUwp
            ? $"Minecraft_Bedrock_{v.Version}_{v.Arch ?? "x64"}.appx"
            : $"Minecraft_Bedrock_{v.Version}_{v.Arch ?? "x64"}.msixvc";
        var target = Path.Combine(bvFolder, fileName);

        // PCL 原生右下角下载任务（LoaderTaskbar：进度/速度/状态一目了然）
        var urls = new List<string>();
        if (v.IsUwp)
            urls.Add(fullUrl);
        else
            urls.AddRange(ModOtherGames.MirrorHosts.Select(h => $"http://{h}{router}"));

        var file = new PCL.Network.DownloadFile(urls, target);
        // 小teto定制：任务标题明确区分 GDK/UWP 包本体下载
        var stageLabel = v.IsUwp ? "下载UWP包本体" : "下载GDK包本体";
        var download = new PCL.Network.Loaders.LoaderDownload($"{stageLabel} {v.Version}", new List<PCL.Network.DownloadFile> { file });
        var combo = new ModLoader.LoaderCombo<string>($"{stageLabel} {v.Version}", new List<ModLoader.LoaderBase> { download });
        // 小teto定制：任务页面任务行，下载→解压两阶段明确进度
        TextBlock taskRow = null;
        ModBase.RunInUi(() => taskRow = AddTaskRow(v.Version, $"{stageLabel}：0%", 0));
        var polling = true;
        _ = Task.Run(async () =>
        {
            while (polling)
            {
                var st = combo.State;
                if (st == ModBase.LoadState.Waiting || st == ModBase.LoadState.Loading)
                {
                    var pct = (int)(combo.Progress * 100);
                    if (pct < 0) pct = 0;
                    if (pct > 100) pct = 100;
                    var row = taskRow;
                    ModBase.RunInUi(() => { if (row != null) row.Text = $"{v.Version}：{stageLabel}... {pct}%"; });
                }
                if (st == ModBase.LoadState.Finished || st == ModBase.LoadState.Failed || st == ModBase.LoadState.Aborted) break;
                await Task.Delay(400);
            }
        });
        combo.OnStateChanged = _ =>
        {
            if (combo.State != ModBase.LoadState.Finished)
            {
                isDownloading = false;
                return;
            }
            isDownloading = false;
            polling = false;
            if (v.IsUwp)
            {
                // 下载完成自动解压到所选实例文件夹（BedrockBoot 式 loose 结构）
                HintInstalled.Text = "下载完成，正在自动解压到实例文件夹……";
                ModBase.RunInUi(() => { if (taskRow != null) taskRow.Text = $"{v.Version}：解压包... 0%"; });
                Task.Run(() => InstallUwpLoose(target, v, taskRow));
            }
            else
            {
                HintInstalled.Text = "下载完成，正在解密 GDK 包（XVD\u2192AES-XTS，大文件可能需数分钟）……";
                ModBase.RunInUi(() => { if (taskRow != null) taskRow.Text = $"{v.Version}：解压包（解密 GDK，大文件请稍候）..."; });
                Task.Run(() => InstallGdkLoose(target, v, taskRow));
            }
        };
        combo.Start(target);
        ModLoader.LoaderTaskbarAdd(combo);
        ModMain.frmMain.BtnExtraDownload.ShowRefresh();
        ModMain.frmMain.BtnExtraDownload.Ribble();
        HintInstalled.Text = $"已加入右下角下载任务：{fileName}";
    }

    private (TextBlock, ProgressBar, TextBlock, TextBlock)? _uiTask;

    /// <summary>
    ///     解析版本的 CDN router（路径）。GDK 直接取 URL 的 AbsolutePath；UWP 走 FE3。
    /// </summary>
    private async Task<string> ResolveRouterAsync(ModOtherGames.BedrockVersion v)
    {
        if (v.MetaData.Count == 0) return "";
        var meta = v.MetaData[0];
        if (meta.StartsWith("http"))
        {
            try
            {
                return new Uri(meta).AbsolutePath;
            }
            catch
            {
                return meta;
            }
        }

        // UWP：UpdateID GUID → FE3 解析
        var urls = await ModOtherGames.ResolveUwpUrlAsync(meta);
        if (urls.Count > 0)
        {
            try
            {
                return new Uri(urls[0]).AbsolutePath;
            }
            catch
            {
                return urls[0];
            }
        }

        return "";
    }

    /// <summary>
    ///     解析版本的所有下载 URL（UWP：FE3 在线解析返回完整签名直链列表；GDK：直接取元数据中的完整 URL）。
    ///     小teto定制：FE3可能返回多个链接（主程序包+资源包+依赖项），必须全部下载解压合并，否则只有data+dll。
    /// </summary>
    private async Task<List<string>> ResolveAllUwpUrlsAsync(ModOtherGames.BedrockVersion v)
    {
        if (v.MetaData.Count == 0) return new List<string>();
        var meta = v.MetaData[0];
        if (meta.StartsWith("http"))
            return new List<string> { meta };
        var urls = await ModOtherGames.ResolveUwpUrlAsync(meta);
        ModBase.Log("[Bedrock] FE3 返回 " + urls.Count + " 个下载链接: " + string.Join("; ", urls.Select(u => u.Substring(0, Math.Min(80, u.Length)) + "...")));
        return urls;
    }

    private TextBlock AddTaskRow(string version, string message, double percent)
    {
        var row = new Grid { Margin = new Thickness(0, 6, 0, 6) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var t = new TextBlock { Text = $"{version}：{message}", TextWrapping = TextWrapping.Wrap, FontSize = 13 };
        row.Children.Add(t);
        PanTasks.Children.Add(row);
        return t;
    }

    // ---------------- 安装与启动 ----------------

    private void BtnInstallUwp_Click(object sender, MouseButtonEventArgs e)
    {
        if (selectedVersion is null)
        {
            HintInstalled.Text = "请先在版本列表中选择一个版本下载。";
            return;
        }

        var folder = ModOtherGames.CurrentBedrockInstanceFolder();
        var bvFolder = Path.Combine(folder, "bedrock_versions");
        string file;
        try
        {
            Directory.CreateDirectory(bvFolder);
            file = Directory.GetFiles(bvFolder, $"Minecraft_Bedrock_{selectedVersion.Version}*.appx").FirstOrDefault()
                   ?? Directory.GetFiles(bvFolder, $"Minecraft_Bedrock_{selectedVersion.Version}*.msixvc").FirstOrDefault();
        }
        catch (Exception ex)
        {
            HintInstalled.Text = "无法访问下载目录：" + ex.Message;
            return;
        }
        if (file is null)
        {
            HintInstalled.Text = "未找到该版本的安装包文件，请先下载。";
            return;
        }

        if (file.EndsWith(".appx", StringComparison.OrdinalIgnoreCase))
            Task.Run(() => InstallUwpLoose(file, selectedVersion));
        else
            Task.Run(() => InstallGdkLoose(file, selectedVersion));
    }

    /// <summary>
    ///     BedrockBoot 式 UWP 安装：解压 appx → 删除签名 → 开发者模式注册（loose 包）→ 可启动。
    /// </summary>
    /// <summary>
    ///     小teto定制：批量下载并解压所有 FE3 返回的 UWP 包（主程序包+资源包+依赖项），合并到同一个版本文件夹。
    ///     修复 1.16~1.18 只下载第一个链接导致只有 data+4个dll 的问题。
    /// </summary>
    private async void DownloadAndExtractAllUwpAsync(ModOtherGames.BedrockVersion version, List<string> urls)
    {
        TextBlock taskRow = null;
        ModBase.RunInUi(() => taskRow = AddTaskRow(version.Version, $"批量下载 {urls.Count} 个包...", 0));
        try
        {
            var folder = ModOtherGames.CurrentBedrockInstanceFolder();
            var bvFolder = Path.Combine(folder, "bedrock_versions");
            var dest = Path.Combine(bvFolder, version.Version);
            var tempDir = Path.Combine(Path.GetTempPath(), "PCL_Bedrock_Download_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(bvFolder);
            Directory.CreateDirectory(tempDir);
            if (Directory.Exists(dest))
                try { Directory.Delete(dest, true); } catch { }
            Directory.CreateDirectory(dest);

            ModBase.Log("[Bedrock] 批量下载开始，版本: " + version.Version + ", 包数: " + urls.Count + ", 临时目录: " + tempDir);

            // 逐个下载并解压
            for (int i = 0; i < urls.Count; i++)
            {
                var url = urls[i];
                var tempFile = Path.Combine(tempDir, $"package_{i}.appx");
                ModBase.RunInUi(() => { if (taskRow != null) taskRow.Text = $"{version.Version}：下载包 {i + 1}/{urls.Count}..."; });
                ModBase.Log("[Bedrock] 下载包 " + (i + 1) + "/" + urls.Count + ": " + url.Substring(0, Math.Min(100, url.Length)) + "...");

                try
                {
                    using var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromMinutes(30) };
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Windows-Update-Agent/10.0.17134.471");
                    using var resp = await httpClient.GetAsync(url, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
                    if (!resp.IsSuccessStatusCode)
                    {
                        ModBase.Log("[Bedrock] 包 " + (i + 1) + " 下载失败: " + resp.StatusCode);
                        continue;
                    }
                    var totalBytes = resp.Content.Headers.ContentLength ?? 0;
                    ModBase.Log("[Bedrock] 包 " + (i + 1) + " 大小: " + (totalBytes / 1024 / 1024) + " MB");
                    using var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None);
                    using var stream = await resp.Content.ReadAsStreamAsync();
                    var buffer = new byte[81920];
                    long readBytes = 0;
                    int read;
                    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fs.WriteAsync(buffer, 0, read);
                        readBytes += read;
                        if (totalBytes > 0 && readBytes % (1024 * 1024) < 81920)
                        {
                            var pct = (int)(readBytes * 100 / totalBytes);
                            var row = taskRow;
                            ModBase.RunInUi(() => { if (row != null) row.Text = $"{version.Version}：下载包 {i + 1}/{urls.Count}... {pct}%"; });
                        }
                    }
                    fs.Close();
                    ModBase.Log("[Bedrock] 包 " + (i + 1) + " 下载完成，实际大小: " + (new FileInfo(tempFile).Length / 1024 / 1024) + " MB");
                }
                catch (Exception ex)
                {
                    ModBase.Log("[Bedrock] 包 " + (i + 1) + " 下载异常: " + ex.Message);
                    continue;
                }

                // 解压这个包到版本文件夹
                ModBase.RunInUi(() => { if (taskRow != null) taskRow.Text = $"{version.Version}：解压包 {i + 1}/{urls.Count}..."; });
                try
                {
                    var progress = new Progress<BedrockLauncher.Core.Utils.DecompressProgress>(p =>
                    {
                        var pct = (int)p.Percentage;
                        var row = taskRow;
                        ModBase.RunInUi(() => { if (row != null) row.Text = $"{version.Version}：解压包 {i + 1}/{urls.Count}... {pct}%"; });
                    });
                    await BedrockLauncher.Core.Utils.ZipExtractor.ExtractWithProgressAsync(tempFile, dest, progress);
                    ModBase.Log("[Bedrock] 包 " + (i + 1) + " 解压完成");
                }
                catch (Exception ex)
                {
                    ModBase.Log("[Bedrock] 包 " + (i + 1) + " 解压异常: " + ex.Message);
                }

                try { File.Delete(tempFile); } catch { }
            }

            // 清理临时目录
            try { Directory.Delete(tempDir, true); } catch { }

            // 验证关键文件
            var hasExe = File.Exists(Path.Combine(dest, "Minecraft.Windows.exe"));
            var hasManifest = File.Exists(Path.Combine(dest, "AppxManifest.xml"));
            var hasData = Directory.Exists(Path.Combine(dest, "data"));
            ModBase.Log("[Bedrock] 批量下载解压完成，exe=" + hasExe + ", manifest=" + hasManifest + ", data=" + hasData);

            if (!hasExe || !hasManifest)
            {
                ModBase.RunInUi(() =>
                {
                    HintInstalled.Text = "下载的包不完整（缺少 Minecraft.Windows.exe 或 AppxManifest.xml）。已清理，请尝试用「mcappx」按钮从浏览器下载，或更换其他版本。";
                    if (taskRow != null) taskRow.Text = $"{version.Version}：包不完整，已清理";
                });
                try { Directory.Delete(dest, true); } catch { }
                isDownloading = false;
                return;
            }

            // 删除签名文件，写入标记
            var p7x = Path.Combine(dest, "AppxSignature.p7x");
            if (File.Exists(p7x)) try { File.Delete(p7x); } catch { }
            ModOtherGames.WriteBedrockMarker(dest);
            ModOtherGames.WriteUwpMarker(dest);

            // 注册 UWP 包
            ModBase.RunInUi(() => HintInstalled.Text = "正在注册 UWP 包……");
            var manifest = Path.Combine(dest, "AppxManifest.xml");
            if (ModOtherGames.RegisterLooseAppx(manifest))
            {
                ModBase.RunInUi(() =>
                {
                    HintInstalled.Text = "注册成功！点击「启动基岩版」即可游玩。";
                    if (taskRow != null) taskRow.Text = $"{version.Version}：UWP 包安装完成";
                    RefreshInstalled();
                });
            }
            else
            {
                ModBase.RunInUi(() =>
                {
                    HintInstalled.Text = "UWP 包注册失败，请确认已开启开发者模式后重试。";
                    if (taskRow != null) taskRow.Text = $"{version.Version}：注册失败";
                });
            }
        }
        catch (Exception ex)
        {
            ModBase.Log("[Bedrock] 批量下载解压异常: " + ex);
            ModBase.RunInUi(() =>
            {
                HintInstalled.Text = "批量下载解压失败：" + ex.Message;
                if (taskRow != null) taskRow.Text = $"{version.Version}：失败 {ex.Message}";
            });
        }
        isDownloading = false;
    }

    private void InstallUwpLoose(string appxPath, ModOtherGames.BedrockVersion version, TextBlock taskRow = null)
    {
        try
        {
            var dest = Path.Combine(ModOtherGames.CurrentBedrockInstanceFolder(), "bedrock_versions", version.Version);
            ModBase.RunInUi(() => HintInstalled.Text = "正在解压 UWP 包……");
            if (Directory.Exists(dest))
                try { Directory.Delete(dest, true); }
                catch { }
            Directory.CreateDirectory(dest);
            // 小teto定制：带进度的解压（BedrockBoot ZipExtractor，任务行显示解压百分比）
            var progress = new Progress<BedrockLauncher.Core.Utils.DecompressProgress>(p =>
            {
                var pct = (int)p.Percentage;
                ModBase.RunInUi(() => { if (taskRow != null) taskRow.Text = $"{version.Version}：解压包... {pct}%"; });
            });
            BedrockLauncher.Core.Utils.ZipExtractor.ExtractWithProgressAsync(appxPath, dest, progress).GetAwaiter().GetResult();
            var p7x = Path.Combine(dest, "AppxSignature.p7x");
            if (File.Exists(p7x))
                try { File.Delete(p7x); }
                catch { }
            // 小teto定制：解压后验证关键文件（修复 1.16/1.17/1.19 解压后只有 data+dll 的问题）
            var manifest = Path.Combine(dest, "AppxManifest.xml");
            var hasExe = File.Exists(Path.Combine(dest, "Minecraft.Windows.exe"));
            var hasData = Directory.Exists(Path.Combine(dest, "data"));
            if (!File.Exists(manifest) || (!hasExe && !hasData))
            {
                ModBase.Log("[Bedrock] UWP 解压后缺少关键文件，包可能不完整或下载了错误格式。manifest=" + File.Exists(manifest) + ", exe=" + hasExe + ", data=" + hasData);
                try
                {
                    var files = Directory.GetFiles(dest).Select(Path.GetFileName).Take(20);
                    var dirs = Directory.GetDirectories(dest).Select(Path.GetFileName).Take(10);
                    ModBase.Log("[Bedrock] 解压后文件: " + string.Join(", ", files) + "; 目录: " + string.Join(", ", dirs));
                }
                catch { }
                try { Directory.Delete(dest, true); } catch { }
                try { if (File.Exists(appxPath)) File.Delete(appxPath); } catch { }
                ModBase.RunInUi(() =>
                {
                    HintInstalled.Text = "下载的包格式不正确或不完整（可能是 FE3 解析出了错误的下载链接）。已清理，请尝试用「mcappx」按钮从浏览器下载，或更换其他版本。";
                    if (taskRow != null) taskRow.Text = $"{version.Version}：包格式错误，已清理";
                });
                return;
            }
            ModOtherGames.WriteBedrockMarker(dest);
            // 小teto定制：UWP 解压完成后创建 UWPtres.txt 标记文件，用于识别包类型
            ModOtherGames.WriteUwpMarker(dest);

            if (!ModOtherGames.IsDeveloperMode())
            {
                ModBase.RunInUi(() => HintInstalled.Text = "需要先开启 Windows 开发者模式才能注册 UWP 包（设置→开发者选项）。正在为你打开设置页……");
                ModBase.RunInUi(() => Process.Start(new ProcessStartInfo("ms-settings:developers") { UseShellExecute = true }));
                return;
            }

            ModBase.RunInUi(() => HintInstalled.Text = "正在以开发者模式注册 UWP 包……");
            var psi = new ProcessStartInfo("powershell.exe")
            {
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"Add-AppxPackage -Register '{manifest}' -ForceUpdateFromAnyVersion\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using var p = Process.Start(psi);
            if (p is null)
            {
                ModBase.RunInUi(() => HintInstalled.Text = "无法启动安装进程。");
                return;
            }
            var outTxt = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
            if (!p.WaitForExit(180000))
            {
                try { p.Kill(); } catch { }
                ModBase.RunInUi(() => HintInstalled.Text = "注册超时，请检查开发者模式后重试。");
                return;
            }
            if (p.ExitCode == 0)
            {
                // 小teto定制：注册成功后自动删除源包（.appx）
                try { if (File.Exists(appxPath)) File.Delete(appxPath); } catch { }
                ModBase.RunInUi(() =>
                {
                    HintInstalled.Text = "注册成功！点击「启动基岩版」即可游玩。";
                    if (taskRow != null) taskRow.Text = $"{version.Version}：UWP 包安装完成";
                    RefreshInstalled();
                });
            }
            else
            {
                var line = outTxt.Trim().Replace("\r", "").Split('\n').LastOrDefault();
                var msg = string.IsNullOrWhiteSpace(line) ? "请确认已开启开发者模式后重试" : line;
                ModBase.RunInUi(() =>
                {
                    HintInstalled.Text = "注册失败：" + msg;
                    if (taskRow != null) taskRow.Text = $"{version.Version}：注册失败 {msg}";
                });
            }
        }
        catch (Exception ex)
        {
            ModBase.RunInUi(() => HintInstalled.Text = "安装失败：" + ex.Message);
        }
    }
    /// <summary>
    ///     GDK 加密包：本地解密（BedrockBoot 核心算法：XVD 容器 + AES-XTS，CIK 密钥来自反编译产物），
    ///     解密后即为完整 loose 游戏目录，可直接 Process.Start Minecraft.Windows.exe 启动，无需正版验证。
    /// </summary>
    private void InstallGdkLoose(string msixvcPath, ModOtherGames.BedrockVersion version, TextBlock taskRow = null)
    {
        try
        {
            var bv = Path.Combine(ModOtherGames.CurrentBedrockInstanceFolder(), "bedrock_versions");
            Directory.CreateDirectory(bv);
            var dest = Path.Combine(bv, version.Version);
            ModBase.RunInUi(() =>
            {
                HintInstalled.Text = $"正在解密 GDK 包 {version.Version}（大文件，可能需数分钟）……";
                if (taskRow != null) taskRow.Text = $"{version.Version}：解压包（解密 GDK，大文件请稍候）...";
            });
            if (Directory.Exists(dest))
                try { Directory.Delete(dest, true); } catch { }
            var gtv = (version.Type ?? "").Contains("Preview", StringComparison.OrdinalIgnoreCase)
                ? BedrockLauncher.Core.MinecraftGameTypeVersion.Preview
                : (version.Type ?? "").Contains("Beta", StringComparison.OrdinalIgnoreCase)
                    ? BedrockLauncher.Core.MinecraftGameTypeVersion.Beta
                    : BedrockLauncher.Core.MinecraftGameTypeVersion.Release;
            var core = new BedrockLauncher.Core.BedrockCore();
            // 后台线程执行（无 WPF 同步上下文），GetResult 不会死锁 UI
            core.InstallPackageAsync(new BedrockLauncher.Core.CoreOption.LocalGamePackageOptions
            {
                FileFullPath = msixvcPath,
                Type = BedrockLauncher.Core.MinecraftBuildTypeVersion.GDK,
                InstallDstFolder = dest,
                GameTypeVersion = gtv,
                UseHardwareDecode = true
            }).GetAwaiter().GetResult();
            ModOtherGames.WriteBedrockMarker(dest);
            // 解包完整性校验：PCLCE 解包若被中断（如中途关机/关闭）会漏解 data 资源，导致启动黑屏崩溃
            var ok = ModOtherGames.IsBedrockVersionComplete(dest);
            // 小teto定制：解密完整后自动删除源包（.msixvc），避免占用磁盘空间
            if (ok)
                try { if (File.Exists(msixvcPath)) File.Delete(msixvcPath); } catch { }
            ModBase.RunInUi(() =>
            {
                HintInstalled.Text = ok
                    ? "GDK 解密完成！可点击「启动基岩版」直接游玩（无需正版验证）。"
                    : "GDK 解密不完整（可能被中断，缺少游戏资源）。请删除该版本后重新下载解包，否则启动会黑屏崩溃。";
                if (taskRow != null) taskRow.Text = ok ? $"{version.Version}：解压完成" : $"{version.Version}：解压不完整";
                RefreshInstalled();
            });
        }
        catch (Exception ex)
        {
            ModBase.RunInUi(() => HintInstalled.Text = "GDK 解密失败：" + ex.Message);
        }
    }
    /// <summary>
    ///     GDK 加密包：通过系统商店安装（需管理员 + 正版授权，作为 InstallGdkLoose 的兜底）。
    /// </summary>
    private void InstallGdkViaStore(string msixvcPath)
    {
        try
        {
            var psi = new ProcessStartInfo("powershell.exe")
            {
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"Add-AppxPackage -Path '{msixvcPath}'\"",
                UseShellExecute = true,
                Verb = "runas",
            };
            Process.Start(psi);
            HintInstalled.Text = "GDK 加密包需通过系统安装，已请求管理员权限（Add-AppxPackage）。请在 UAC 中确认；若账号未购买该版本将安装失败。";
        }
        catch
        {
            HintInstalled.Text = "GDK 安装被取消（未授权管理员权限）。";
        }
    }

    private void RefreshInstalled()
    {
        // 优先读取本地 BE 版本（标记判定，兼容 根/版本 与 根/bedrock_versions/版本 结构）
        List<string> localVersions = new();
        try
        {
            var folder = ModOtherGames.CurrentBedrockInstanceFolder();
            foreach (var vdir in ModOtherGames.FindBedrockVersionFolders(folder))
            {
                var name = Path.GetFileName(vdir);
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (string.Equals(Path.TrimEndingDirectorySeparator(vdir),
                        Path.TrimEndingDirectorySeparator(folder), StringComparison.OrdinalIgnoreCase))
                    continue; // 排除 BE 根目录本身
                localVersions.Add(name);
            }
            localVersions = localVersions.Distinct().ToList();
        }
        catch
        {
            // 目录不可读时静默处理，避免页面加载崩溃
        }

        if (localVersions.Count > 0)
        {
            TextInstalled.Text = "本地 bedrock_versions 已解压版本：" +
                                 string.Join("、", localVersions.OrderByDescending(x => x, NaturalVersionComparer));
            BtnLaunch.IsEnabled = true;
            HintInstalled.Text = "";
            return;
        }

        var list = ModOtherGames.FindGamePackage(ModOtherGames.BedrockFamilyPrefixes);
        if (list.Count == 0)
        {
            TextInstalled.Text = "未检测到已安装的基岩版（本地 bedrock_versions 为空，且未安装系统包）。";
            BtnLaunch.IsEnabled = false;
            HintInstalled.Text = "";
            return;
        }

        var info = list.First();
        TextInstalled.Text = $"已安装  {info.Name}   版本 {info.Version}   授权{(info.IsOk ? "有效" : "异常")}";
        BtnLaunch.IsEnabled = info.IsOk;
    }

    private void BtnLaunch_Click(object sender, MouseButtonEventArgs e)
    {
        // 优先启动本地 BE 解压版本（标记判定：GDK 直接 Process.Start exe，无需正版验证）
        var folder = ModOtherGames.CurrentBedrockInstanceFolder();
        string? bestExe = null;
        string? bestDir = null;
        foreach (var vdir in ModOtherGames.FindBedrockVersionFolders(folder))
        {
            var f = Path.Combine(vdir, "Minecraft.Windows.exe");
            if (!File.Exists(f)) continue;
            var dn = Path.GetFileName(vdir);
            if (bestExe is null || string.Compare(dn, Path.GetFileName(bestDir ?? ""),
                    StringComparison.OrdinalIgnoreCase) > 0)
            {
                bestExe = f;
                bestDir = vdir;
            }
        }
        if (bestExe is not null && bestDir is not null)
        {
            try
            {
                var proc = Process.Start(new ProcessStartInfo(bestExe) { WorkingDirectory = bestDir, UseShellExecute = true });
                // 小teto定制：接入进程监控（右下角结束进程按钮 / 运行日志 / 异常退出报告）
                ModOtherGames.MonitorBedrockProcess(bestDir, proc);
                HintInstalled.Text = "已启动基岩版（本地 GDK 版本 " + Path.GetFileName(bestDir) + "）。";
                return;
            }
            catch (Exception ex2)
            {
                HintInstalled.Text = "启动失败：" + ex2.Message;
                return;
            }
        }

        // 回退：系统 UWP 包
        var list = ModOtherGames.FindGamePackage(ModOtherGames.BedrockFamilyPrefixes);
        if (list.Count == 0)
        {
            HintInstalled.Text = "尚未安装基岩版（本地 bedrock_versions 无解压版本，系统也未安装包）。";
            return;
        }

        var ok = ModOtherGames.LaunchPackage(list.First());
        HintInstalled.Text = ok ? "已启动基岩版（系统包）。" : "启动失败。";
    }

    private void BtnOpenFolder_Click(object sender, MouseButtonEventArgs e)
    {
        var folder = ModOtherGames.CurrentBedrockInstanceFolder();
        var bvFolder = Path.Combine(folder, "bedrock_versions");
        Directory.CreateDirectory(bvFolder);
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{bvFolder}\"") { UseShellExecute = true });
    }
}
