using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using PCL.Core.App.Localization;

namespace PCL;

[ContentProperty("SearchTags")]
public partial class PageComp
{
    /// <summary>
    ///     每页展示的结果数量。
    /// </summary>
    public const int pageSize = 40;

    public int page;

    public ModComp.CompProjectStorage storage = new();

    // 结果 UI 化
    private void Load_OnFinish()
    {
        try
        {
            ModBase.Log($"[Comp] 开始可视化{TypeNameSpaced}列表，已储藏 {storage.results.Count} 个结果，当前在第 {page + 1} 页");
            // 列表项
            PanProjects.Children.Clear();
            var index = Math.Min(page * pageSize, storage.results.Count - 1);
            foreach (var result in storage.results.GetRange(index, Math.Min(storage.results.Count - index, pageSize)))
            {
                var showQuickDownload = result.Type != ModComp.CompType.ModPack &&
                                        result.Type != ModComp.CompType.DataPack;
                // 小teto定制：BE 模式始终显示版本描述、不显示加载器描述（BE 无加载器，避免显示“未知”）
                PanProjects.Children.Add(result.ToCompItem(IsBedrock || loader.input.gameVersion is null,
                    !IsBedrock &&
                    loader.input.modLoader == ModComp.CompLoaderType.Any &&
                    (PageType == ModComp.CompType.Mod || PageType == ModComp.CompType.ModPack),
                    showQuickDownload));
            }
            // 页码
            CardPages.Visibility =
                storage.results.Count > 40 || storage.curseForgeOffset < storage.curseForgeTotal ||
                storage.modrinthOffset < storage.modrinthTotal
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            LabPage.Text = Lang.Number(page + 1, "N0");
            BtnPageFirst.IsEnabled = page > 1;
            BtnPageFirst.Opacity = page > 1 ? 1d : 0.2d;
            BtnPageLeft.IsEnabled = page > 0;
            BtnPageLeft.Opacity = page > 0 ? 1d : 0.2d;
            var isRightEnabled = storage.results.Count > pageSize * (page + 1) ||
                                 storage.curseForgeOffset < storage.curseForgeTotal ||
                                 storage.modrinthOffset <
                                 storage.modrinthTotal; // 由于 WPF 的未知 bug，读取到的 IsEnabled 可能是错误的值（#3319）
            BtnPageRight.IsEnabled = isRightEnabled;
            BtnPageRight.Opacity = isRightEnabled ? 1d : 0.2d;
            // 错误信息
            if (storage.errorMessage is null)
            {
                HintError.Visibility = Visibility.Collapsed;
            }
            else
            {
                HintError.Visibility = Visibility.Visible;
                HintError.Text = storage.errorMessage;
            }

            // 强制返回顶部
            ScrollToTop();
        }
        catch (Exception ex)
        {
            ModBase.Log(
                ex,
                $"可视化{TypeNameSpaced}列表出错",
                ModBase.LogLevel.Feedback,
                userSummary: Lang.Text("Download.Comp.Error.OperationFailed"));
        }
    }

    // 自动重试
    private void Load_State(object sender, MyLoading.MyLoadingState state, MyLoading.MyLoadingState oldState)
    {
        switch (loader.State)
        {
            case ModBase.LoadState.Failed:
            {
                var errorMessage = "";
                if (loader.Error is not null)
                    errorMessage = loader.Error.Message;
                if (errorMessage.Contains(Lang.Text("Common.Error.InvalidJson")))
                {
                    ModBase.Log($"[Download] 下载的{TypeNameSpaced}列表 json 文件损坏，已自动重试", ModBase.LogLevel.Debug);
                    ((MyPageRight)Parent).PageLoaderRestart();
                }

                break;
            }
        }
    }

    // 切换页码
    private void BtnPageFirst_Click(object sender, EventArgs e)
    {
        ChangePage(0);
    }

    private void BtnPageLeft_Click(object sender, EventArgs e)
    {
        ChangePage(page - 1);
    }

    private void BtnPageRight_Click(object sender, EventArgs e)
    {
        ChangePage(page + 1);
    }

    private void ChangePage(int newPage)
    {
        CardPages.IsEnabled = false;
        page = newPage;
        ModMain.frmMain.BackToTop();
        ModBase.Log($"[Download] {TypeName}：切换到第 {page + 1} 页");
        ModBase.RunInThread(() =>
        {
            Thread.Sleep(100); // 等待向上滚的动画结束
            ModBase.RunInUi(() => CardPages.IsEnabled = true);
            loader.Start();
        });
    }

    // 安装已有整合包按钮
    private void BtnSearchInstallModPack_Click(object sender, EventArgs e)
    {
        ModModpack.ModpackInstall();
    }

    /// <summary>
    ///     刷新所有已显示项目的收藏状态
    /// </summary>
    public void RefreshAllFavoriteStatus()
    {
        try
        {
            foreach (var item in PanProjects.Children)
                if (item is MyCompItem)
                    ((MyCompItem)item).RefreshFavoriteStatus();
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "刷新收藏状态时出错");
        }
    }

    #region 属性

    /// <summary>
    ///     用于 XAML 快速设置的 Tag 下拉框列表。
    /// </summary>
    public ItemCollection SearchTags => ComboSearchTag.Items;

    public static readonly DependencyProperty SupportCurseForgeProperty =
        DependencyProperty.Register("SupportCurseForge", typeof(bool), typeof(PageComp), new PropertyMetadata(true));

    public bool SupportCurseForge
    {
        get => (bool)GetValue(SupportCurseForgeProperty);
        set => SetValue(SupportCurseForgeProperty, value);
    }

    public static readonly DependencyProperty SupportModrinthProperty =
        DependencyProperty.Register("SupportModrinth", typeof(bool), typeof(PageComp), new PropertyMetadata(true));

    public bool SupportModrinth
    {
        get => (bool)GetValue(SupportModrinthProperty);
        set => SetValue(SupportModrinthProperty, value);
    }

    /// <summary>
    ///     英文前后不含空格的可读资源类型名，例如 "Mod"、"整合包"。
    /// </summary>
    public string TypeName => ModComp.GetCompTypeName(PageType);

    /// <summary>
    ///     英文前后含一个空格的可读资源类型名，例如 " Mod "、"整合包"。
    /// </summary>
    public string TypeNameSpaced => TypeName;

    /// <summary>
    ///     该页面对应的资源类型。
    /// </summary>
    public ModComp.CompType PageType
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field = value;
            BtnSearchInstallModPack.Visibility =
                value == ModComp.CompType.ModPack ? Visibility.Visible : Visibility.Collapsed;
            loader.name = Lang.Text("Download.Comp.List.Source.ResourceFetch", TypeName);
            PanSearchBox.HintText = ModComp.GetCompSearchName(value);
            Load.Text = ModComp.GetCompLoadingName(value);
        }
    } = (ModComp.CompType)(-1);

    #endregion

    private bool isFillingSource;

    /// <summary>
    ///     Bedrock 资源聚合模式：显示全部 BE 资源（不按子分类过滤），并按“下载量 + 版本新旧”综合排序。
    /// </summary>
    public bool IsBedrockAll { get; set; } = false;

    /// <summary>小teto定制：固定为基岩版模式（如 Bedrock 资源页），点重置不回退为 JE。</summary>
    public bool IsBedrockLocked { get; set; } = false;

    /// <summary>
    ///     是否为基岩版模式。BE 模式下使用 CurseForge 基岩版分类（Addon / 资源包 / 世界）。
    /// </summary>
    public bool IsBedrock
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field = value;
            ApplyBedrockUi();
            StartNewSearch();
        }
    } = false;

    // 切换 BE/JE 时更新界面（来源、加载器、版本下拉、按钮高亮）
    private void ApplyBedrockUi()
    {
        if (IsBedrock)
        {
            FillBedrockSourceCombo();
            LabTag.Text = "种类"; // 小teto定制：BE 资源页「标签」改「种类」
            LabLoader.Visibility = Visibility.Collapsed;
            ComboSearchLoader.Visibility = Visibility.Collapsed;
            ComboSearchShaderLoader.Visibility = Visibility.Collapsed;
            // 小teto定制：先同步填充本地 BE 实例版本（立即清掉 Java 残留），再异步加载完整版本库覆盖
            try { FillVersionCombo(ModOtherGames.GetBedrockLocalVersions()); } catch { }
            _ = LoadBedrockVersionsIntoCombo();
        }
        else
        {
            RestoreJavaSourceCombo();
            LabTag.Text = Lang.Text("Download.Comp.Filter.Tag");
            RestoreJavaVersionCombo();
            if (PageType == ModComp.CompType.Shader)
            {
                LabLoader.Visibility = Visibility.Visible;
                ComboSearchLoader.Visibility = Visibility.Collapsed;
                ComboSearchShaderLoader.Visibility = Visibility.Visible;
            }
            else if (PageType == ModComp.CompType.Mod || PageType == ModComp.CompType.ModPack)
            {
                LabLoader.Visibility = Visibility.Visible;
                ComboSearchLoader.Visibility = Visibility.Visible;
                ComboSearchShaderLoader.Visibility = Visibility.Collapsed;
            }
            else
            {
                LabLoader.Visibility = Visibility.Collapsed;
                ComboSearchLoader.Visibility = Visibility.Collapsed;
                ComboSearchShaderLoader.Visibility = Visibility.Collapsed;
            }
        }
    }

    private MyComboBoxItem FindSourceItem(int tag)
    {
        foreach (MyComboBoxItem item in ComboSearchSource.Items)
            if (ModBase.Val(item.Tag) == tag)
                return item;
        return (MyComboBoxItem)ComboSearchSource.Items[0];
    }

    private MyComboBoxItem FindSourceItemByTag(string tag)
    {
        foreach (MyComboBoxItem item in ComboSearchSource.Items)
            if (ModBase.Val(item.Tag).ToString() == tag)
                return item;
        return null;
    }

    // 恢复 JE 来源下拉（全部 / CurseForge / Modrinth）
    private void RestoreJavaSourceCombo()
    {
        isFillingSource = true;
        ComboSearchSource.Items.Clear();
        ComboSearchSource.Items.Add(new MyComboBoxItem { Content = Lang.Text("Common.Option.All"), Tag = 3, IsSelected = true });
        var cf = new MyComboBoxItem { Content = "CurseForge", Tag = 1 };
        cf.Visibility = SupportCurseForge ? Visibility.Visible : Visibility.Collapsed;
        ComboSearchSource.Items.Add(cf);
        var mr = new MyComboBoxItem { Content = "Modrinth", Tag = 2 };
        mr.Visibility = SupportModrinth ? Visibility.Visible : Visibility.Collapsed;
        ComboSearchSource.Items.Add(mr);
        isFillingSource = false;
    }

    // BE 来源：仅 CurseForge 内嵌直取（MCBBS / klpbbs / MCPEDL 无公开 API 且被 Cloudflare 拦截，
    // 无法在启动器内直接获取，按要求移除浏览器跳转入口）
    private void FillBedrockSourceCombo()
    {
        isFillingSource = true;
        ComboSearchSource.Items.Clear();
        ComboSearchSource.Items.Add(new MyComboBoxItem { Content = "CurseForge", Tag = "CF", IsSelected = true });
        isFillingSource = false;
    }

    // 非 CurseForge 跳转源已移除；此处仅保留 CF 内嵌逻辑（无浏览器跳转）
    private void ComboSearchSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (isFillingSource || !IsBedrock)
            return;
        if (ComboSearchSource.SelectedItem is not MyComboBoxItem item)
            return;
        var tag = ModBase.Val(item.Tag).ToString();
        if (string.IsNullOrEmpty(tag) || tag == "CF")
            return;
        // 预留：若未来接入可直取的 BE 源，在此实现内嵌获取，不打开浏览器
        ModBase.RunInUi(() =>
        {
            isFillingSource = true;
            ComboSearchSource.SelectedItem = FindSourceItemByTag("CF");
            isFillingSource = false;
        });
        HintService.Hint("该下载源暂不支持在启动器内直接获取，已切回 CurseForge。");
    }

    // 加载 BE 版本列表到版本下拉（按大版本分组，仅每大版本显示最新若干）
    private async Task LoadBedrockVersionsIntoCombo()
    {
        try
        {
            var versions = (await ModOtherGames.GetBedrockVersionsAsync())
                .Where(v => IsValidBedrockVersion(v.Version))
                .Select(v => v.Version)
                .ToList();
            if (versions.Count == 0)
                versions = GetBedrockVersionsFromResults(); // 版本库无数据时从已加载结果兜底
            FillVersionCombo(versions);
        }
        catch
        {
            // 小teto定制：版本库加载失败时从已加载的资源结果中提取 BE 版本兜底，避免版本下拉残留 Java 版本
            try { FillVersionCombo(GetBedrockVersionsFromResults()); }
            catch { }
        }
    }

    // 小teto定制：填充 BE 版本下拉（大版本分组，倒序）
    private void FillVersionCombo(List<string> versions)
    {
        TextSearchVersion.Items.Clear();
        TextSearchVersion.Items.Add(new MyComboBoxItem
            { Content = Lang.Text("Download.Comp.Filter.Version.Any"), IsSelected = true });
        foreach (var g in versions.GroupBy(BeMajorGroup).OrderByDescending(g => g.Key, BeVersionComparer))
        {
            TextSearchVersion.Items.Add(new MyComboBoxItem { Content = "—— " + g.Key + " ——", IsEnabled = false });
            foreach (var v in g.OrderByDescending(v => v, BeVersionComparer))
                TextSearchVersion.Items.Add(new MyComboBoxItem { Content = v });
        }
    }

    // 小teto定制：从已加载的资源结果中提取 BE 版本（版本库失败时的兜底）
    private List<string> GetBedrockVersionsFromResults()
    {
        var list = new List<string>();
        try
        {
            foreach (var p in storage.results)
            {
                if (p is null) continue;
                foreach (var d in p.Drops)
                {
                    var v = McInstanceInfo.DropToVersion(d);
                    if (string.IsNullOrEmpty(v) || v.Contains("Unknown") || v.Contains("?")) continue;
                    if (!IsValidBedrockVersion(v)) continue;
                    if (!list.Contains(v)) list.Add(v);
                }
            }
        }
        catch { }
        list.Sort(CompareBeVersion);
        list.Reverse();
        return list;
    }

    private static readonly System.Collections.Generic.Comparer<string> BeVersionComparer =
        System.Collections.Generic.Comparer<string>.Create(CompareBeVersion);

    private static int CompareBeVersion(string a, string b)
    {
        var pa = a.Split('.');
        var pb = b.Split('.');
        for (var i = 0; i < Math.Max(pa.Length, pb.Length); i++)
        {
            var x = i < pa.Length && int.TryParse(pa[i], out var n1) ? n1 : 0;
            var y = i < pb.Length && int.TryParse(pb[i], out var n2) ? n2 : 0;
            if (x != y) return x.CompareTo(y);
        }

        return string.Compare(a, b, System.StringComparison.Ordinal);
    }

    // 小teto定制：校验 BE 版本号格式并排除未来脏数据。
    // 合法格式：经典版 1.x（1.0~1.21），年号版 20~26（当前最新 26.x；30.0 等未来/测试条目排除）
    private static bool IsValidBedrockVersion(string version)
    {
        var parts = version.Split('.');
        if (parts.Length < 2) return false;
        if (!int.TryParse(parts[0], out var major)) return false;
        if (major == 1) return int.TryParse(parts[1], out _); // 1.x 经典版
        return major >= 20 && major <= 26; // 年号版 20~26，仅到当前最新大版本
    }

    private static string BeMajorGroup(string version)
    {
        var parts = version.Split('.');
        if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
            return version;
        var major = int.TryParse(parts[0], out var m) ? m : -1;
        if (major >= 10)
        {
            // 年度版本号按大版本归类：26.33 / 26.50 / 26.45 全部归入 "26"（用户要求 26.x 一组）
            return parts[0];
        }
        if (parts.Length >= 2)
            return parts[0] + "." + parts[1]; // 1.19.0.23 → 1.19
        return parts[0];
    }

    private void RestoreJavaVersionCombo()
    {
        var current = TextSearchVersion.Text;
        TextSearchVersion.Items.Clear();
        TextSearchVersion.Items.Add(new MyComboBoxItem
            { Content = Lang.Text("Download.Comp.Filter.Version.Any"), IsSelected = true });
        foreach (var ver in new[] { "1.21.11", "1.21.1", "1.20.6", "1.20.1", "1.19.4", "1.19.2", "1.18.2", "1.16.5", "1.12.2", "1.10.2", "1.8.9", "1.7.10" })
            TextSearchVersion.Items.Add(new MyComboBoxItem { Content = ver });
        TextSearchVersion.Text = current;
    }

    #region 加载

    /// <summary>
    ///     在切换到页面时，应自动将筛选项设置为与该目标 MC 版本和加载器相同。
    /// </summary>
    public static McInstance targetVersion;

    // 在点击 MyCompItem 时会获取 Loader 的输入，以使资源详情页面可以应用相同的筛选项
    public ModLoader.LoaderTask<ModComp.CompProjectRequest, int> loader;

    private bool isLoaderInited;

    public PageComp()
    {
        loader = new ModLoader.LoaderTask<ModComp.CompProjectRequest, int>(Lang.Text("Download.Comp.List.Source.ResourceFetch", "XXX"), ModComp.CompProjectsGet,
            LoaderInput) { reloadTimeout = 60 * 1000 };
        Loaded += PageCompControls_Inited;
        IsVisibleChanged += PageComp_IsVisibleChanged;
        InitializeComponent();
        Load.StateChanged += Load_State;
        BtnPageFirst.Click += BtnPageFirst_Click;
        BtnPageLeft.Click += BtnPageLeft_Click;
        BtnPageRight.Click += BtnPageRight_Click;
        PanSearchBox.Search += (_, _) => StartNewSearch();
        PanSearchBox.KeyDown += EnterTrigger;
        TextSearchVersion.KeyDown += EnterTrigger;
        BtnSearchReset.Click += (_, _) => ResetFilter();
        BtnSearchInstallModPack.Click += BtnSearchInstallModPack_Click;
        ComboSearchSource.SelectionChanged += ComboSearchSource_SelectionChanged;
        // 小teto定制：BE「种类」下拉变化立即触发搜索（否则改选项不刷新结果）
        ComboSearchTag.SelectionChanged += (_, _) =>
        {
            if (isLoaderInited)
                StartNewSearch();
        };
    }

    private void PageCompControls_Inited(object sender, EventArgs e)
    {
        // 不知道从 Initialized 改成 Loaded 会不会有问题，但用 Initialized 会导致初始的筛选器修改被覆盖回默认值
        if (targetVersion is not null)
        {
            // 设置目标
            ResetFilter(); // 重置筛选器
            TextSearchVersion.Text = targetVersion.Info.VanillaName;

            MyComboBoxItem GetTargetItemByName(string name)
            {
                foreach (MyComboBoxItem Item in ComboSearchLoader.Items)
                    if (string.Equals(Item.Content?.ToString(), name, StringComparison.OrdinalIgnoreCase))
                        return Item;
                return (MyComboBoxItem)ComboSearchLoader.Items[0];
            }

            ;
            if (targetVersion.Info.HasForge)
                ComboSearchLoader.SelectedItem = GetTargetItemByName("Forge");
            else if (targetVersion.Info.HasFabric)
                ComboSearchLoader.SelectedItem = GetTargetItemByName("Fabric");
            else if (targetVersion.Info.HasNeoForge)
                ComboSearchLoader.SelectedItem = GetTargetItemByName("NeoForge");
            else if (targetVersion.Info.HasQuilt) ComboSearchLoader.SelectedItem = GetTargetItemByName("Quilt");
            targetVersion = null;
            // 如果已经完成请求，则重新开始
            if (isLoaderInited)
                StartNewSearch();
            ScrollToHome();
        }

        // 加载器初始化
        if (isLoaderInited)
            return;
        isLoaderInited = true;
        // 小teto定制：JE 模式确保版本下拉已填充（XAML 不再硬编码版本项，避免 Items[1] 越界崩溃）
        if (!IsBedrock && TextSearchVersion.Items.Count <= 1)
            RestoreJavaVersionCombo();
        ((MyPageRight)Parent).PageLoaderInit(Load, PanLoad, PanContent, PanAlways, loader, _ => Load_OnFinish(),
            LoaderInput);
        // 将最高 Drop 加入筛选（BE 模式跳过，避免污染 BE 版本列表；JE 模式也需 Items 至少有 2 项）
        if (!IsBedrock && TextSearchVersion.Items.Count > 1 &&
            ModDownload.AllDrops is not null && ModDownload.AllDrops.Count != 0 && ModDownload.AllDrops.First() > 250)
        {
            var highestVersion = McInstanceInfo.DropToVersion(ModDownload.AllDrops.First());
            if ((((MyComboBoxItem)TextSearchVersion.Items[1]).Content.ToString() ?? "") !=
                (highestVersion ?? "")) // 0 是全部
                TextSearchVersion.Items.Insert(1, new MyComboBoxItem { Content = highestVersion });
        }

        // 根据页面类型控制加载器选择的显示
        if (PageType == ModComp.CompType.Shader)
        {
            LabLoader.Visibility = Visibility.Visible;
            ComboSearchLoader.Visibility = Visibility.Collapsed;
            ComboSearchShaderLoader.Visibility = Visibility.Visible;
        }
        else if (PageType == ModComp.CompType.Mod || PageType == ModComp.CompType.ModPack)
        {
            LabLoader.Visibility = Visibility.Visible;
            ComboSearchLoader.Visibility = Visibility.Visible;
            ComboSearchShaderLoader.Visibility = Visibility.Collapsed;
        }
        else
        {
            LabLoader.Visibility = Visibility.Collapsed;
            ComboSearchLoader.Visibility = Visibility.Collapsed;
            ComboSearchShaderLoader.Visibility = Visibility.Collapsed;
        }
    }

    private void PageComp_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // 当页面变为可见时刷新收藏按钮状态
        if (IsVisible) RefreshAllFavoriteStatus();
    }

    private ModComp.CompProjectRequest LoaderInput()
    {
        var request = new ModComp.CompProjectRequest(PageType, storage, (page + 1) * pageSize);
        var gameVersion = TextSearchVersion.Text == Lang.Text("Download.Comp.Filter.Version.AllInputAvailable") ? null :
            TextSearchVersion.Text.Contains(".") || TextSearchVersion.Text.Contains("w") ? TextSearchVersion.Text :
            null;
        var modLoader = ModComp.CompLoaderType.Any;
        if (PageType == ModComp.CompType.Mod || PageType == ModComp.CompType.ModPack) // 只有 Mod 考虑加载器
        {
            modLoader = (ModComp.CompLoaderType)ModBase.Val(((MyComboBoxItem)ComboSearchLoader.SelectedItem).Tag);
            if (gameVersion is not null && gameVersion.Contains(".") && ModBase.Val(gameVersion.Split(".")[1]) < 14d &&
                modLoader == ModComp.CompLoaderType.Forge) // 1.14-
                                                           // 选择了 Forge
                modLoader = ModComp.CompLoaderType.Any; // 此时，视作没有筛选 Mod Loader（因为部分老 Mod 没有设置自己支持的加载器）
        }

        request.searchText = PanSearchBox.Text;
        request.gameVersion = gameVersion;
        var selectedTag = (ComboSearchTag.SelectedItem as FrameworkElement)?.Tag?.ToString();
        var loaderTag = (ComboSearchShaderLoader.SelectedItem as FrameworkElement)?.Tag?.ToString();

        request.tag = PageType == ModComp.CompType.Shader
            ? string.IsNullOrEmpty(loaderTag)
                ? selectedTag
                : selectedTag + loaderTag
            : selectedTag;
        request.modLoader =
            (ModComp.CompLoaderType)(PageType == ModComp.CompType.Mod || PageType == ModComp.CompType.ModPack
                ? ModBase.Val(((MyComboBoxItem)ComboSearchLoader.SelectedItem).Tag)
                : (double)ModComp.CompLoaderType.Any);
        request.isBedrock = IsBedrock;
        request.isBedrockAll = IsBedrockAll;
        if (IsBedrock)
        {
            request.source = ModComp.CompSourceType.CurseForge; // BE 仅使用 CurseForge
            request.modLoader = ModComp.CompLoaderType.Any;
            // 小teto定制：「种类」下拉映射 CurseForge classId（0=全部, 4984=Add-On, 6913=地图, 6929=材质包）
            request.tag = "";
            // 小teto定制：空值保护——SelectedItem 为空时按「全部」(0) 处理，避免列表加载失败
            request.bedrockClassFilter = ComboSearchTag.SelectedItem is MyComboBoxItem selTag
                ? (int)ModBase.Val(selTag.Tag)
                : 0;
        }
        else
        {
            request.source = (ModComp.CompSourceType)ModBase.Val(((MyComboBoxItem)ComboSearchSource.SelectedItem).Tag);
        }

        request.sort = (ModComp.CompSortType)ModBase.Val(((MyComboBoxItem)ComboSearchSort.SelectedItem).Tag);
        return request;
    }

    #endregion

    #region 搜索

    // 搜索按钮
    private void StartNewSearch()
    {
        page = 0;
        object argInput = LoaderInput();
        if (loader.ShouldStart(ref argInput))
            storage = new ModComp.CompProjectStorage(); // 避免连续搜索两次使得 CompProjectStorage 引用丢失（#1311）
        loader.Start();
    }

    private void EnterTrigger(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            StartNewSearch();
    }

    // 重置按钮
    private void ResetFilter()
    {
        PanSearchBox.Text = "";
        TextSearchVersion.Text = Lang.Text("Download.Comp.Filter.Version.AllInputAvailable");
        TextSearchVersion.SelectedIndex = 0;
        ComboSearchSource.SelectedIndex = 0;
        ComboSearchTag.SelectedIndex = 0;
        ComboSearchLoader.SelectedIndex = 0;
        ComboSearchShaderLoader.SelectedIndex = 0;
        ComboSearchSort.SelectedIndex = 0;
        if (!IsBedrockLocked) IsBedrock = false; // 重置按钮可将 JE/BE 切回 JE（固定 BE 页保持 BE）
        loader.lastFinishedTime = 0L; // 要求强制重新开始
    }

    private void BtnSearchReset_Click(object sender, EventArgs e)
    {
        ResetFilter();
    }

    #endregion
}