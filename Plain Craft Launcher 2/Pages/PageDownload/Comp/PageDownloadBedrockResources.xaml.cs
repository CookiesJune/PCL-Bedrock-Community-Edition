namespace PCL;

public partial class PageDownloadBedrockResources
{
    public PageDownloadBedrockResources()
    {
        InitializeComponent();
        // Bedrock 资源聚合页：固定基岩版模式，展示全部 BE 资源（不过滤子分类）
        Content.IsBedrock = true;
        Content.IsBedrockAll = true;
        Content.IsBedrockLocked = true; // 固定 BE：点重置不回退为 JE
    }
}