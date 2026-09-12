using System.Windows.Input;
using PCL.Core.App;

namespace PCL;

/// <summary>
///     小teto实验室定制版：下载中心落地页，整合 Java / 基岩版 两个下载入口横幅。
/// </summary>
public partial class PageDownloadHome
{
    public PageDownloadHome()
    {
        InitializeComponent();
    }

    private void Java_Click(object sender, MouseButtonEventArgs e) =>
        Go(FormMain.PageSubType.DownloadInstall);

    private void Bedrock_Click(object sender, MouseButtonEventArgs e) =>
        Go(FormMain.PageSubType.DownloadBedrock);

    private void Dungeons_Click(object sender, MouseButtonEventArgs e) =>
        Go(FormMain.PageSubType.DownloadOtherGames);

    private void Legends_Click(object sender, MouseButtonEventArgs e) =>
        Go(FormMain.PageSubType.DownloadLegends);

    private void Go(FormMain.PageSubType subType)
    {
        if (ModMain.frmDownloadLeft is not null)
            ModMain.frmDownloadLeft.GoTo(subType);
    }
}
