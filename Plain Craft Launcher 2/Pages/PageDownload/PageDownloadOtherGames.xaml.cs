using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PCL.Core.App;

namespace PCL;

public partial class PageDownloadOtherGames
{
    private bool isLoad;

    public PageDownloadOtherGames()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (isLoad) return;
        isLoad = true;
        RefreshStatus();
    }

    /// <summary>
    ///     刷新地下城 / 传奇的安装状态。
    /// </summary>
    public void RefreshStatus()
    {
        try
        {
            // 地下城
            var dungeons = ModOtherGames.FindGamePackage(ModOtherGames.DungeonsFamilyPrefixes);
            if (dungeons.Count == 0)
            {
                TextDungeonsStatus.Text = "未检测到已安装的 Minecraft Dungeons。";
                BtnDungeonsLaunch.IsEnabled = false;
            }
            else
            {
                var d = dungeons.First();
                TextDungeonsStatus.Text = "已安装  " + (d.IsOk ? "授权有效" : "授权状态异常") + "   版本 " + d.Version;
                BtnDungeonsLaunch.IsEnabled = d.IsOk;
            }

            // 传奇
            var legends = ModOtherGames.FindGamePackage(ModOtherGames.LegendsFamilyPrefixes);
            if (legends.Count == 0)
            {
                TextLegendsStatus.Text = "未检测到已安装的 Minecraft Legends。";
                BtnLegendsLaunch.IsEnabled = false;
            }
            else
            {
                var l = legends.First();
                TextLegendsStatus.Text = "已安装  " + (l.IsOk ? "授权有效" : "授权状态异常") + "   版本 " + l.Version;
                BtnLegendsLaunch.IsEnabled = l.IsOk;
            }

            // 教育版
            var education = ModOtherGames.FindGamePackage(ModOtherGames.EducationFamilyPrefixes);
            if (education.Count == 0)
            {
                TextEducationStatus.Text = "未检测到已安装的 Minecraft Education。";
                BtnEducationLaunch.IsEnabled = false;
            }
            else
            {
                var ed = education.First();
                TextEducationStatus.Text = "已安装  " + (ed.IsOk ? "授权有效" : "授权状态异常") + "   版本 " + ed.Version;
                BtnEducationLaunch.IsEnabled = ed.IsOk;
            }
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "刷新游戏状态失败");
        }
    }

    private async void BtnDungeonsLaunch_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var list = ModOtherGames.FindGamePackage(ModOtherGames.DungeonsFamilyPrefixes);
        if (list.Count == 0)
        {
            HintInfo.Text = "尚未安装 Minecraft Dungeons，请先在商店安装。";
            return;
        }

        var ok = ModOtherGames.LaunchPackage(list.First());
        HintInfo.Text = ok ? "已启动 Minecraft Dungeons。" : "启动失败，请检查授权状态。";
    }

    private async void BtnLegendsLaunch_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var check = ModOtherGames.CheckLegends();
        TextLegendsCheck.Text = check.Message;
        if (check.Result != ModOtherGames.LegendsCheckResult.Owned)
        {
            HintInfo.Text = "传奇正版校验未通过，未启动。";
            return;
        }

        var list = ModOtherGames.FindGamePackage(ModOtherGames.LegendsFamilyPrefixes);
        if (list.Count == 0)
        {
            HintInfo.Text = "尚未安装 Minecraft Legends。";
            return;
        }

        var ok = ModOtherGames.LaunchPackage(list.First());
        HintInfo.Text = ok ? "正版校验通过，已启动 Minecraft Legends。" : "启动失败。";
    }

    private async void BtnLegendsCheck_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var check = ModOtherGames.CheckLegends();
        TextLegendsCheck.Text = check.Message;
        HintInfo.Text = "正版校验完成：" + (check.Result == ModOtherGames.LegendsCheckResult.Owned ? "通过" : "未通过");
    }

    private async void BtnDungeonsStore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ModOtherGames.OpenStore(ModOtherGames.ProductIdDungeons);
    }

    private async void BtnLegendsStore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ModOtherGames.OpenStore(ModOtherGames.ProductIdLegends);
    }

    private async void BtnDungeonsUpdate_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        BtnDungeonsUpdate.IsEnabled = false;
        TextDungeonsUpdate.Text = "正在检查最新版本……";
        var latest = await ModOtherGames.QueryStoreLatestVersionAsync(ModOtherGames.ProductIdDungeons);
        var installed = ModOtherGames.FindGamePackage(ModOtherGames.DungeonsFamilyPrefixes);
        var instVer = installed.Count > 0 ? installed.First().Version : "";
        TextDungeonsUpdate.Text = string.IsNullOrEmpty(latest)
            ? (string.IsNullOrEmpty(instVer) ? "未能获取在线版本信息。" : "已安装版本 " + instVer + "（在线查询暂不可用）。")
            : "最新版本 " + latest + (string.IsNullOrEmpty(instVer) ? "（本机未安装）" : "；本机 " + instVer + (IsNewer(latest, instVer) ? "，可更新" : "，已是最新"));
        BtnDungeonsUpdate.IsEnabled = true;
    }

    private async void BtnLegendsUpdate_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        BtnLegendsUpdate.IsEnabled = false;
        TextLegendsUpdate.Text = "正在检查最新版本……";
        var latest = await ModOtherGames.QueryStoreLatestVersionAsync(ModOtherGames.ProductIdLegends);
        var installed = ModOtherGames.FindGamePackage(ModOtherGames.LegendsFamilyPrefixes);
        var instVer = installed.Count > 0 ? installed.First().Version : "";
        TextLegendsUpdate.Text = string.IsNullOrEmpty(latest)
            ? (string.IsNullOrEmpty(instVer) ? "未能获取在线版本信息。" : "已安装版本 " + instVer + "（在线查询暂不可用）。")
            : "最新版本 " + latest + (string.IsNullOrEmpty(instVer) ? "（本机未安装）" : "；本机 " + instVer + (IsNewer(latest, instVer) ? "，可更新" : "，已是最新"));
        BtnLegendsUpdate.IsEnabled = true;
    }

    /// <summary>
    ///     判断远程版本是否比本地版本新（best-effort 版本号比较）。
    /// </summary>
    private static bool IsNewer(string remote, string local)
    {
        try
        {
            var r = System.Version.Parse(remote);
            var l = System.Version.Parse(local.Split(' ')[0]);
            return r > l;
        }
        catch
        {
            return false;
        }
    }

    // ---------- 直接下载（正版验证 + 商店获取 + 下载 + 系统安装） ----------

    private void SetDlUi(string text, bool busy, double percent, string detail)
    {
        TextDlTask.Text = text;
        TextDlDetail.Text = detail ?? "";
        BarDlTask.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        if (busy) BarDlTask.Value = percent;
    }

    private async void BtnDungeonsDownload_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => await DownloadStoreGameAsync("Minecraft Dungeons", ModOtherGames.ProductIdDungeons,
            ModOtherGames.DungeonsFamilyPrefixes, BtnDungeonsDownload);

    private async void BtnLegendsDownload_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => await DownloadStoreGameAsync("Minecraft Legends", ModOtherGames.ProductIdLegends,
            ModOtherGames.LegendsFamilyPrefixes, BtnLegendsDownload);

    private async void BtnEducationDownload_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => await DownloadStoreGameAsync("Minecraft Education", ModOtherGames.ProductIdEducation,
            ModOtherGames.EducationFamilyPrefixes, BtnEducationDownload);

    private async Task DownloadStoreGameAsync(string gameName, string productId, string[] familyPrefixes, MyButton btn)
    {
        // 正版验证门槛：必须登录微软账号（安装与启动由系统按账号授权校验）
        if (!ModOtherGames.IsMicrosoftLoggedIn())
        {
            HintInfo.Text = gameName + " 正版验证未通过：未登录微软账号。请先在启动器登录微软账号后重试。";
            return;
        }

        try { btn.IsEnabled = false; } catch { }

        try
        {
            SetDlUi("正在为 " + gameName + " 获取下载地址……", true, 0, "查询微软商店公开接口 store.rg-adguard.net");
            var urls = await ModOtherGames.GetStorePackageUrlsAsync(productId);
            if (urls.Count == 0)
            {
                SetDlUi(gameName + "：未能获取下载地址。", false, 0, "请检查网络，或改用「商店」按钮安装。");
                HintInfo.Text = gameName + " 获取下载地址失败，请改用商店安装。";
                return;
            }

            var url = urls.First();
            var folder = System.IO.Path.Combine(ModOtherGames.DownloadFolder(), "StoreGames");
            var fileName = System.IO.Path.GetFileName(new Uri(url).AbsolutePath);
            if (string.IsNullOrWhiteSpace(fileName) || !fileName.Contains('.'))
                fileName = gameName.Replace(' ', '_') + ".appxbundle";
            var target = System.IO.Path.Combine(folder, fileName);

            SetDlUi("正在下载 " + gameName + "……", true, 0, fileName);
            var progress = new Progress<ModOtherGames.DownloadProgressEventArgs>(p =>
            {
                BarDlTask.Value = p.Percent;
                TextDlDetail.Text = string.Format("{0}  {1:0.0}%  {2}/{3}  {4}/s",
                    fileName, p.Percent, ModOtherGames.FormatBytes(p.DoneBytes),
                    ModOtherGames.FormatBytes(p.TotalBytes), ModOtherGames.FormatBytes(p.SpeedBytes));
            });
            await ModOtherGames.DownloadUrlAsync(url, target, progress);

            SetDlUi("正在安装 " + gameName + "……（正版授权由系统校验）", true, 100, target);
            var ok = ModOtherGames.InstallAppx(target);
            if (ok)
            {
                SetDlUi(gameName + " 下载并安装成功。", false, 100, "");
                HintInfo.Text = gameName + " 已安装。若无法启动，请确认微软账号持有该游戏正版授权。";
            }
            else
            {
                SetDlUi(gameName + "：安装包已下载，但系统安装失败（通常表示账号未持有该游戏授权，或需管理员权限）。", false, 100, target);
                HintInfo.Text = gameName + " 安装失败，请确认账号持有正版授权后重试。";
            }
            RefreshStatus();
        }
        catch (Exception ex)
        {
            SetDlUi(gameName + " 下载失败：" + ex.Message, false, 0, "");
            HintInfo.Text = gameName + " 下载失败，请检查网络。";
        }
        finally
        {
            try { btn.IsEnabled = true; } catch { }
        }
    }

    // ---------- 教育版 ----------

    private async void BtnEducationLaunch_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var list = ModOtherGames.FindGamePackage(ModOtherGames.EducationFamilyPrefixes);
        if (list.Count == 0)
        {
            HintInfo.Text = "尚未安装 Minecraft Education，请先安装。";
            return;
        }

        var ok = ModOtherGames.LaunchPackage(list.First());
        HintInfo.Text = ok ? "已启动 Minecraft Education。" : "启动失败，请检查授权状态。";
    }

    private async void BtnEducationStore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ModOtherGames.OpenStore(ModOtherGames.ProductIdEducation);
    }

    private async void BtnEducationUpdate_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        BtnEducationUpdate.IsEnabled = false;
        TextEducationUpdate.Text = "正在检查最新版本……";
        var latest = await ModOtherGames.QueryStoreLatestVersionAsync(ModOtherGames.ProductIdEducation);
        var installed = ModOtherGames.FindGamePackage(ModOtherGames.EducationFamilyPrefixes);
        var instVer = installed.Count > 0 ? installed.First().Version : "";
        TextEducationUpdate.Text = string.IsNullOrEmpty(latest)
            ? (string.IsNullOrEmpty(instVer) ? "未能获取在线版本信息。" : "已安装版本 " + instVer + "（在线查询暂不可用）。")
            : "最新版本 " + latest + (string.IsNullOrEmpty(instVer) ? "（本机未安装）" : "；本机 " + instVer + (IsNewer(latest, instVer) ? "，可更新" : "，已是最新"));
        BtnEducationUpdate.IsEnabled = true;
    }
}
