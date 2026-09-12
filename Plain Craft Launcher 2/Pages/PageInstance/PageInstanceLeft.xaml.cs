using System.IO;
using System.Windows;
using System.Windows.Controls;
using PCL.Core.App;
using PCL.Core.App.Localization;

namespace PCL;

public partial class PageInstanceLeft : IRefreshable
{
    /// <summary>
    ///     当前显示设置的 MC 实例。
    /// </summary>
    public static McInstance McInstance = null;

    /// <summary>
    ///     当前 PageInstanceLeft 实例（用于静态方法调用实例方法）
    /// </summary>
    public static PageInstanceLeft CurrentInstance = null;

    public PageInstanceLeft()
    {
        InitializeComponent();
        CurrentInstance = this;
        Loaded += (_, _) => RefreshModDisabled();
    }

    public void Refresh()
    {
        Refresh(ModMain.frmMain.PageCurrentSub);
    }

    public void RefreshModDisabled()
    {
        var hide = Config.Preference.Hide;

        // 小teto定制：BE 实例导航项 BE 化（不删除，而是修改标题和行为）
        // 使用更简单可靠的检测方式：路径包含 bedrock_versions，或目录内含 Minecraft.Windows.exe / AppxManifest.xml
        bool isBeInstance = false;
        if (McInstance != null)
        {
            string instPath = McInstance.PathInstance.TrimEnd('\\');
            isBeInstance = instPath.Contains("bedrock_versions") ||
                           File.Exists(Path.Combine(instPath, "Minecraft.Windows.exe")) ||
                           File.Exists(Path.Combine(instPath, "AppxManifest.xml")) ||
                           ModOtherGames.IsBedrockFolder(McInstance.PathInstance);
        }
        if (isBeInstance)
        {
            // 隐藏 Java 专属导航项
            ItemInstall.Visibility = Visibility.Collapsed; // 修改/安装
            ItemExport.Visibility = Visibility.Collapsed; // 导出
            ItemSetup.Visibility = Visibility.Collapsed; // 设置（BE不需要）
            // 恢复 ItemMod（行为包），隐藏 ItemModDisabled（模组）
            ItemMod.Visibility = Visibility.Visible;
            ItemMod.Title = "行为包"; // 模组 -> 行为包 (.mcaddon)
            ItemModDisabled.Visibility = Visibility.Collapsed; // 禁用的模组（删除）
            ItemShader.Visibility = Visibility.Collapsed; // 光影包
            ItemSchematic.Visibility = Visibility.Collapsed; // 投影原理图
            ItemServer.Visibility = Visibility.Collapsed; // 服务器
            // BE 化导航项标题
            ItemResourcePack.Title = "资源包"; // 资源包 (.mcpack)
            ItemWorld.Title = "存档"; // 存档 (/saves)
            ItemScreenshot.Title = "截图"; // 截图
            // 确保 BE 专属导航项可见
            ItemResourcePack.Visibility = Visibility.Visible;
            ItemWorld.Visibility = Visibility.Visible;
            ItemScreenshot.Visibility = Visibility.Visible;
            ItemSkinPack.Visibility = Visibility.Visible; // 皮肤包
            TextResource.Visibility = Visibility.Visible;
            // 关键：直接返回，防止后面的 Java 版逻辑覆盖 BE 设置
            return;
        }

        if (McInstance is not null && McInstance.Modable)
        {
            ItemMod.Visibility = !PageSetupUI.HiddenForceShow && hide.InstanceMod
                ? Visibility.Collapsed
                : Visibility.Visible;
            ItemModDisabled.Visibility = Visibility.Collapsed;
        }
        else
        {
            ItemMod.Visibility = Visibility.Collapsed;
            ItemModDisabled.Visibility = !PageSetupUI.HiddenForceShow && hide.InstanceMod
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        // 功能隐藏
        if (!PageSetupUI.HiddenForceShow)
        {
            var disableCount = 0;
            if (hide.InstanceSave)
                disableCount += 1;
            if (hide.InstanceScreenshot)
                disableCount += 1;
            if (hide.InstanceMod)
                disableCount += 1;
            if (hide.InstanceResourcePack)
                disableCount += 1;
            if (hide.InstanceShader)
                disableCount += 1;
            if (hide.InstanceSchematic)
                disableCount += 1;
            if (hide.InstanceServer)
                disableCount += 1;
            if (disableCount == 7)
                TextResource.Visibility = Visibility.Collapsed;
            else
                TextResource.Visibility = Visibility.Visible;
        }
        else
        {
            TextResource.Visibility = Visibility.Visible;
        }

        ItemInstall.Visibility = !PageSetupUI.HiddenForceShow && hide.InstanceEdit
            ? Visibility.Collapsed
            : Visibility.Visible;
        ItemExport.Visibility = !PageSetupUI.HiddenForceShow && hide.InstanceExport
            ? Visibility.Collapsed
            : Visibility.Visible;
        ItemWorld.Visibility = !PageSetupUI.HiddenForceShow && hide.InstanceSave
            ? Visibility.Collapsed
            : Visibility.Visible;
        ItemScreenshot.Visibility = !PageSetupUI.HiddenForceShow && hide.InstanceScreenshot
            ? Visibility.Collapsed
            : Visibility.Visible;
        ItemResourcePack.Visibility = !PageSetupUI.HiddenForceShow && hide.InstanceResourcePack
            ? Visibility.Collapsed
            : Visibility.Visible;
        ItemShader.Visibility = !PageSetupUI.HiddenForceShow && hide.InstanceShader
            ? Visibility.Collapsed
            : Visibility.Visible;
        ItemSchematic.Visibility = !PageSetupUI.HiddenForceShow && hide.InstanceSchematic
            ? Visibility.Collapsed
            : Visibility.Visible;
        ItemServer.Visibility = !PageSetupUI.HiddenForceShow && hide.InstanceServer
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void RefreshButton_Click(object sender, EventArgs e) // 由边栏按钮匿名调用
    {
        Refresh((FormMain.PageSubType)ModBase.Val(((MyIconButton)sender).Tag));
    }

    public void Refresh(FormMain.PageSubType subType)
    {
        switch (subType)
        {
            case FormMain.PageSubType.VersionMod:
            {
                PageInstanceCompResource.Refresh(ModComp.CompType.Mod);
                break;
            }
            case FormMain.PageSubType.VersionScreenshot:
            {
                var ignore= PageInstanceScreenshot.RefreshAsync();
                break;
            }
            case FormMain.PageSubType.VersionWorld:
            {
                PageInstanceSaves.Refresh();
                break;
            }
            case FormMain.PageSubType.VersionResourcePack:
            {
                PageInstanceCompResource.Refresh(ModComp.CompType.ResourcePack);
                break;
            }
            case FormMain.PageSubType.VersionShader:
            {
                PageInstanceCompResource.Refresh(ModComp.CompType.Shader);
                break;
            }
            case FormMain.PageSubType.VersionSchematic:
            {
                PageInstanceCompResource.Refresh(ModComp.CompType.Schematic);
                break;
            }
            case FormMain.PageSubType.VersionInstall:
            {
                ModDownload.dlClientListLoader.Start(isForceRestart: true);
                ModDownload.dlOptiFineListLoader.Start(isForceRestart: true);
                ModDownload.dlForgeListLoader.Start(isForceRestart: true);
                ModDownload.dlNeoForgeListLoader.Start(isForceRestart: true);
                ModDownload.dlLiteLoaderListLoader.Start(isForceRestart: true);
                ModDownload.dlFabricListLoader.Start(isForceRestart: true);
                ModDownload.dlFabricApiLoader.Start(isForceRestart: true);
                ModDownload.dlOptiFabricLoader.Start(isForceRestart: true);
                ModDownload.dlLabyModListLoader.Start(isForceRestart: true);
                ItemInstall.Checked = true;
                ModMain.frmInstanceInstall.GetCurrentInfo();
                break;
            }
            case FormMain.PageSubType.VersionExport:
            {
                if (ModMain.frmInstanceExport is not null)
                    ModMain.frmInstanceExport.RefreshAll();
                ItemExport.Checked = true;
                break;
            }
            case FormMain.PageSubType.VersionServer:
            {
                if (ModMain.frmInstanceServer is not null)
                    ModMain.frmInstanceServer.RefreshServers();
                ItemServer.Checked = true;
                break;
            }
        }
    }

    public void Reset(object sender, EventArgs e)
    {
        if (ModMain.MyMsgBox(Lang.Text("Instance.Left.InitializeSettings.ConfirmMessage"),
                Lang.Text("Instance.Left.InitializeSettings.ConfirmTitle"),
                button2: Lang.Text("Common.Action.Cancel"),
                isWarn: true)
            == 1)
        {
            if (ModMain.frmInstanceSetup is null)
                ModMain.frmInstanceSetup = new PageInstanceSetup();
            ModMain.frmInstanceSetup.Reset();
            ItemSetup.Checked = true;
        }
    }

    #region 页面切换

    /// <summary>
    ///     当前页面的编号。从 0 开始计算。
    /// </summary>
    public FormMain.PageSubType pageID = FormMain.PageSubType.Default;

    /// <summary>
    ///     勾选事件改变页面。
    /// </summary>
    private void PageCheck(object sender, ModBase.RouteEventArgs e)
    {
        if (sender is MyListItem item && item.Tag is not null)
        {
            int tag = (int)ModBase.Val(item.Tag);
            // 小teto定制：皮肤包（Tag=20）进入资源管理页显示其中内容（皮肤包模式）
            if (tag == 20 && McInstance != null && ModOtherGames.IsBedrockFolder(McInstance.PathInstance))
            {
                PageInstanceCompResource.IsSkinPackMode = true;
                PageChange(FormMain.PageSubType.VersionResourcePack);
                return;
            }
            // 小teto定制：非皮肤包导航时退出皮肤包模式（皮肤包与资源包共用页面，防止标志残留导致路径错乱）
            PageInstanceCompResource.IsSkinPackMode = false;
            PageChange((FormMain.PageSubType)tag);
        }
    }

    public object PageGet(FormMain.PageSubType id)
    {
        if ((int)id == -1)
            id = pageID;
        switch (id)
        {
            case FormMain.PageSubType.VersionOverall:
            {
                if (ModMain.frmInstanceOverall is null)
                    ModMain.frmInstanceOverall = new PageInstanceOverall();
                return ModMain.frmInstanceOverall;
            }
            case FormMain.PageSubType.VersionMod:
            {
                if (ModMain.frmInstanceMod is null)
                    ModMain.frmInstanceMod = new PageInstanceCompResource(ModComp.CompType.Mod);
                return ModMain.frmInstanceMod;
            }
            case FormMain.PageSubType.VersionModDisabled:
            {
                if (ModMain.frmInstanceModDisabled is null)
                    ModMain.frmInstanceModDisabled = new PageInstanceModDisabled();
                return ModMain.frmInstanceModDisabled;
            }
            case FormMain.PageSubType.VersionSetup:
            {
                if (ModMain.frmInstanceSetup is null)
                    ModMain.frmInstanceSetup = new PageInstanceSetup();
                return ModMain.frmInstanceSetup;
            }
            case FormMain.PageSubType.VersionWorld:
            {
                if (ModMain.frmInstanceSaves is null)
                    ModMain.frmInstanceSaves = new PageInstanceSaves();
                return ModMain.frmInstanceSaves;
            }
            case FormMain.PageSubType.VersionScreenshot:
            {
                if (ModMain.frmInstanceScreenshot is null)
                    ModMain.frmInstanceScreenshot = new PageInstanceScreenshot();
                return ModMain.frmInstanceScreenshot;
            }
            case FormMain.PageSubType.VersionResourcePack:
            {
                if (ModMain.frmInstanceResourcePack is null)
                    ModMain.frmInstanceResourcePack = new PageInstanceCompResource(ModComp.CompType.ResourcePack);
                return ModMain.frmInstanceResourcePack;
            }
            case FormMain.PageSubType.VersionShader:
            {
                if (ModMain.frmInstanceShader is null)
                    ModMain.frmInstanceShader = new PageInstanceCompResource(ModComp.CompType.Shader);
                return ModMain.frmInstanceShader;
            }
            case FormMain.PageSubType.VersionSchematic:
            {
                if (ModMain.frmInstanceSchematic is null)
                    ModMain.frmInstanceSchematic = new PageInstanceCompResource(ModComp.CompType.Schematic);
                return ModMain.frmInstanceSchematic;
            }
            case FormMain.PageSubType.VersionInstall:
            {
                if (ModMain.frmInstanceInstall is null)
                    ModMain.frmInstanceInstall = new PageInstanceInstall();
                return ModMain.frmInstanceInstall;
            }
            case FormMain.PageSubType.VersionExport:
            {
                if (ModMain.frmInstanceExport is null)
                    ModMain.frmInstanceExport = new PageInstanceExport();
                return ModMain.frmInstanceExport;
            }
            case FormMain.PageSubType.VersionServer:
            {
                if (ModMain.frmInstanceServer is null)
                    ModMain.frmInstanceServer = new PageInstanceServer();
                return ModMain.frmInstanceServer;
            }

            default:
            {
                throw new Exception("未知的实例设置子页面种类：" + (int)id);
            }
        }
    }

    /// <summary>
    ///     切换现有页面。
    /// </summary>
    public void PageChange(FormMain.PageSubType id)
    {
        if (pageID == id)
            return;
        ModAnimation.AniControlEnabled += 1;
        try
        {
            PageChangeRun((MyPageRight)PageGet(id));
            pageID = id;
        }
        catch (Exception ex)
        {
            ModBase.Log(
                ex,
                "切换分页面失败（ID " + (int)id + "）",
                ModBase.LogLevel.Feedback,
                userSummary: Lang.Text("Instance.Error.OperationFailed"));
        }
        finally
        {
            ModAnimation.AniControlEnabled -= 1;
        }
    }

    private static void PageChangeRun(MyPageRight target)
    {
        ModAnimation.AniStop("FrmMain PageChangeRight"); // 停止主页面的右页面切换动画，防止它与本动画一起触发多次 PageOnEnter
        if (target.Parent is not null)
            target.SetValue(ContentPresenter.ContentProperty, null);
        ModMain.frmMain.pageRight = target;
        ((MyPageRight)ModMain.frmMain.PanMainRight.Child).PageOnExit();
        ModAnimation.AniStart(new[]
        {
            ModAnimation.AaCode(() =>
            {
                ((MyPageRight)ModMain.frmMain.PanMainRight.Child).PageOnForceExit();
                ModMain.frmMain.PanMainRight.Child = ModMain.frmMain.pageRight;
                ModMain.frmMain.pageRight.Opacity = 0d;
            }, 130),
            ModAnimation.AaCode(() =>
            {
                // 延迟触发页面通用动画，以使得在 Loaded 事件中加载的控件得以处理
                ModMain.frmMain.pageRight.Opacity = 1d;
                ModMain.frmMain.pageRight.PageOnEnter();
            }, 30, true)
        }, "PageLeft PageChange");
    }

    #endregion
}
