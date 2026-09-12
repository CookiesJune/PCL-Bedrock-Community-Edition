using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using PCL.Core.App;
using PCL.Core.App.Configuration;
using PCL.Core.App.Configuration.Storage;
using FileSystem = Microsoft.VisualBasic.FileIO.FileSystem;
using PCL.Core.App.Localization;
using PCL.Core.UI;

namespace PCL;

public partial class PageSelectRight
{
    private const int normalDelay = 75; // 正常输入延迟0.075秒
    private const int quickDelay = 50; // 清空搜索框延迟0.05秒
    private bool isRefreshing;

    private DateTime lastInputTime = DateTime.MinValue;
    private DispatcherTimer reloadTimer;

    // 窗口属性
    /// <summary>
    ///     是否显示隐藏的 Minecraft 实例。
    /// </summary>
    public bool showHidden = false;

    public PageSelectRight()
    {
        InitializeComponent();
        PanVerSearchBox.HintText = Lang.Text("Select.Instance.Search.Hint");
        Loaded += PageSelectRight_Loaded;
        Unloaded += PageSelectRight_Unloaded;
        // 小teto定制：每次显示实例选择页时处理待进入的 BE 版本介绍页（修复点"实例设置"停在版本列表的问题）
        IsVisibleChanged += PageSelectRight_IsVisibleChanged;
        LoaderInit();
    }

    private void PageSelectRight_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!IsVisible) return;
        TryOpenPendingBeInfo();
    }

    /// <summary>小teto定制：消费待进入的 BE 版本介绍页（从启动页「实例设置」跳转时设置）。</summary>
    private void TryOpenPendingBeInfo()
    {
        try
        {
            if (string.IsNullOrEmpty(ModOtherGames.PendingBeInfoDir)) return;
            if (!ModOtherGames.IsBedrockFolder(ModFolder.mcFolderSelected)) return;
            var pd = ModOtherGames.PendingBeInfoDir;
            ModOtherGames.PendingBeInfoDir = "";
            if (Directory.Exists(pd)) ShowBeVersionInfo(pd);
        }
        catch
        {
            // 忽略
        }
    }

    // 窗口基础
    private void PageSelectRight_Loaded(object sender, RoutedEventArgs e)
    {
        // 小teto定制：若已处于 BE 版本介绍页，保持介绍页不被覆盖
        if (_beInfoMode)
        {
            RenderBeInfo();
        }
        // 小teto定制：BE 文件夹直接显示基岩版版本列表，不跑 Java 实例扫描（避免列表"时显时隐"）
        else if (ModOtherGames.IsBedrockFolder(ModFolder.mcFolderSelected))
        {
            RefreshBeList();
        }
        else
        {
            ModLoader.LoaderFolderRun(ModInstanceList.mcInstanceListLoader, ModFolder.mcFolderSelected,
                ModLoader.LoaderFolderRunType.RunOnUpdated, 1, @"versions\");
        }
        PanBack.ScrollToHome();
        PanVerSearchBox.TextChanged += (a, b) => PanVerSearchBox_TextChanged(a, (TextChangedEventArgs)b);

        reloadTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(normalDelay) };
        reloadTimer.Tick += ReloadTimer_Tick;
    }

    private void PanVerSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // 记录最后一次输入时间
        lastInputTime = DateTime.Now;

        isRefreshing = false;

        // 动态调整延迟时间
        if (string.IsNullOrWhiteSpace(PanVerSearchBox.Text))
        {
            if (reloadTimer.Interval.TotalMilliseconds != quickDelay)
                reloadTimer.Interval = TimeSpan.FromMilliseconds(quickDelay);
        }
        else if (reloadTimer.Interval.TotalMilliseconds != normalDelay)
        {
            reloadTimer.Interval = TimeSpan.FromMilliseconds(normalDelay);
        }


        if (!reloadTimer.IsEnabled) reloadTimer.Start();
    }

    private void ReloadTimer_Tick(object sender, EventArgs e)
    {
        // 检查是否超过当前设定的延迟时间没有新输入
        var elapsed = (DateTime.Now - lastInputTime).TotalMilliseconds;
        var currentDelay = reloadTimer.Interval.TotalMilliseconds;

        // 小teto定制：BE 文件夹下不依赖 Java 实例扫描结果，直接刷新版本列表
        var isBeNow = ModOtherGames.IsBedrockFolder(ModFolder.mcFolderSelected);

        if (elapsed >= currentDelay && (isBeNow || ModInstanceList.mcInstanceListLoader.State == ModBase.LoadState.Finished) &&
            !isRefreshing)
        {
            isRefreshing = true;

            // 确保在UI线程执行刷新
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (isBeNow)
                    RefreshBeList();
                else
                    McInstanceListUI(ModInstanceList.mcInstanceListLoader);
                isRefreshing = false;
            }));
            reloadTimer.Stop();
        }
    }

    private void PageSelectRight_Unloaded(object sender, RoutedEventArgs e)
    {
        // 清理计时器
        if (reloadTimer is not null)
        {
            reloadTimer.Stop();
            reloadTimer.Tick -= ReloadTimer_Tick;
            reloadTimer = null;
        }
        // 小teto定制：切离页面时重置 BE 介绍页状态（避免返回后版本列表不显示/页面错乱）
        _beInfoMode = false;
        _beInfoDir = "";
    }

    private void LoaderInit()
    {
        PageLoaderInit(Load, PanLoad, PanAllBack, null, ModInstanceList.mcInstanceListLoader,
            a => this.McInstanceListUI((ModLoader.LoaderTask<string, int>)a),
            autoRun: false);
    }

    private void Load_Click(object sender, MouseButtonEventArgs e)
    {
        // 小teto定制：BE 文件夹直接刷新版本列表，不依赖 Java 实例扫描
        if (ModOtherGames.IsBedrockFolder(ModFolder.mcFolderSelected))
        {
            RefreshBeList();
            return;
        }
        if (ModInstanceList.mcInstanceListLoader.State == ModBase.LoadState.Failed)
            ModLoader.LoaderFolderRun(ModInstanceList.mcInstanceListLoader, ModFolder.mcFolderSelected,
                ModLoader.LoaderFolderRunType.ForceRun, 1, @"versions\");
    }

    #region 结果 UI 化

    private void McInstanceListUI(ModLoader.LoaderTask<string, int> loader)
    {
        try
        {
            var path = loader.input;
            // 加载 UI
            PanMain.Children.Clear();

            // 小teto定制：BE 介绍页模式时不刷新版本列表
            if (_beInfoMode)
            {
                RenderBeInfo();
                return;
            }

            // 小teto定制：BE 文件夹实例 -> 显示基岩版版本列表（倒三角折叠，点击标题可收起）
            if (ModOtherGames.IsBedrockFolder(ModFolder.mcFolderSelected))
            {
                RefreshBeList();
                // 小teto定制：从启动页「实例设置」跳转来时自动进入对应版本的介绍页
                TryOpenPendingBeInfo();
                return;
            }

            var hasVisibleFolders = false;
            var searchText = PanVerSearchBox.Text.Trim().ToLower(); // 获取搜索框文本
            var hasAnyResults = false;
            var originalHasInstances = ModInstanceList.mcInstanceList.ToArray().Any(c => c.Value.Count > 0);

            // 搜索无结果时显示 PanEmptySearch
            PanEmptySearch.Visibility = Visibility.Collapsed; // 默认隐藏

            foreach (var Card in ModInstanceList.mcInstanceList.ToArray())
            {
                if ((Card.Key == McInstanceCardType.Hidden) ^ showHidden)
                    continue;
                var filteredInstances = Card.Value.Where(v =>
                {
                    if (string.IsNullOrEmpty(searchText))
                        return true;
                    return v.Name.ToLower().Contains(searchText) ||
                           (v.Desc is not null && v.Desc.ToLower().Contains(searchText)) || v.GetDefaultDescription()
                               .Replace(",", "").ToLower().Trim().Contains(searchText);
                }).ToList();
                if (filteredInstances.Count == 0)
                    continue;

                hasVisibleFolders = true;
                hasAnyResults = true;
                if (filteredInstances.Count == 0)
                    continue;
                hasVisibleFolders = true;

                #region 确认卡片名称

                var cardName = "";
                switch (Card.Key)
                {
                    case McInstanceCardType.OriginalLike:
                    {
                        cardName = Lang.Text("Select.Instance.Card.Regular");
                        break;
                    }
                    case McInstanceCardType.API:
                    {
                        var isForgeExists = false;
                        var isNeoForgeExists = false;
                        var isFabricExists = false;
                        var isQuiltExists = false;
                        var isLiteExists = false;
                        var isCleanroomExists = false;
                        var isLabyModExists = false;
                        foreach (var instance in Card.Value)
                        {
                            if (!instance.IsLoaded)
                                instance.Load();
                            if (instance.Info.HasFabric)
                                isFabricExists = true;
                            if (instance.Info.HasQuilt)
                                isQuiltExists = true;
                            if (instance.Info.HasLiteLoader)
                                isLiteExists = true;
                            if (instance.Info.HasForge)
                                isForgeExists = true;
                            if (instance.Info.HasNeoForge)
                                isNeoForgeExists = true;
                            if (instance.Info.HasCleanroom)
                                isCleanroomExists = true;
                            if (instance.Info.HasLabyMod)
                                isLabyModExists = true;
                        }

                        if ((isLiteExists ? 1 : 0) + (isForgeExists ? 1 : 0) + (isFabricExists ? 1 : 0) +
                            (isNeoForgeExists ? 1 : 0) + (isQuiltExists ? 1 : 0) + (isCleanroomExists ? 1 : 0) +
                            (isLabyModExists ? 1 : 0) > 1)
                            cardName = Lang.Text("Select.Instance.Card.Modable");
                        else if (isForgeExists)
                            cardName = Lang.Text("Select.Instance.Card.Forge");
                        else if (isNeoForgeExists)
                            cardName = Lang.Text("Select.Instance.Card.NeoForge");
                        else if (isCleanroomExists)
                            cardName = Lang.Text("Select.Instance.Card.Cleanroom");
                        else if (isLabyModExists)
                            cardName = Lang.Text("Select.Instance.Card.LabyMod");
                        else if (isLiteExists)
                            cardName = Lang.Text("Select.Instance.Card.LiteLoader");
                        else if (isQuiltExists)
                            cardName = Lang.Text("Select.Instance.Card.Quilt");
                        else
                            cardName = Lang.Text("Select.Instance.Card.Fabric");

                        break;
                    }
                    case McInstanceCardType.Error:
                    {
                        cardName = Lang.Text("Select.Instance.Card.Error");
                        break;
                    }
                    case McInstanceCardType.Hidden:
                    {
                        cardName = Lang.Text("Select.Instance.Card.Hidden");
                        break;
                    }
                    case McInstanceCardType.Rubbish:
                    {
                        cardName = Lang.Text("Select.Instance.Card.LessUsed");
                        break;
                    }
                    case McInstanceCardType.Star:
                    {
                        cardName = Lang.Text("Select.Instance.Card.Favorites");
                        break;
                    }
                    case McInstanceCardType.Fool:
                    {
                        cardName = Lang.Text("Select.Instance.Card.AprilFools");
                        break;
                    }

                    default:
                    {
                        throw new ArgumentException($"未知的卡片种类（{(int)Card.Key}）");
                    }
                }

                #endregion

                // 建立控件
                var cardTitle = $"{cardName}{(Card.Key == McInstanceCardType.Star ? "" : $" ({Lang.Number(filteredInstances.Count, "N0")})")}";
                var newCard = new MyCard { Title = cardTitle, Margin = new Thickness(0d, 0d, 0d, 15d) };
                var newStack = new StackPanel
                {
                    Margin = new Thickness(20d, MyCard.SwapedHeight, 18d, 0d),
                    VerticalAlignment = VerticalAlignment.Top, RenderTransform = new TranslateTransform(0d, 0d),
                    Tag = filteredInstances
                };
                newCard.Children.Add(newStack);
                newCard.SwapControl = newStack;
                PanMain.Children.Add(newCard);

                // 确定卡片是否展开
                void PutMethod(StackPanel stack)
                {
                    foreach (var item in (IEnumerable)stack.Tag)
                        stack.Children.Add(McVersionListItem((McInstance)item));
                }

                ;
                if (Card.Key == McInstanceCardType.Rubbish ||
                    Card.Key == McInstanceCardType.Error ||
                    Card.Key == McInstanceCardType.Fool)
                {
                    newCard.IsSwapped = true;
                    newCard.InstallMethod = PutMethod;
                }
                else
                {
                    MyCard.StackInstall(ref newStack, PutMethod);
                }
            }

            // 若只有一个卡片，则强制展开
            if (PanMain.Children.Count == 1 && ((MyCard)PanMain.Children[0]).IsSwapped)
                ((MyCard)PanMain.Children[0]).IsSwapped = false;

            PanVerSearchBox.Visibility = hasVisibleFolders ? Visibility.Visible : Visibility.Collapsed;

            // 判断应该显示哪一个页面
            if (!hasAnyResults)
            {
                if (!originalHasInstances)
                {
                    // 完全没有实例的情况
                    PanEmpty.Visibility = Visibility.Visible;
                    PanBack.Visibility = Visibility.Collapsed;
                    if (showHidden)
                    {
                        LabEmptyTitle.Text = Lang.Text("Select.Instance.Hidden.EmptyTitle");
                        LabEmptyContent.Text = Lang.Text("Select.Instance.Hidden.EmptyMessage");
                        BtnEmptyDownload.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        LabEmptyTitle.Text = Lang.Text("Select.Instance.Empty.Title");
                        LabEmptyContent.Text = Lang.Text("Select.Instance.Empty.Message");
                        BtnEmptyDownload.Visibility =
                            Config.Preference.Hide.PageDownload && !PageSetupUI.HiddenForceShow
                                ? Visibility.Collapsed
                                : Visibility.Visible;
                    }
                }
                // 有实例但搜索无结果的情况
                else if (showHidden && ModInstanceList.mcInstanceList.ToArray().Any(c =>
                             c.Key == McInstanceCardType.Hidden && c.Value.Count > 0))
                {
                    // 有隐藏实例但搜索无结果 - 显示搜索无结果提示
                    PanVerSearchBox.Visibility = Visibility.Visible;
                    PanEmpty.Visibility = Visibility.Collapsed;
                    PanBack.Visibility = Visibility.Visible;
                    PanEmptySearch.Visibility = Visibility.Visible;
                    LabEmptySearchTitle.Text = Lang.Text("Select.Instance.Hidden.EmptySearchTitle");
                    LabEmptySearchContent.Text = string.IsNullOrWhiteSpace(searchText)
                        ? Lang.Text("Select.Instance.Search.EmptyInput")
                        : Lang.Text("Select.Instance.Search.NoHiddenResult", searchText);
                }
                else if (showHidden)
                {
                    // 无隐藏实例 - 显示"无隐藏实例"提示
                    PanEmpty.Visibility = Visibility.Visible;
                    PanBack.Visibility = Visibility.Collapsed;
                    LabEmptyTitle.Text = Lang.Text("Select.Instance.Hidden.EmptyTitle");
                    LabEmptyContent.Text = Lang.Text("Select.Instance.Hidden.EmptyMessage");
                    BtnEmptyDownload.Visibility = Visibility.Collapsed;
                    PanVerSearchBox.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // 普通模式下的搜索无结果
                    PanVerSearchBox.Visibility = Visibility.Visible;
                    PanEmpty.Visibility = Visibility.Collapsed;
                    PanBack.Visibility = Visibility.Visible;
                    PanEmptySearch.Visibility = Visibility.Visible;
                    LabEmptySearchTitle.Text = Lang.Text("Select.Instance.EmptySearch.Title");
                    LabEmptySearchContent.Text = string.IsNullOrWhiteSpace(searchText)
                        ? Lang.Text("Select.Instance.Search.EmptyInput")
                        : Lang.Text("Select.Instance.Search.NoResult", searchText);
                }
            }
            else
            {
                PanBack.Visibility = Visibility.Visible;
                PanEmpty.Visibility = Visibility.Collapsed;
                PanEmptySearch.Visibility = Visibility.Collapsed;
            } // 有结果时隐藏
        }


        catch (Exception ex)
        {
            ModBase.Log(
                ex,
                Lang.Text("Select.Instance.Error.UiUpdate"),
                ModBase.LogLevel.Feedback,
                userSummary: Lang.Text("Select.Instance.Error.UiUpdate"));
        }
    }

    private bool _beInfoMode = false;
    private string _beInfoDir = "";

    /// <summary>小teto定制：进入 BE 版本介绍页（使用 Java 版 PageInstanceOverall 页面）。</summary>
    public void ShowBeVersionInfo(string versionDir)
    {
        // 设置当前选中的 BE 版本
        ModOtherGames.SelectedBedrockVersion = Path.GetFileName(versionDir);
        States.Game.SelectedInstance = ModOtherGames.SelectedBedrockVersion;
        try { ModBase.WriteIni(ModFolder.mcFolderSelected + "PCL.ini", "Version", ModOtherGames.SelectedBedrockVersion); } catch { }
        // 创建 McInstance 对象（传入绝对路径，PathInstance 就是 BE 版本路径）
        PageInstanceLeft.McInstance = new McInstance(versionDir);
        // 强制刷新左侧导航栏，确保BE专属导航项隐藏/显示（关键：必须在 PageChange 之前调用）
        try { PageInstanceLeft.CurrentInstance?.RefreshModDisabled(); } catch { }
        // 跳转到 Java 版实例概览页面（PageInstanceOverall 会自动检测 BE 实例并适配）
        ModMain.frmMain.PageChange(FormMain.PageType.InstanceSetup);
    }

    private void ExitBeVersionInfo()
    {
        _beInfoMode = false;
        _beInfoDir = "";
        if (ModOtherGames.IsBedrockFolder(ModFolder.mcFolderSelected))
            RefreshBeList();
    }

    private void RefreshBeList()
    {
        var bvFolders = ModOtherGames.FindBedrockVersionFolders(ModFolder.mcFolderSelected);
        PanMain.Children.Clear();
        // 小teto定制：按类型倒三角折叠分类（预览版 Preview / 正式版 Release / Beta 版 Beta）
        if (bvFolders.Count == 0)
            PanMain.Children.Add(new TextBlock
            {
                Text = "未检测到基岩版版本。",
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.6,
                Margin = new Thickness(8)
            });
        else
        {
            var preview = new List<string>();
            var release = new List<string>();
            var beta = new List<string>();
            foreach (var vdir in bvFolders)
            {
                switch (ClassifyBeVersion(vdir))
                {
                    case "preview": preview.Add(vdir); break;
                    case "beta": beta.Add(vdir); break;
                    default: release.Add(vdir); break;
                }
            }
            AddBeGroup(preview, "预览版（Preview）");
            AddBeGroup(release, "正式版（Release）");
            AddBeGroup(beta, "Beta 版（Beta）");
        }
        PanVerSearchBox.Visibility = Visibility.Visible;
        PanEmpty.Visibility = Visibility.Collapsed;
        PanBack.Visibility = Visibility.Visible;
        PanEmptySearch.Visibility = Visibility.Collapsed;
    }

    /// <summary>小teto定制：渲染一个倒三角折叠分类组（折叠头单独一行，杜绝与版本行错位重叠）。</summary>
    private void AddBeGroup(List<string> folders, string title)
    {
        // 空组不显示（避免出现空标题）
        if (folders.Count == 0)
            return;
        // 小teto定制：使用 Java 同款 MyCard 折叠卡片（带动画、右侧箭头）
        var card = new MyCard
        {
            Title = title + "（" + folders.Count + "）",
            Margin = new Thickness(0, 12, 0, 0),
            CanSwap = true,
            SwapLogoRight = true,
            IsSwapped = false  // 默认展开
        };
        var content = new StackPanel
        {
            Margin = new Thickness(20, 40, 18, 15),  // 顶部 40 避开标题区域（与 Java 下载页一致）
            VerticalAlignment = VerticalAlignment.Top
        };
        foreach (var v in folders)
            content.Children.Add(BedrockVersionListItem(v));
        card.Children.Add(content);  // Children[3] = SwapControl
        PanMain.Children.Add(card);
    }

    /// <summary>小teto定制：对 BE 版本目录分类（preview 预览版 / release 正式版 / beta）。</summary>
    private static string ClassifyBeVersion(string dir)
    {
        try
        {
            var name = Path.GetFileName(dir).ToLowerInvariant();
            if (name.Contains("beta")) return "beta";
            if (name.Contains("preview") || name.Contains("education")) return "preview";
            var m = System.Text.RegularExpressions.Regex.Match(Path.GetFileName(dir), @"^(\d+)\.(\d+)\.(\d+)");
            if (m.Success && int.TryParse(m.Groups[1].Value, out var major) && major >= 26) return "preview";
        }
        catch { }
        return "release";
    }

    /// <summary>小teto定制：渲染 BE 版本介绍页（图3：版本号 + BE 版本号 + 实例文件夹/全局资源/存档 + 个性化）。</summary>
    private void RenderBeInfo()
    {
        var dir = _beInfoDir;
        var name = Path.GetFileName(dir);
        PanMain.Children.Clear();
        PanVerSearchBox.Visibility = Visibility.Collapsed;
        PanEmpty.Visibility = Visibility.Collapsed;
        PanEmptySearch.Visibility = Visibility.Collapsed;
        PanBack.Visibility = Visibility.Visible;

        // 顶部：返回 + 版本号
        var topBar = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var btnBack = new MyButton { Text = "< 返回", Height = 32, MinWidth = 90, Margin = new Thickness(0, 0, 14, 0) };
        btnBack.Click += (_, _) =>
        {
            // 小teto定制：介绍页的返回按钮直接返回启动页面（而非停留在实例选择/版本列表）
            _beInfoMode = false;
            _beInfoDir = "";
            try
            {
                ModMain.frmMain.PageChange(FormMain.PageType.Launch);
            }
            catch
            {
                ModMain.frmMain.PageBack();
            }
        };
        topBar.Children.Add(btnBack);
        // 小teto定制：顶部显示实例图标
        var beLogo = "pack://application:,,,/images/Custom/BedrockIconLarge.png";
        try
        {
            var customLogo = States.Instance.LogoPath[dir];
            var isCustom = States.Instance.IsLogoCustom[dir];
            if (isCustom || (!string.IsNullOrEmpty(customLogo) && !customLogo.Contains("BedrockIconLarge")))
            {
                var logoFile = Path.Combine(dir, "PCL", "Logo.png");
                if (File.Exists(logoFile)) beLogo = logoFile;
                else if (!string.IsNullOrEmpty(customLogo) && customLogo.StartsWith("pack:")) beLogo = customLogo;
            }
        }
        catch { }
        var imgLogo = new Image { Source = new MyBitmap(beLogo), Width = 34, Height = 34, Margin = new Thickness(0, 0, 12, 0), Stretch = Stretch.Uniform };
        topBar.Children.Add(imgLogo);
        Grid.SetColumn(imgLogo, 1);
        var title = new TextBlock
        {
            Text = name,
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = (Brush)ThemeManager.AppResources["ColorBrush3"]
        };
        topBar.Children.Add(title);
        Grid.SetColumn(title, 2);
        PanMain.Children.Add(topBar);

        var sub = new TextBlock
        {
            Text = "BE  " + name,
            FontSize = 14,
            Margin = new Thickness(2, 0, 0, 10),
            Opacity = 0.7
        };
        PanMain.Children.Add(sub);

        // 实例信息（和 Java 版概览页面一样的样式：MyListItem 横向排列）
        var card1 = new MyCard { Title = "实例信息", UseAnimation = false, Margin = new Thickness(0, 0, 0, 15) };
        var sp1 = new StackPanel { Margin = new Thickness(15, 40, 25, 5) };
        var infoWrap = new WrapPanel { Margin = new Thickness(0, -5, -20, 7) };
        int launchCount = 0;
        try { launchCount = States.Instance.LaunchCount[dir]; } catch { }
        // 启动次数
        infoWrap.Children.Add(new MyListItem
        {
            Title = "启动次数",
            Info = launchCount > 0 ? launchCount.ToString("N0") : "从未启动",
            Logo = "pack://application:,,,/images/Blocks/RedstoneLampOn.png"
        });
        infoWrap.Children.Add(new TextBlock { Width = 2d });
        // 包类型
        string pkgType = ModOtherGames.IsUwpFolder(dir) ? "UWP 包" : "GDK 包";
        infoWrap.Children.Add(new MyListItem
        {
            Title = "包类型",
            Info = pkgType,
            Logo = "pack://application:,,,/images/Custom/BedrockIconLarge.png"
        });
        infoWrap.Children.Add(new TextBlock { Width = 2d });
        // 基岩版版本
        infoWrap.Children.Add(new MyListItem
        {
            Title = "基岩版",
            Info = name,
            Logo = "pack://application:,,,/images/Blocks/Grass.png"
        });
        infoWrap.Children.Add(new TextBlock { Width = 2d });
        // 游戏时长
        long playSeconds = 0;
        try { playSeconds = ModOtherGames.GetBedrockPlayTime(dir); } catch { }
        infoWrap.Children.Add(new MyListItem
        {
            Title = "游戏时长",
            Info = ModOtherGames.FormatPlayTime(playSeconds),
            Logo = "pack://application:,,,/images/Blocks/GoldBlock.png"
        });
        infoWrap.Children.Add(new TextBlock { Width = 2d });
        sp1.Children.Add(infoWrap);
        card1.Children.Add(sp1);
        PanMain.Children.Add(card1);

        // 快捷方式（和 Java 版一样的按钮样式）
        var card2 = new MyCard { Title = "快捷方式", UseAnimation = false, Margin = new Thickness(0, 0, 0, 15) };
        var sp2 = new StackPanel { Margin = new Thickness(25, 40, 25, 15) };
        var shortcutGrid = new Grid { Margin = new Thickness(0, 2, 0, 7), Height = 35 };
        shortcutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        shortcutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        shortcutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        var btnFolderVersion = new MyButton { Text = "实例文件夹", MinWidth = 140, Padding = new Thickness(13, 0, 13, 0), Margin = new Thickness(0, 0, 20, 0) };
        btnFolderVersion.Click += (_, _) => OpenBeFolder(dir);
        shortcutGrid.Children.Add(btnFolderVersion);
        var btnFolderSaves = new MyButton { Text = "存档文件夹", MinWidth = 140, Padding = new Thickness(13, 0, 13, 0), Margin = new Thickness(0, 0, 20, 0) };
        Grid.SetColumn(btnFolderSaves, 1);
        btnFolderSaves.Click += (_, _) => HintService.Hint("暂时无法读取到该实例的存档，请自行寻找", HintType.Warning);
        shortcutGrid.Children.Add(btnFolderSaves);
        var btnFolderBp = new MyButton { Text = "行为包文件夹", MinWidth = 140, Padding = new Thickness(13, 0, 13, 0), Margin = new Thickness(0, 0, 20, 0) };
        Grid.SetColumn(btnFolderBp, 2);
        btnFolderBp.Click += (_, _) => OpenBeFolder(Path.Combine(dir, "data", "behavior_packs"));
        shortcutGrid.Children.Add(btnFolderBp);
        sp2.Children.Add(shortcutGrid);
        card2.Children.Add(sp2);
        PanMain.Children.Add(card2);

        // 实例设置（小teto定制：参考 Java 实例设置，仅保留「模组」「资源包」，其余功能隐藏）
        var cardSettings = new MyCard { Title = "实例设置", UseAnimation = false, Margin = new Thickness(0, 0, 0, 8) };
        var spSettings = new StackPanel { Margin = new Thickness(14, 36, 14, 8) };
        spSettings.Children.Add(new TextBlock { Text = "模组（data\\behavior_packs\\）", FontSize = 13, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 2, 0, 4) });
        spSettings.Children.Add(MakeBePackPanel(dir, "behavior_packs", "模组"));
        spSettings.Children.Add(new TextBlock { Text = "资源包（data\\resource_packs\\）", FontSize = 13, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 12, 0, 4) });
        spSettings.Children.Add(MakeBePackPanel(dir, "resource_packs", "资源包"));
        // 修改实例名 = 修改文件夹名
        spSettings.Children.Add(new TextBlock { Text = "修改实例名（重命名实例文件夹）", FontSize = 13, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 12, 0, 4) });
        var renameRow = new Grid { Margin = new Thickness(0, 0, 0, 2) };
        renameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        renameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        var renameBox = new MyTextBox { Height = 30, Text = name, Margin = new Thickness(0, 0, 10, 0), VerticalContentAlignment = VerticalAlignment.Center };
        var btnRename = new MyButton { Text = "重命名", Height = 30, MinWidth = 80 };
        btnRename.Click += (_, _) =>
        {
            var newName = renameBox.Text.Trim();
            if (string.IsNullOrEmpty(newName) || newName == name)
            {
                HintService.Hint("名称未改变。", HintType.Warning);
                return;
            }
            var parentDir = Path.GetDirectoryName(dir);
            var newPath = Path.Combine(parentDir, newName);
            if (Directory.Exists(newPath))
            {
                HintService.Hint("目标文件夹已存在。", HintType.Error);
                return;
            }
            try
            {
                Directory.Move(dir, newPath);
                ModOtherGames.SelectedBedrockVersion = newName;
                try { ModBase.WriteIni(ModFolder.mcFolderSelected + "PCL.ini", "Version", newName); } catch { }
                HintService.Hint("实例已重命名为：" + newName, HintType.Success);
                _beInfoDir = newPath;
                RenderBeInfo();
            }
            catch (Exception ex)
            {
                HintService.Hint("重命名失败：" + ex.Message, HintType.Error);
            }
        };
        renameRow.Children.Add(renameBox);
        Grid.SetColumn(btnRename, 1);
        renameRow.Children.Add(btnRename);
        spSettings.Children.Add(renameRow);
        // 图标选择（加入基岩版图标）
        spSettings.Children.Add(new TextBlock { Text = "实例图标", FontSize = 13, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 12, 0, 4) });
        var iconRow = new Grid { Margin = new Thickness(0, 0, 0, 2) };
        iconRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var iconCombo = new MyComboBox { Height = 30, VerticalAlignment = VerticalAlignment.Center };
        iconCombo.Items.Add(new MyComboBoxItem { Content = "默认", Tag = "" });
        iconCombo.Items.Add(new MyComboBoxItem { Content = "基岩版", Tag = "pack://application:,,,/images/Custom/BedrockIconLarge.png" });
        iconCombo.Items.Add(new MyComboBoxItem { Content = "基岩版预览", Tag = "pack://application:,,,/images/Custom/BedrockPreviewIcon.png" });
        iconCombo.Items.Add(new MyComboBoxItem { Content = "自定义…", Tag = "__CUSTOM__" });
        string curLogo = "";
        try { curLogo = States.Instance.LogoPath[dir]; } catch { }
        bool curCustom = false;
        try { curCustom = States.Instance.IsLogoCustom[dir]; } catch { }
        if (curCustom || (!string.IsNullOrEmpty(curLogo) && !curLogo.Contains("BedrockIconLarge") && !curLogo.Contains("BedrockPreviewIcon")))
            iconCombo.SelectedIndex = 2;
        else if (curLogo.Contains("BedrockIconLarge"))
            iconCombo.SelectedIndex = 1;
        else
            iconCombo.SelectedIndex = 0;
        iconCombo.SelectionChanged += (_, _) =>
        {
            try
            {
                var sel = iconCombo.SelectedItem as MyComboBoxItem;
                var tag = sel?.Tag?.ToString() ?? "";
                if (tag == "__CUSTOM__")
                {
                    var fileName = SystemDialogs.SelectFile(Lang.Text("Instance.Overall.Icon.SelectFile.Filter"), Lang.Text("Instance.Overall.Icon.SelectFile.Title"));
                    if (string.IsNullOrEmpty(fileName))
                    {
                        iconCombo.SelectedIndex = 1;
                        return;
                    }
                    try { Directory.CreateDirectory(Path.Combine(dir, "PCL")); } catch { }
                    try { File.Copy(fileName, Path.Combine(dir, "PCL", "Logo.png"), true); } catch { }
                    States.Instance.LogoPath[dir] = Path.Combine(dir, "PCL", "Logo.png");
                    States.Instance.IsLogoCustom[dir] = true;
                }
                else
                {
                    States.Instance.LogoPath[dir] = tag;
                    States.Instance.IsLogoCustom[dir] = !string.IsNullOrEmpty(tag);
                    try { File.Delete(Path.Combine(dir, "PCL", "Logo.png")); } catch { }
                }
                try { ModBase.WriteIni(ModFolder.mcFolderSelected + "PCL.ini", "InstanceCache", ""); } catch { }
                HintService.Hint("实例图标已更新。", HintType.Success);
            }
            catch (Exception ex)
            {
                HintService.Hint("图标设置失败：" + ex.Message, HintType.Error);
            }
        };
        iconRow.Children.Add(iconCombo);
        spSettings.Children.Add(iconRow);
        // 小teto定制：启动参数自定义
        spSettings.Children.Add(new TextBlock { Text = "启动参数（GDK包生效，如 -windowed -fullscreen）", FontSize = 13, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 12, 0, 4) });
        var argsRow = new Grid { Margin = new Thickness(0, 0, 0, 2) };
        argsRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        argsRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        string curArgs = "";
        try { curArgs = ModOtherGames.GetBedrockLaunchArgs(dir); } catch { }
        var argsBox = new MyTextBox { Height = 30, Text = curArgs, Margin = new Thickness(0, 0, 10, 0), VerticalContentAlignment = VerticalAlignment.Center };
        var btnArgsSave = new MyButton { Text = "保存参数", Height = 30, MinWidth = 90 };
        btnArgsSave.Click += (_, _) =>
        {
            try
            {
                var argsFile = Path.Combine(dir, "launch_args.txt");
                if (string.IsNullOrWhiteSpace(argsBox.Text))
                {
                    if (File.Exists(argsFile)) File.Delete(argsFile);
                    HintService.Hint("已清除启动参数", HintType.Success);
                }
                else
                {
                    File.WriteAllText(argsFile, argsBox.Text.Trim(), System.Text.Encoding.UTF8);
                    HintService.Hint("启动参数已保存", HintType.Success);
                }
            }
            catch (Exception ex) { HintService.Hint("保存失败：" + ex.Message, HintType.Error); }
        };
        argsRow.Children.Add(argsBox);
        Grid.SetColumn(btnArgsSave, 1);
        argsRow.Children.Add(btnArgsSave);
        spSettings.Children.Add(argsRow);
        cardSettings.Children.Add(spSettings);
        PanMain.Children.Add(cardSettings);

        // 个性化
        var card3 = new MyCard { Title = "个性化", UseAnimation = false, Margin = new Thickness(0, 0, 0, 8) };
        var sp3 = new StackPanel { Margin = new Thickness(14, 36, 14, 8) };

        sp3.Children.Add(new TextBlock { Text = "版本描述（不影响游戏）", FontSize = 13, Opacity = 0.7, Margin = new Thickness(0, 6, 0, 2) });
        var descRow = new Grid { Margin = new Thickness(0, 0, 0, 2) };
        descRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        descRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        var descBox = new MyTextBox { Height = 30, Text = ReadBeDesc(dir), Margin = new Thickness(0, 0, 10, 0), VerticalContentAlignment = VerticalAlignment.Center };
        var btnSave = new MyButton { Text = "保存描述", Height = 30, MinWidth = 90 };
        btnSave.Click += (_, _) =>
        {
            WriteBeDesc(dir, descBox.Text);
            HintService.Hint("版本描述已保存。", HintType.Success);
        };
        descRow.Children.Add(descBox);
        Grid.SetColumn(btnSave, 1);
        descRow.Children.Add(btnSave);
        sp3.Children.Add(descRow);
        card3.Children.Add(sp3);
        PanMain.Children.Add(card3);

        // 启动按钮
        var btnLaunch = new MyButton
        {
            Text = "启动该版本",
            Height = 42,
            MinWidth = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            ColorType = MyButton.ColorState.Highlight,
            Margin = new Thickness(0, 6, 0, 0)
        };
        btnLaunch.Click += (_, _) =>
        {
            ModOtherGames.SelectedBedrockVersion = name;
            try { ModBase.WriteIni(ModFolder.mcFolderSelected + "PCL.ini", "Version", name); } catch { }
            var err = ModOtherGames.LaunchBedrockInstance(ModFolder.mcFolderSelected);
            if (string.IsNullOrEmpty(err))
                HintService.Hint("基岩版已启动。", HintType.Success);
            else
                HintService.Hint(err, HintType.Error);
        };
        PanMain.Children.Add(btnLaunch);
    }

    private static Grid MakeInfoRow(string k, string v)
    {
        var g = new Grid { Margin = new Thickness(0, 3, 0, 3) };
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var kt = new TextBlock { Text = k, FontSize = 14, Opacity = 0.7, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 16, 0) };
        var vt = new TextBlock { Text = v, FontSize = 14, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
        g.Children.Add(kt);
        Grid.SetColumn(vt, 1);
        g.Children.Add(vt);
        return g;
    }

    private static Grid MakeActionRow(string label, string hint, Action act)
    {
        var g = new Grid { Margin = new Thickness(0, 3, 0, 3) };
        g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var btn = new MyButton { Text = label, Height = 34, HorizontalAlignment = HorizontalAlignment.Stretch };
        btn.Click += (_, _) => act();
        g.Children.Add(btn);
        if (!string.IsNullOrEmpty(hint))
        {
            var h = new TextBlock { Text = hint, FontSize = 12, Opacity = 0.55, Margin = new Thickness(2, 4, 0, 0), TextWrapping = TextWrapping.Wrap };
            Grid.SetRow(h, 1);
            g.Children.Add(h);
        }
        return g;
    }

    /// <summary>小teto定制：列出 BE 版本 data 目录下指定子文件夹（behavior_packs / resource_packs）的内容并支持打开。</summary>
    private static UIElement MakeBePackPanel(string versionDir, string subFolder, string label)
    {
        var packDir = Path.Combine(versionDir, "data", subFolder);
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 4) };
        if (Directory.Exists(packDir))
        {
            try
            {
                var subs = Directory.GetDirectories(packDir);
                if (subs.Length == 0)
                {
                    panel.Children.Add(new TextBlock { Text = "（暂无" + label + "）", FontSize = 12, Opacity = 0.55 });
                }
                else
                {
                    foreach (var s in subs.Take(8))
                    {
                        var row = new Grid { Margin = new Thickness(0, 1, 0, 1) };
                        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                        var t = new TextBlock { Text = Path.GetFileName(s), FontSize = 13, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
                        var open = new MyButton { Text = "打开", Height = 26, MinWidth = 58, Padding = new Thickness(8, 0, 8, 0) };
                        open.Click += (_, _) => OpenBeFolder(s);
                        row.Children.Add(t);
                        Grid.SetColumn(open, 1);
                        row.Children.Add(open);
                        panel.Children.Add(row);
                    }
                    if (subs.Length > 8)
                        panel.Children.Add(new TextBlock { Text = "……共 " + subs.Length + " 个", FontSize = 12, Opacity = 0.5 });
                }
            }
            catch
            {
                panel.Children.Add(new TextBlock { Text = "（读取失败）", FontSize = 12, Opacity = 0.55 });
            }
        }
        else
        {
            panel.Children.Add(new TextBlock { Text = "（目录不存在）", FontSize = 12, Opacity = 0.55 });
        }
        var btnOpenPack = new MyButton { Text = "打开 " + label + " 文件夹", Height = 28, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 4, 0, 0), Padding = new Thickness(10, 0, 10, 0) };
        btnOpenPack.Click += (_, _) => OpenBeFolder(packDir);
        panel.Children.Add(btnOpenPack);
        return panel;
    }

    private static void OpenBeFolder(string dir)
    {
        try
        {
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                Process.Start(new ProcessStartInfo("explorer.exe", "\"" + dir + "\"") { UseShellExecute = true });
        }
        catch { }
    }

    private static string ReadBeDesc(string versionDir)
    {
        try
        {
            var f = Path.Combine(versionDir, "BE_desc.txt");
            return File.Exists(f) ? File.ReadAllText(f) : "";
        }
        catch { return ""; }
    }

    private static void WriteBeDesc(string versionDir, string text)
    {
        try
        {
            File.WriteAllText(Path.Combine(versionDir, "BE_desc.txt"), text ?? "");
        }
        catch { }
    }

    /// <summary>
    ///     小teto定制：BE 版本列表项（单选，点击选中该版本并返回启动页；右键/按钮进入版本介绍页）。
    /// </summary>
    public static MyListItem BedrockVersionListItem(string versionDir)
    {
        var name = Path.GetFileName(versionDir.TrimEnd('\\'));
        var newItem = new MyListItem
        {
            Title = name,
            Info = "基岩版",
            Height = 42d,
            Tag = versionDir,
            SnapsToDevicePixels = true,
            Type = MyListItem.CheckType.Clickable
        };
        // 小teto定制：BE版本强制使用基岩版图标（正式版用BedrockIconLarge，预览版用BedrockPreviewIcon）
        // 不考虑自定义图标，防止被错误替换为红石块等Java版方块图标
        string beLogo = "pack://application:,,,/images/Custom/BedrockIconLarge.png";
        bool isPreviewVer = false;
        try
        {
            var lname = name.ToLowerInvariant();
            isPreviewVer = lname.Contains("preview") || lname.Contains("beta") || lname.Contains("education");
            if (!isPreviewVer)
            {
                // 26.x 及以上均为预览版（目录名纯数字，不含 preview 字样）
                var m = System.Text.RegularExpressions.Regex.Match(name, @"^(\d+)\.(\d+)\.(\d+)");
                if (m.Success && int.TryParse(m.Groups[1].Value, out var major) && major >= 26)
                    isPreviewVer = true;
            }
            if (isPreviewVer)
            {
                beLogo = "pack://application:,,,/images/Custom/BedrockPreviewIcon.png";
                newItem.Info = "基岩版预览";
            }
            else
            {
                newItem.Info = "基岩版";
            }
        }
        catch { }
        // 小teto定制：优先使用用户手动设置的实例图标（修复修改图标不生效）
        try
        {
            var logoKey = versionDir.TrimEnd('\\') + "\\";
            var customLogo = States.Instance.LogoPath[logoKey];
            var isCustom = States.Instance.IsLogoCustom[logoKey];
            if (!string.IsNullOrEmpty(customLogo) && isCustom)
            {
                if (customLogo.StartsWith("pack://"))
                {
                    beLogo = customLogo;
                }
                else if (customLogo.Replace('/', '\\') == @"PCL\Logo.png")
                {
                    var logoFile = Path.Combine(versionDir, "PCL", "Logo.png");
                    if (File.Exists(logoFile))
                        beLogo = logoFile;
                }
                else if (File.Exists(customLogo))
                {
                    beLogo = customLogo;
                }
            }
        }
        catch { }
        // 最后回退：版本目录存在 PCL\Logo.png 自定义文件也使用
        try
        {
            var logoFile = Path.Combine(versionDir, "PCL", "Logo.png");
            if (File.Exists(logoFile))
                beLogo = logoFile;
        }
        catch { }
        newItem.Logo = beLogo;
        // 小teto定制：GDK / UWP 小标签（类似 Java 内核版本标签，显示在版本号下方）
        // 用 UWPtres.txt 标记文件判断（最可靠），有标记为 UWP，无标记为 GDK
        string beKind = ModOtherGames.IsUwpFolder(versionDir) ? "UWP" : "GDK";
        newItem.Tags = new List<string> { beKind };
        newItem.Checked = string.Equals(ModOtherGames.SelectedBedrockVersion, name, StringComparison.OrdinalIgnoreCase);
        newItem.ContentHandler = (sender, _) =>
        {
            // 点击：选中该 BE 版本并返回启动页（启动按钮会显示该版本）
            sender.Click += (_, _) =>
            {
                var dir = (string)sender.Tag;
                ModOtherGames.SelectedBedrockVersion = Path.GetFileName(dir);
                try
                {
                    States.Game.SelectedInstance = ModOtherGames.SelectedBedrockVersion;
                    // 小teto定制：写入独立的 BedrockVersion 键（不与 Java 版 Version 键冲突，确保重启后恢复）
                    ModBase.WriteIni(ModFolder.mcFolderSelected + "PCL.ini", "BedrockVersion",
                        ModOtherGames.SelectedBedrockVersion);
                    ModBase.WriteIni(ModFolder.mcFolderSelected + "PCL.ini", "Version",
                        ModOtherGames.SelectedBedrockVersion);
                }
                catch
                {
                    // 忽略写入失败
                }

                ModMain.frmMain.PageBack();
            };
            // 按钮：打开版本文件夹 + 实例设置（进入 BE 版本介绍页）
            var btnOpen = new MyIconButton { LogoScale = 1.1d, SvgIcon = "lucide/folder-open" };
            btnOpen.ToolTip = Lang.Text("Common.Action.OpenFolder");
            ToolTipService.SetPlacement(btnOpen, PlacementMode.Center);
            ToolTipService.SetVerticalOffset(btnOpen, 30d);
            ToolTipService.SetHorizontalOffset(btnOpen, 2d);
            btnOpen.Click += (_, _) => OpenBeFolder((string)sender.Tag);
            var btnCont = new MyIconButton { LogoScale = 1.1d, SvgIcon = "lucide/settings" };
            btnCont.ToolTip = "实例设置";
            ToolTipService.SetPlacement(btnCont, PlacementMode.Center);
            ToolTipService.SetVerticalOffset(btnCont, 30d);
            ToolTipService.SetHorizontalOffset(btnCont, 2d);
            btnCont.Click += (_, _) => ModMain.frmSelectRight?.ShowBeVersionInfo((string)sender.Tag);
            sender.Buttons = new[] { btnOpen, btnCont };
            sender.MouseRightButtonUp += (_, _) => ModMain.frmSelectRight?.ShowBeVersionInfo((string)sender.Tag);
        };
        return newItem;
    }

    public static MyListItem McVersionListItem(McInstance mcInstance)
    {
        var newItem = new MyListItem
        {
            Title = mcInstance.Name, Info = mcInstance.Desc, Height = 42d, Tag = mcInstance, SnapsToDevicePixels = true,
            Type = MyListItem.CheckType.Clickable
        };
        var instanceInfo = mcInstance.Info;
        var tags = new List<string>();
        tags.Add(instanceInfo.VanillaName);
        if (instanceInfo.HasForge)
            tags.Add("Forge " + instanceInfo.Forge);
        else if (instanceInfo.HasNeoForge)
            tags.Add("NeoForge " + instanceInfo.NeoForge);
        else if (instanceInfo.HasCleanroom)
            tags.Add("Cleanroom " + instanceInfo.Cleanroom);
        else if (instanceInfo.HasLabyMod)
            tags.Add("LabyMod " + instanceInfo.LabyMod);
        else if (instanceInfo.HasQuilt)
            tags.Add("Quilt " + instanceInfo.Quilt);
        else if (instanceInfo.HasFabric) tags.Add("Fabric " + instanceInfo.Fabric);
        if (instanceInfo.HasLiteLoader)
            tags.Add("LiteLoader");
        if (instanceInfo.HasOptiFine)
            tags.Add("OptiFine " + instanceInfo.OptiFine);
        newItem.Tags = tags;
        try
        {
            if (mcInstance.Logo.EndsWith(@"PCL\Logo.png"))
                newItem.Logo = mcInstance.PathInstance + @"PCL\Logo.png"; // 修复老版本中，存储的自定义 Logo 使用完整路径，导致移动后无法加载的 Bug
            else
                newItem.Logo = mcInstance.Logo;
        }
        catch (Exception ex)
        {
            ModBase.Log(
                ex,
                Lang.Text("Select.Instance.Error.IconLoad"),
                ModBase.LogLevel.Hint,
                userSummary: Lang.Text("Select.Instance.Error.IconLoad"));
            newItem.Logo = "pack://application:,,,/images/Blocks/RedstoneBlock.png";
        }

        newItem.ContentHandler = McVersionListContent;
        return newItem;
    }

    private static void McVersionListContent(MyListItem sender, EventArgs e)
    {
        var version = (McInstance)sender.Tag;
        // 注册点击事件
        sender.Click += (a, b) => Item_Click((MyListItem)a, b);
        // 图标按钮
        var btnStar = new MyIconButton();
        if (version.IsStar)
        {
            btnStar.ToolTip = Lang.Text("Select.Instance.Unfavorite");
            ToolTipService.SetPlacement(btnStar, PlacementMode.Center);
            ToolTipService.SetVerticalOffset(btnStar, 30d);
            ToolTipService.SetHorizontalOffset(btnStar, 2d);
            btnStar.LogoScale = 1.1d;
            btnStar.SvgIcon = "lucide/heart-filled";
        }
        else
        {
            btnStar.ToolTip = Lang.Text("Select.Instance.Favorite");
            ToolTipService.SetPlacement(btnStar, PlacementMode.Center);
            ToolTipService.SetVerticalOffset(btnStar, 30d);
            ToolTipService.SetHorizontalOffset(btnStar, 2d);
            btnStar.LogoScale = 1.1d;
            btnStar.SvgIcon = "lucide/heart";
        }

        btnStar.Click += (_, _) =>
        {
            States.Instance.Starred[version.PathInstance] = !version.IsStar;
            ModInstanceList.mcInstanceListForceRefresh = true;
            ModLoader.LoaderFolderRun(ModInstanceList.mcInstanceListLoader, ModFolder.mcFolderSelected,
                ModLoader.LoaderFolderRunType.ForceRun, 1, @"versions\");
        };
        var btnOpenFolder = new MyIconButton { LogoScale = 1.1d, SvgIcon = "lucide/folder-open" };
        btnOpenFolder.ToolTip = Lang.Text("Select.Instance.OpenFolder");
        ToolTipService.SetPlacement(btnOpenFolder, PlacementMode.Center);
        ToolTipService.SetVerticalOffset(btnOpenFolder, 30d);
        ToolTipService.SetHorizontalOffset(btnOpenFolder, 2d);
        btnOpenFolder.Click += (_, _) => PageInstanceOverall.OpenVersionFolder(version);
        var btnDel = new MyIconButton { LogoScale = 1.1d, SvgIcon = "lucide/trash-2" };
        btnDel.ToolTip = Lang.Text("Common.Action.Delete");
        ToolTipService.SetPlacement(btnDel, PlacementMode.Center);
        ToolTipService.SetVerticalOffset(btnDel, 30d);
        ToolTipService.SetHorizontalOffset(btnDel, 2d);
        btnDel.Click += (_, _) => DeleteVersion(sender, version);
        if (version.state != McInstanceState.Error)
        {
            var btnCont = new MyIconButton { LogoScale = 1.1d, SvgIcon = "lucide/settings" };
            btnCont.ToolTip = Lang.Text("Select.Instance.Settings");
            ToolTipService.SetPlacement(btnCont, PlacementMode.Center);
            ToolTipService.SetVerticalOffset(btnCont, 30d);
            ToolTipService.SetHorizontalOffset(btnCont, 2d);
            btnCont.Click += (_, _) =>
            {
                PageInstanceLeft.McInstance = version;
                ModMain.frmMain.PageChange(FormMain.PageType.InstanceSetup);
            };
            sender.MouseRightButtonUp += (_, _) =>
            {
                PageInstanceLeft.McInstance = version;
                ModMain.frmMain.PageChange(FormMain.PageType.InstanceSetup);
            };
            sender.Buttons = new[] { btnStar, btnOpenFolder, btnDel, btnCont };
        }
        else
        {
            var btnCont = new MyIconButton { LogoScale = 1.15d, SvgIcon = "lucide/folder-open" };
            btnCont.ToolTip = Lang.Text("Common.Action.OpenFolder");
            ToolTipService.SetPlacement(btnCont, PlacementMode.Center);
            ToolTipService.SetVerticalOffset(btnCont, 30d);
            ToolTipService.SetHorizontalOffset(btnCont, 2d);
            btnCont.Click += (_, _) => PageInstanceOverall.OpenVersionFolder(version);
            sender.MouseRightButtonUp += (_, _) => PageInstanceOverall.OpenVersionFolder(version);
            sender.Buttons = new[] { btnStar, btnOpenFolder, btnDel, btnCont };
        }
    }

    #endregion

    #region 页面事件

    // 点击选项
    public static void Item_Click(MyListItem sender, EventArgs e)
    {
        var instance = (McInstance)sender.Tag;
        if (new McInstance(instance.PathInstance).Check())
        {
            // 正常实例
            ModInstanceList.McMcInstanceSelected = instance;
            States.Game.SelectedInstance = ModInstanceList.McMcInstanceSelected.Name;
            ModMain.frmMain.PageBack();
        }
        else
        {
            // 错误实例
            PageInstanceOverall.OpenVersionFolder(instance);
        }
    }

    private void BtnDownload_Click(object sender, MouseButtonEventArgs e)
    {
        ModMain.frmMain.PageChange(FormMain.PageType.Download, FormMain.PageSubType.DownloadInstall);
    }

    // 修改此代码时，同时修改 PageInstanceOverall 中的代码
    public static void DeleteVersion(MyListItem item, McInstance mcInstance)
    {
        try
        {
            var isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            var isHintIndie = mcInstance.state != McInstanceState.Error &&
                              (mcInstance.PathIndie ?? "") != (ModFolder.mcFolderSelected ?? "");
            var confirmMsg = isShiftPressed
                ? Lang.Text("Select.Instance.Delete.ConfirmPermanentMessage", mcInstance.Name)
                : Lang.Text("Select.Instance.Delete.ConfirmMessage", mcInstance.Name);
            var confirmFullMsg = confirmMsg +
                                 (isHintIndie ? "\r\n" + Lang.Text("Select.Instance.Delete.IsolatedWarning") : "");
            switch (ModMain.MyMsgBox(confirmFullMsg, Lang.Text("Select.Instance.Delete.ConfirmTitle"),
                        button2: Lang.Text("Common.Action.Cancel"), isWarn: true))
            {
                case 1:
                {
                    ModBase.IniClearCache(Path.Combine(mcInstance.PathIndie, "options.txt"));
                    ((DynamicCacheConfigStorage)ConfigService.GetProvider(ConfigSource.GameInstance)).InvalidateCache(
                        mcInstance.PathInstance);
                    if (isShiftPressed)
                    {
                        ModBase.DeleteDirectory(mcInstance.PathInstance);
                        HintService.Hint(Lang.Text("Select.Instance.Delete.PermanentSuccess", mcInstance.Name),
                            HintType.Success);
                    }
                    else
                    {
                        FileSystem.DeleteDirectory(mcInstance.PathInstance, UIOption.AllDialogs,
                            RecycleOption.SendToRecycleBin);
                        HintService.Hint(Lang.Text("Select.Instance.Delete.RecycleBinSuccess", mcInstance.Name),
                            HintType.Success);
                    }

                    break;
                }
                case 2:
                {
                    return;
                }
            }

            // 从 UI 中移除
            if (mcInstance.displayType == McInstanceCardType.Hidden || !mcInstance.IsStar)
            {
                // 仅出现在当前卡片
                var parent = (StackPanel)item.Parent;
                if (parent.Children.Count > 2) // 当前的项目与一个占位符
                {
                    // 删除后还有剩
                    var card = (MyCard)parent.Parent;
                    card.Title = card.Title.Replace(Lang.Number(parent.Children.Count - 1, "N0"),
                        Lang.Number(parent.Children.Count - 2, "N0")); // 有一个占位符
                    parent.Children.Remove(item);
                    if (ModInstanceList.McMcInstanceSelected is not null && (mcInstance.PathInstance ?? "") ==
                        (ModInstanceList.McMcInstanceSelected.PathInstance ?? ""))
                        // 删除当前实例就更改选择
                        ModInstanceList.McMcInstanceSelected = (McInstance)((MyListItem)parent.Children[0]).Tag;
                    ModLoader.LoaderFolderRun(ModInstanceList.mcInstanceListLoader, ModFolder.mcFolderSelected,
                        ModLoader.LoaderFolderRunType.UpdateOnly, 1, @"versions\");
                }
                else
                {
                    // 删除后没剩了
                    ModLoader.LoaderFolderRun(ModInstanceList.mcInstanceListLoader, ModFolder.mcFolderSelected,
                        ModLoader.LoaderFolderRunType.ForceRun, 1, @"versions\");
                }
            }
            else
            {
                // 同时出现在当前卡片与收藏夹
                ModLoader.LoaderFolderRun(ModInstanceList.mcInstanceListLoader, ModFolder.mcFolderSelected,
                    ModLoader.LoaderFolderRunType.ForceRun, 1, @"versions\");
            }
        }
        catch (OperationCanceledException ex)
        {
            ModBase.Log(ex, $"删除实例 {mcInstance.Name} 被主动取消");
        }
        catch (Exception ex)
        {
            ModBase.Log(
                ex,
                Lang.Text("Select.Instance.Error.Delete", mcInstance.Name),
                ModBase.LogLevel.Msgbox,
                userSummary: Lang.Text("Select.Instance.Error.Delete", mcInstance.Name));
        }
    }

    public void BtnEmptyDownload_Loaded()
    {
        var newVisibility = (Config.Preference.Hide.PageDownload && !PageSetupUI.HiddenForceShow) || showHidden
            ? Visibility.Collapsed
            : Visibility.Visible;
        if (BtnEmptyDownload.Visibility != newVisibility)
        {
            BtnEmptyDownload.Visibility = newVisibility;
            PanLoad.TriggerForceResize();
        }
    }

    #endregion
}
