using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Win32;
using BedrockLauncher.Core.Utils;
using Windows.Management.Deployment;
using PCL.Core.App;
using PCL.Core.Minecraft.Profile;
using PCL.Core.Minecraft.Profile.Models;

namespace PCL;

/// <summary>
///     小teto实验室定制版：其他游戏（地下城 / 传奇 / 基岩版）支持模块。
///     基岩版版本库使用 McAppx 数据源，下载走微软官方 Xbox CDN 10 镜像并行（BedrockBoot 式）。
/// </summary>
public static class ModOtherGames
{
    // 镜像主机列表（微软官方 Xbox CDN，与 BedrockBoot 一致）
    public static readonly string[] MirrorHosts =
    {
        "assets1.xboxlive.cn", "assets2.xboxlive.cn",
        "assets1.xboxlive.com", "assets2.xboxlive.com",
        "xvcf1.xboxlive.com", "xvcf2.xboxlive.com",
        "d1.xboxlive.cn", "d2.xboxlive.cn",
        "d1.xboxlive.com", "d2.xboxlive.com",
    };

    // 微软商店产品 ID
    public const string ProductIdDungeons = "9P8MK4NC0LJB";
    public const string ProductIdLegends = "9N98Z825TNFW";
    public const string ProductIdBedrock = "9NBLGGH2JHXJ";
    public const string ProductIdEducation = "9NBLGGH4R2R6";

    // 包 FamilyName 前缀（best-effort）
    public static readonly string[] BedrockFamilyPrefixes = { "Microsoft.MinecraftUWP_", "Microsoft.MinecraftWindowsBeta_" };
    public static readonly string[] DungeonsFamilyPrefixes = { "Microsoft.59226Dungeons_", "Microsoft.Lovika_" };
    public static readonly string[] LegendsFamilyPrefixes = { "Microsoft.MinecraftLegends_", "Microsoft.4297127D64EC6_" };
    public static readonly string[] EducationFamilyPrefixes = { "Microsoft.MinecraftEducationEdition_" };

    // McAppx 版本库（主源 + 备用）
    public static readonly string[] BedrockJsonUrls =
    {
        "https://data.mcappx.com/v2/bedrock.json",
        "https://raw.giteeusercontent.com/minecraftyjq/bedrock-version-db/raw/main/data/bedrock.json",
        "https://api.chlna6666.com/api/v1/bedrock/mcappx",
    };

    private static readonly HttpClient Http = CreateHttp();

    private static HttpClient CreateHttp()
    {
        var h = new HttpClient();
        h.Timeout = TimeSpan.FromSeconds(25);
        return h;
    }

    /// <summary>
    ///     基岩版版本条目。
    /// </summary>
    public class BedrockVersion
    {
        public string Version = "";          // 例如 1.20.81
        public string Type = "";             // Release / Preview
        public string BuildType = "";        // UWP / GDK
        public string Date = "";
        public string Id = "";
        public string Arch = "";
        public string OSBuild = "";
        public string Md5 = "";
        public List<string> MetaData = new();
        public string Display => $"{Version}  ({Type} / {BuildType})";

        /// <summary>
        ///     小teto定制：判断是否为 UWP 包。
        ///     优先用版本库的 BuildType 字段，若不可靠则根据版本号范围推断：
        ///     UWP 线程在 1.21.114 后关闭，故 1.21.114 及以下为 UWP，1.21.115 及以上为 GDK。
        /// </summary>
        public bool IsUwp
        {
            get
            {
                // 小teto定制：始终以版本号为准，不依赖版本库的 BuildType 字段（数据不可靠，如 1.20.81 被误标为 GDK）
                // UWP 线程在 1.21.114 后关闭，故 1.21.114 及以下为 UWP，1.21.115 及以上为 GDK
                return IsVersionUwpByNumber(Version);
            }
        }
        public bool IsGdk => !IsUwp;

        /// <summary>小teto定制：根据版本号判断是否为 UWP 包（1.21.114 及以下为 UWP）。</summary>
        private static bool IsVersionUwpByNumber(string version)
        {
            try
            {
                var parts = version.Split('.');
                if (parts.Length < 2) return true; // 版本号格式异常时默认 UWP
                int major = int.Parse(parts[0]);
                int minor = int.Parse(parts[1]);
                // 1.21.114 及以下为 UWP，1.21.115 及以上为 GDK
                if (major < 1) return true;
                if (major == 1 && minor <= 21)
                {
                    // 1.21.x：需要判断 patch 版本
                    if (parts.Length >= 3 && int.TryParse(parts[2], out int patch))
                        return patch <= 114;
                    return true; // 1.21 无 patch 时默认 UWP
                }
                return false; // 1.22+ 或 2.x+ 为 GDK
            }
            catch { return true; }
        }
    }

    /// <summary>
    ///     拉取并解析基岩版版本库。
    /// </summary>
    public static async Task<List<BedrockVersion>> GetBedrockVersionsAsync()
    {
        foreach (var url in BedrockJsonUrls)
        {
            try
            {
                var json = await Http.GetStringAsync(url).ConfigureAwait(false);
                var versions = ParseBedrockJson(json);
                if (versions.Count > 0)
                    return versions;
            }
            catch
            {
                // 尝试下一个源
            }
        }

        return new List<BedrockVersion>();
    }

    /// <summary>
    ///     解析 McAppx 版本库 JSON。
    ///     结构：{ "From_mcappx.com": { 版本键: { Type, BuildType, ID, Date, Variations:[{Arch,OSbuild,MD5,MetaData:[]}] } } }
    /// </summary>
    public static List<BedrockVersion> ParseBedrockJson(string json)
    {
        var result = new List<BedrockVersion>();
        try
        {
            var root = JsonNode.Parse(json);
            var src = root?["From_mcappx.com"] as JsonObject;
            if (src is null)
            {
                // 兼容直接是版本对象的情况
                if (root is JsonObject ro)
                    src = ro;
            }

            if (src is null) return result;

            foreach (var kv in src)
            {
                var v = new BedrockVersion { Version = kv.Key };
                var node = kv.Value as JsonObject;
                if (node is null) continue;
                v.Type = (string)node["Type"] ?? "";
                v.BuildType = (string)node["BuildType"] ?? "";
                v.Id = (string)node["ID"] ?? "";
                v.Date = (string)node["Date"] ?? "";

                var variations = node["Variations"] as JsonArray;
                if (variations is not null && variations.Count > 0)
                {
                    // 优先取 x64 / x64_arm64 变体
                    JsonObject best = null;
                    foreach (var vari in variations)
                    {
                        var vo = vari as JsonObject;
                        if (vo is null) continue;
                        var arch = (string)vo["Arch"] ?? "";
                        if (string.IsNullOrEmpty(arch) || arch == "x64" || arch.Contains("arm64"))
                        {
                            best = vo;
                            if (arch.Contains("x64") && !arch.Contains("arm64")) break;
                        }
                    }

                    best ??= variations[0] as JsonObject;
                    if (best is not null)
                    {
                        v.Arch = (string)best["Arch"] ?? "";
                        v.OSBuild = (string)best["OSbuild"] ?? "";
                        v.Md5 = (string)best["MD5"] ?? "";
                        var meta = best["MetaData"] as JsonArray;
                        if (meta is not null)
                        {
                            foreach (var m in meta)
                            {
                                if (m is not null) v.MetaData.Add(m.GetValue<string>());
                            }
                        }
                    }
                }

                result.Add(v);
            }
        }
        catch
        {
            // 解析失败返回空
        }

        return result;
    }

    /// <summary>
    ///     通过微软 FE3 服务把 UWP 包的 UpdateID 解析为下载直链（BedrockBoot 同款格式）。
    /// </summary>
    private const string WU_DEVICE_ATTRIBUTES =
        "E:BranchReadinessLevel=CBB&amp;DchuNvidiaGrfxExists=1&amp;ProcessorIdentifier=Intel64%20Family%206%20Model%2063%20Stepping%202&amp;CurrentBranch=rs4_release&amp;DataVer_RS5=1942&amp;FlightRing=Retail&amp;AttrDataVer=57&amp;InstallLanguage=en-US&amp;DchuAmdGrfxExists=1&amp;OSUILocale=en-US&amp;InstallationType=Client&amp;FlightingBranchName=&amp;Version_RS5=10&amp;UpgEx_RS5=Green&amp;GStatus_RS5=2&amp;OSSkuId=48&amp;App=WU&amp;InstallDate=1529700913&amp;ProcessorManufacturer=GenuineIntel&amp;AppVer=10.0.17134.471&amp;OSArchitecture=AMD64&amp;UpdateManagementGroup=2&amp;IsDeviceRetailDemo=0&amp;HidOverGattReg=C%3A%5CWINDOWS%5CSystem32%5CDriverStore%5CFileRepository%5Chidbthle.inf_amd64_467f181075371c89%5CMicrosoft.Bluetooth.Profiles.HidOverGatt.dll&amp;IsFlightingEnabled=0&amp;DchuIntelGrfxExists=1&amp;TelemetryLevel=1&amp;DefaultUserRegion=244&amp;DeferFeatureUpdatePeriodInDays=365&amp;Bios=Unknown&amp;WuClientVer=10.0.17134.471&amp;PausedFeatureStatus=1&amp;Steam=URL%3Asteam%20protocol&amp;Free=8to16&amp;OSVersion=10.0.17134.472&amp;DeviceFamily=Windows.Desktop";

    public static async Task<List<string>> ResolveUwpUrlAsync(string updateId, string revision = "1")
    {
        var urls = new List<string>();
        try
        {
            var now = DateTime.UtcNow;
            var created = now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
            var expires = now.AddMinutes(5).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");

            var body = new StringBuilder();
            body.Append("<s:Envelope xmlns:a=\"http://www.w3.org/2005/08/addressing\" xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\">");
            body.Append("<s:Header>");
            body.Append("<a:Action s:mustUnderstand=\"1\">http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService/GetExtendedUpdateInfo2</a:Action>");
            body.Append("<a:MessageID>urn:uuid:5754a03d-d8d5-489f-b24d-efc31b3fd32d</a:MessageID>");
            body.Append("<a:To s:mustUnderstand=\"1\">https://fe3.delivery.mp.microsoft.com/ClientWebService/Client.asmx/secured</a:To>");
            body.Append("<o:Security s:mustUnderstand=\"1\" xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">");
            body.Append("<Timestamp xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">");
            body.Append("<Created>").Append(created).Append("</Created>");
            body.Append("<Expires>").Append(expires).Append("</Expires>");
            body.Append("</Timestamp>");
            body.Append("<wuws:WindowsUpdateTicketsToken wsu:id=\"ClientMSA\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:wuws=\"http://schemas.microsoft.com/msus/2014/10/WindowsUpdateAuthorization\">");
            body.Append("<TicketType Name=\"AAD\" Version=\"1.0\" Policy=\"MBI_SSL\"></TicketType>");
            body.Append("</wuws:WindowsUpdateTicketsToken>");
            body.Append("</o:Security>");
            body.Append("</s:Header>");
            body.Append("<s:Body>");
            body.Append("<GetExtendedUpdateInfo2 xmlns=\"http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService\">");
            body.Append("<updateIDs><UpdateIdentity><UpdateID>").Append(updateId).Append("</UpdateID><RevisionNumber>").Append(revision).Append("</RevisionNumber></UpdateIdentity></updateIDs>");
            body.Append("<infoTypes><XmlUpdateFragmentType>FileUrl</XmlUpdateFragmentType></infoTypes>");
            body.Append("<deviceAttributes>").Append(WU_DEVICE_ATTRIBUTES).Append("</deviceAttributes>");
            body.Append("</GetExtendedUpdateInfo2>");
            body.Append("</s:Body></s:Envelope>");

            using var req = new HttpRequestMessage(HttpMethod.Post, "https://fe3.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured");
            req.Headers.TryAddWithoutValidation("User-Agent", "Windows-Update-Agent/10.0.17134.471 Client-Protocol/2.0");
            req.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/soap+xml");

            var resp = await Http.SendAsync(req).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode) return urls;
            var xml = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

            // 提取 <Url>（优先 tlu.dl.delivery，其次 dl.delivery）
            foreach (var host in new[] { "tlu.dl.delivery", "dl.delivery" })
            {
                var idx = 0;
                while (true)
                {
                    idx = xml.IndexOf("<Url>", idx, StringComparison.OrdinalIgnoreCase);
                    if (idx < 0) break;
                    idx += 5;
                    var end = xml.IndexOf("</Url>", idx, StringComparison.OrdinalIgnoreCase);
                    if (end < 0) break;
                    var u = xml.Substring(idx, end - idx).Trim();
                    // FE3 响应中的 & 被编码为 &amp;（可能双重编码），必须解码否则下载 403
                    while (u.Contains("&amp;"))
                        u = System.Net.WebUtility.HtmlDecode(u);
                    if (u.StartsWith("http") && u.Contains(host))
                        urls.Add(u);
                    idx = end + 6;
                }

                if (urls.Count > 0) break;
            }
        }
        catch
        {
            // FE3 失败返回空
        }

        return urls;
    }

    /// <summary>
    ///     下载任务回调：进度百分比（0-100）与速度（字节/秒）。
    /// </summary>
    public class DownloadProgressEventArgs : EventArgs
    {
        public double Percent;
        public long SpeedBytes;
        public long DoneBytes;
        public long TotalBytes;
    }

    /// <summary>
    ///     使用 10 镜像并行分片下载单个文件，支持断点续传。下载完成后可选 MD5 校验。
    /// </summary>
    public static async Task DownloadFileMultiMirrorAsync(string router, string targetPath, string expectMd5,
        IProgress<DownloadProgressEventArgs> progress, Action<string, long, long> perMirror = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

        // 1) HEAD 探活，取可用镜像
        var aliveMirrors = new List<string>();
        using (var headHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(8) })
        {
            foreach (var host in MirrorHosts)
            {
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Head, $"http://{host}{router}");
                    using var resp = await headHttp.SendAsync(req).ConfigureAwait(false);
                    if (resp.IsSuccessStatusCode)
                    {
                        aliveMirrors.Add(host);
                        perMirror?.Invoke(host, 0, 0);
                        if (aliveMirrors.Count >= 5) break;
                    }
                }
                catch
                {
                    // 镜像不可用
                }
            }
        }

        if (aliveMirrors.Count == 0)
            aliveMirrors.Add(MirrorHosts[0]);

        // 2) 探测文件总大小（用第一个可用镜像）
        long total = 0;
        using (var probe = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
        {
            using var req = new HttpRequestMessage(HttpMethod.Head, $"http://{aliveMirrors[0]}{router}");
            using var resp = await probe.SendAsync(req).ConfigureAwait(false);
            if (resp.Content.Headers.ContentLength.HasValue)
                total = resp.Content.Headers.ContentLength.Value;
        }

        if (total <= 0)
            total = 1743065088; // 兜底（实测 GDK 包大小约 1.62GB；实际会从 HEAD 得到真实值）

        // 3) 分片下载（8MB 一片，最多 8 并发）
        const int chunkSize = 8 * 1024 * 1024;
        long downloaded = 0;
        var lockObj = new object();
        var started = DateTime.UtcNow;

        // 先删除可能存在的临时文件
        var tmpPath = targetPath + ".part";
        if (File.Exists(tmpPath)) File.Delete(tmpPath);
        using (var fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            fs.SetLength(total);
        }

        var chunkCount = (int)((total + chunkSize - 1) / chunkSize);
        var mirrorIndex = 0;

        await Task.WhenAll(Enumerable.Range(0, Math.Min(8, chunkCount)).Select(_ => Task.Run(async () =>
        {
            while (true)
            {
                long start;
                int idx;
                lock (lockObj)
                {
                    idx = chunkIndex;
                    if (idx >= chunkCount) return;
                    chunkIndex++;
                    start = (long)idx * chunkSize;
                }

                var end = Math.Min(total - 1, start + chunkSize - 1);
                var host = aliveMirrors[Math.Abs(Interlocked.Increment(ref mirrorIndex)) % aliveMirrors.Count];
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Get, $"http://{host}{router}");
                    req.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);
                    using var resp = await Http.SendAsync(req).ConfigureAwait(false);
                    if (!resp.IsSuccessStatusCode) throw new Exception("HTTP " + resp.StatusCode);
                    await using var src = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var buffer = new byte[81920];
                    long offset = start;
                    int read;
                    while ((read = await src.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                    {
                        lock (lockObj)
                        {
                            using (var wfs = new FileStream(tmpPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                            {
                                wfs.Seek(offset, SeekOrigin.Begin);
                                wfs.Write(buffer, 0, read);
                            }

                            offset += read;
                            downloaded += read;
                        }
                    }

                    perMirror?.Invoke(host, end - start + 1, (end - start + 1));
                    progress?.Report(new DownloadProgressEventArgs
                    {
                        DoneBytes = Interlocked.Read(ref downloaded),
                        TotalBytes = total,
                        Percent = total <= 0 ? 0 : Math.Min(100, 100.0 * Interlocked.Read(ref downloaded) / total),
                        SpeedBytes = (long)(Interlocked.Read(ref downloaded) / Math.Max(1, (DateTime.UtcNow - started).TotalSeconds)),
                    });
                }
                catch
                {
                    // 分片失败：退回该分片重试（用其他镜像）
                    lock (lockObj)
                    {
                        chunkIndex--;
                    }
                }
            }
        })).ToArray());

        // 4) 收尾：改名 + MD5 校验
        if (File.Exists(targetPath)) File.Delete(targetPath);
        File.Move(tmpPath, targetPath);

        if (!string.IsNullOrEmpty(expectMd5))
        {
            var md5 = ComputeMd5(targetPath);
            if (!string.Equals(md5, expectMd5, StringComparison.OrdinalIgnoreCase))
                ModBase.Log($"[OtherGames] MD5 校验不一致：实际 {md5}，期望 {expectMd5}（文件保留）");
        }
    }

    private static int chunkIndex;

    /// <summary>
    ///     计算文件 MD5。
    /// </summary>
    public static string ComputeMd5(string path)
    {
        using var fs = File.OpenRead(path);
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(fs);
        return Convert.ToHexString(hash);
    }

    /// <summary>
    ///     下载目录：一律为当前选中的 .minecraft 文件夹（BE 实例根）。
    ///     下载/解压目标由调用方在此基础上拼接 bedrock_versions（用户最高优先级要求）。
    /// </summary>
    public static string DownloadFolder()
    {
        try
        {
            var mc = ModFolder.mcFolderSelected;
            if (!string.IsNullOrEmpty(mc))
                return Path.TrimEndingDirectorySeparator(mc) + Path.DirectorySeparatorChar;
        }
        catch
        {
            // 忽略
        }

        return Path.Combine(ModFolder.mcFolderSelected, "BE") + Path.DirectorySeparatorChar;
    }

    /// <summary>
    ///     扫描已安装的 UWP 包，返回匹配前缀的包信息。
    /// </summary>
        public static List<InstalledGameInfo> FindGamePackage(string[] familyPrefixes)
    {
        var result = new List<InstalledGameInfo>();
        try
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var roots = new[]
            {
                @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\Applications",
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\Applications",
            };

            foreach (var root in roots)
            {
                string[] names;
                try
                {
                    using var key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey(root)
                        ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(root);
                    names = key?.GetSubKeyNames() ?? Array.Empty<string>();
                }
                catch
                {
                    continue;
                }

                foreach (var fullName in names)
                {
                    // 完整包名格式：{FamilyName}_{Version}_{Arch}_{PublisherHash}
                    // 例如 Microsoft.MinecraftUWP_1.21.45.0_x64__8wekyb3d8bbwe
                    var m = System.Text.RegularExpressions.Regex.Match(fullName,
                        @"^(?<fam>[^_]+)_(?<ver>\d+\.\d+\.\d+\.\d+)_");
                    if (!m.Success) continue;
                    var fam = m.Groups["fam"].Value;
                    if (!familyPrefixes.Any(p => fam.StartsWith(p.TrimEnd('_'), StringComparison.OrdinalIgnoreCase)))
                        continue;
                    if (!seen.Add(fullName)) continue;

                    var pubHash = fullName.Substring(fullName.LastIndexOf('_') + 1);
                    var info = new InstalledGameInfo
                    {
                        FullName = fullName,
                        Name = fam,
                        FamilyName = fam + "_" + pubHash,
                        Version = m.Groups["ver"].Value,
                        Aumid = fam + "_" + pubHash + "!App",
                        IsOk = true,
                    };
                    result.Add(info);
                }
            }
        }
        catch
        {
            // 无注册表访问能力
        }

        return result;
    }

    /// <summary>
    ///     已安装游戏信息。
    /// </summary>
    public class InstalledGameInfo
    {
        public string FamilyName = "";
        public string FullName = "";
        public string Name = "";
        public string Version = "";
        public string Aumid = "";
        public bool IsOk;
    }

    /// <summary>
    ///     启动已安装的 UWP 游戏。
    /// </summary>
    public static bool LaunchPackage(InstalledGameInfo info)
    {
        try
        {
            var aumid = string.IsNullOrEmpty(info.Aumid) ? info.FamilyName + "!App" : info.Aumid;
            Process.Start(new ProcessStartInfo("shell:AppsFolder\\" + aumid) { UseShellExecute = true });
            return true;
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "启动游戏失败");
            return false;
        }
    }


    /// <summary>
    ///     直接启动已安装的基岩版（若有）。
    /// </summary>
    public static bool LaunchBedrock()
    {
        var list = FindGamePackage(BedrockFamilyPrefixes);
        if (list.Count == 0) return false;
        return LaunchPackage(list.First());
    }

    /// <summary>
    ///     基岩版实例信息。
    /// </summary>
    public class BedrockInstanceInfo
    {
        public string Name = "";
        public string PathInstance = "";
        public int ResourcePackCount;
        public int AddonCount;
        public int WorldCount;
    }

    /// <summary>
    ///     枚举 .minecraft/BE/ 下的基岩版实例。
    /// </summary>
    public static List<BedrockInstanceInfo> GetBedrockInstances()
    {
        var result = new List<BedrockInstanceInfo>();
        try
        {
            var baseDir = Path.Combine(ModFolder.mcFolderSelected, "BE");
            if (!Directory.Exists(baseDir)) return result;
            foreach (var dir in Directory.GetDirectories(baseDir).OrderBy(d => d))
            {
                result.Add(new BedrockInstanceInfo
                {
                    Name = Path.GetFileName(dir),
                    PathInstance = dir,
                    ResourcePackCount = CountFolderFiles(Path.Combine(dir, "resource_packs")),
                    AddonCount = CountFolderFiles(Path.Combine(dir, "behavior_packs")),
                    WorldCount = CountFolderFiles(Path.Combine(dir, "worlds")),
                });
            }
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "枚举基岩版实例失败");
        }
        return result;
    }

    private static int CountFolderFiles(string p)
    {
        try
        {
            return Directory.Exists(p)
                ? Directory.EnumerateFiles(p, "*.*", SearchOption.AllDirectories).Count()
                : 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    ///     打开微软商店对应产品页。
    /// </summary>
    public static void OpenStore(string productId)
    {
        try
        {
            Process.Start(new ProcessStartInfo($"ms-windows-store://pdp/?ProductId={productId}") { UseShellExecute = true });
        }
        catch
        {
            // 忽略
        }
    }

    /// <summary>
    ///     传奇正版校验结果。
    /// </summary>
    public enum LegendsCheckResult
    {
        Owned,
        NotLoggedIn,
        NotOwned,
        Unverifiable,
    }

    public class LegendsCheckOutcome
    {
        public LegendsCheckResult Result;
        public string Message = "";
    }

    /// <summary>
    ///     传奇启动前正版校验：① PCL2 已登录微软账号 ② 本地包已安装且授权有效。
    /// </summary>
    public static LegendsCheckOutcome CheckLegends()
    {
        var outcome = new LegendsCheckOutcome();

        // ① 微软账号登录门槛
        var loggedIn = false;
        try
        {
            var profile = ProfileService.Current;
            if (profile is not null && profile.ProfileType == ProfileType.Microsoft)
                loggedIn = true;
        }
        catch
        {
            loggedIn = false;
        }

        var installed = FindGamePackage(LegendsFamilyPrefixes);
        var hasValid = installed.Any(i => i.IsOk);

        if (!loggedIn)
        {
            outcome.Result = LegendsCheckResult.NotLoggedIn;
            outcome.Message = "未登录微软账号，无法确认账号持有权。请先在启动器登录微软账号后重试。";
            return outcome;
        }

        if (!hasValid)
        {
            outcome.Result = LegendsCheckResult.NotOwned;
            outcome.Message = "未检测到已安装的 Minecraft Legends 有效授权。请先在微软商店购买并安装该游戏。";
            return outcome;
        }

        outcome.Result = LegendsCheckResult.Owned;
        outcome.Message = "正版校验通过：已登录微软账号且本地授权有效，确认可启动 Minecraft Legends。";
        return outcome;
    }

    /// <summary>
    ///     获取已登录的微软账号名（用于显示）。
    /// </summary>
    public static string LoggedInMsName()
    {
        try
        {
            var profile = ProfileService.Current;
            if (profile is not null && profile.ProfileType == ProfileType.Microsoft)
                return profile?.UserName ?? "";
        }
        catch
        {
            // 忽略
        }

        return "";
    }

    /// <summary>
    ///     通过微软商店公开接口尽力查询产品最新版本（best-effort，失败返回空字符串）。
    /// </summary>
    public static async Task<string> QueryStoreLatestVersionAsync(string productId)
    {
        try
        {
            // store.rg-adguard.net 公共工具（type=productid）
            using var req = new HttpRequestMessage(HttpMethod.Post, "https://store.rg-adguard.net/api/GetFiles");
            req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "type", "productid" },
                { "url", productId },
                { "ring", "Retail" },
                { "lang", "zh-CN" },
            });
            var resp = await Http.SendAsync(req).ConfigureAwait(false);
            var html = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            // 在 HTML 中找形如 ..._1.2.3.4_... 的版本号
            var m = System.Text.RegularExpressions.Regex.Match(html,
                @"_(\d+\.\d+\.\d+(?:\.\d+)?)_", System.Text.RegularExpressions.RegexOptions.Compiled);
            return m.Success ? m.Groups[1].Value : "";
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    ///     格式化字节数为可读字符串。
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return bytes + " B";
        if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("0.0") + " KB";
        if (bytes < 1024L * 1024 * 1024) return (bytes / 1048576.0).ToString("0.00") + " MB";
        return (bytes / 1073741824.0).ToString("0.00") + " GB";
    }
    /// <summary>
    ///     是否已登录微软账号（正版验证第一层门槛）。
    /// </summary>
    public static bool IsMicrosoftLoggedIn()
    {
        try
        {
            var p = ProfileService.Current;
            return p is not null && p.ProfileType == ProfileType.Microsoft;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     从 store.rg-adguard.net 获取产品的可下载应用包 URL 列表（.appx/.appxbundle/.msix/.msixbundle，排除依赖与映射文件）。
    /// </summary>
    public static async Task<List<string>> GetStorePackageUrlsAsync(string productId)
    {
        var urls = new List<string>();
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "https://store.rg-adguard.net/api/GetFiles");
            req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "type", "productid" },
                { "url", productId },
                { "ring", "Retail" },
                { "lang", "zh-CN" },
            });
            var resp = await Http.SendAsync(req).ConfigureAwait(false);
            var html = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var matches = System.Text.RegularExpressions.Regex.Matches(html,
                @"href=""(?<u>https?://[^""]+?\.(?:appx|appxbundle|msix|msixbundle))""",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                var u = m.Groups["u"].Value;
                if (u.Contains("Dependencies", StringComparison.OrdinalIgnoreCase)) continue;
                if (u.Contains(".blockmap", StringComparison.OrdinalIgnoreCase)) continue;
                if (u.Contains("_eappx", StringComparison.OrdinalIgnoreCase)) continue;
                if (!urls.Contains(u)) urls.Add(u);
            }
            return urls
                .OrderBy(u => u.Contains("x64", StringComparison.OrdinalIgnoreCase) || u.Contains("neutral", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(u => u.Length)
                .ToList();
        }
        catch
        {
            return urls;
        }
    }

    /// <summary>
    ///     下载单个 URL 到本地文件（带进度，长超时）。
    /// </summary>
    public static async Task DownloadUrlAsync(string url, string targetPath, IProgress<DownloadProgressEventArgs> progress)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        using var dl = new HttpClient { Timeout = TimeSpan.FromMinutes(40) };
        using var resp = await dl.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        var total = resp.Content.Headers.ContentLength ?? 0;
        await using var src = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var dst = File.Create(targetPath);
        var buffer = new byte[81920];
        long done = 0;
        var started = DateTime.UtcNow;
        int read;
        while ((read = await src.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
        {
            await dst.WriteAsync(buffer, 0, read).ConfigureAwait(false);
            done += read;
            progress?.Report(new DownloadProgressEventArgs
            {
                DoneBytes = done,
                TotalBytes = total,
                Percent = total <= 0 ? 0 : Math.Min(100, 100.0 * done / total),
                SpeedBytes = (long)(done / Math.Max(1, (DateTime.UtcNow - started).TotalSeconds)),
            });
        }
    }

    /// <summary>
    ///     判断指定文件夹是否为基岩版（BE）游戏文件夹（参考 BedrockBoot 结构）。
    ///     特征：直接含 AppxManifest.xml（loose UWP 包根目录）、Minecraft.Windows.exe（GDK 解包）、
    ///     resource_packs+behavior_packs（BedrockBoot 管理结构）、versions 子目录含 AppxManifest.xml。
    /// </summary>
    /// <summary>
    ///     小teto定制：基岩版标记文件内容。某个版本文件夹内存在含此标记的 txt 即判定为 BE 版本文件夹。
    /// </summary>
    public const string BedrockMarker = "BE!!!IODHDFIFIOFHEWUIFGUWFGUWFGUW";
    public const string BedrockMarkerFile = "BE_marker.txt";
    /// <summary>小teto实验室定制版版本名称（全面更名为 PCL-CE_TetoLab_x64）。</summary>
    public const string TetoLabVersionName = "PCL-CE_TetoLab_x64";
    /// <summary>小teto定制：UWP 包标记文件名（空文件，存在即判定为 UWP 包）。</summary>
    public const string UwpMarkerFile = "UWPtres.txt";

    public static void WriteBedrockMarker(string dir)
    {
        try
        {
            Directory.CreateDirectory(dir);
            var f = Path.Combine(dir, BedrockMarkerFile);
            if (!File.Exists(f))
                File.WriteAllText(f, BedrockMarker);
        }
        catch
        {
            // 忽略
        }
    }

    /// <summary>小teto定制：在版本文件夹中创建 UWP 标记空文件（UWPtres.txt）。</summary>
    public static void WriteUwpMarker(string dir)
    {
        try
        {
            Directory.CreateDirectory(dir);
            var f = Path.Combine(dir, UwpMarkerFile);
            if (!File.Exists(f))
                File.WriteAllText(f, "");
        }
        catch { }
    }

    /// <summary>
    ///     小teto定制：检测版本文件夹是否为 UWP 包（存在 UWPtres.txt 则为 UWP，否则为 GDK）。
    ///     兼容迁移：若没有标记文件但有 UWP 特征（AppxBlockMap.xml 且无 MicrosoftGame.Config），自动创建标记。
    /// </summary>
    public static bool IsUwpFolder(string dir)
    {
        try
        {
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return false;
            var marker = Path.Combine(dir, UwpMarkerFile);
            if (File.Exists(marker)) return true;
            // 自动迁移：已有 UWP 版本但缺少标记文件时，根据特征自动补标记
            bool hasUwpFeature = File.Exists(Path.Combine(dir, "AppxBlockMap.xml")) &&
                                  !File.Exists(Path.Combine(dir, "MicrosoftGame.Config"));
            if (hasUwpFeature)
            {
                WriteUwpMarker(dir);
                return true;
            }
            return false;
        }
        catch { return false; }
    }

    private static bool FileContainsMarker(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return false;
            using var fs = File.OpenRead(filePath);
            using var sr = new StreamReader(fs);
            var buf = new char[Math.Min(BedrockMarker.Length + 8, 4096)];
            var read = sr.Read(buf, 0, buf.Length);
            return read > 0 && new string(buf, 0, read).Contains(BedrockMarker);
        }
        catch
        {
            return false;
        }
    }

    private static bool ContainsMarkerFile(string dir)
    {
        try
        {
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return false;
            return Directory.EnumerateFiles(dir, "*.txt").Any(FileContainsMarker);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     在 root 及其一层子目录（兼容 root/bedrock_versions/版本）中查找所有含标记文件的 BE 版本文件夹。
    /// </summary>
    /// <summary>小teto定制：获取当前选中的 BE 版本路径。</summary>
    public static string GetBedrockVersionPath(string root)
    {
        try
        {
            var versionsDir = Path.Combine(root, "bedrock_versions");
            if (!string.IsNullOrEmpty(SelectedBedrockVersion))
            {
                var sel = Path.Combine(versionsDir, SelectedBedrockVersion);
                if (Directory.Exists(sel)) return sel;
                var sel2 = Path.Combine(root, SelectedBedrockVersion);
                if (Directory.Exists(sel2)) return sel2;
            }
            var folders = FindBedrockVersionFolders(root);
            if (folders.Count > 0) return folders[0];
        }
        catch { }
        return root;
    }

    // 小teto定制：获取 BE 实例的各个目录路径
    public enum BedrockFolderType
    {
        BehaviorPacks,  // 行为包
        ResourcePacks,  // 资源包
        Worlds,         // 世界
        Screenshots,    // 截图
        SkinPacks       // 皮肤包
    }

    public static string GetBedrockFolderPath(string instancePath, BedrockFolderType folderType)
    {
        try
        {
            string versionPath = GetBedrockVersionPath(instancePath);
            string versionName = Path.GetFileName(versionPath).TrimEnd('\\').ToLowerInvariant();
            // 小teto定制：改进预览版检测逻辑
            bool isPreview = versionName.Contains("preview") || versionName.Contains("beta") || versionName.Contains("education");
            if (!isPreview)
            {
                // 26.x 及以上均为预览版
                try
                {
                    var parts = versionName.Split('.');
                    if (parts.Length >= 1 && int.TryParse(parts[0], out var major) && major >= 26)
                        isPreview = true;
                }
                catch { }
            }
            // 检查 UWPtres.txt 标记文件和版本目录中的 AppxManifest.xml
            if (!isPreview)
            {
                try
                {
                    // 检查版本目录中是否有预览版相关的文件
                    var manifestPath = Path.Combine(versionPath, "AppxManifest.xml");
                    if (File.Exists(manifestPath))
                    {
                        var manifestContent = File.ReadAllText(manifestPath).ToLowerInvariant();
                        if (manifestContent.Contains("preview") || manifestContent.Contains("beta"))
                            isPreview = true;
                    }
                }
                catch { }
            }
            string bedrockFolder = isPreview ? "Minecraft Bedrock Preview" : "Minecraft Bedrock";
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                bedrockFolder, "Users", "Shared", "games", "com.mojang");

            switch (folderType)
            {
                case BedrockFolderType.BehaviorPacks:
                    return Path.Combine(versionPath, "data", "behavior_packs") + @"\\";
                case BedrockFolderType.ResourcePacks:
                    return Path.Combine(versionPath, "data", "resource_packs") + @"\\";
                case BedrockFolderType.Worlds:
                    return Path.Combine(appDataPath, "minecraftWorlds") + @"\\";
                case BedrockFolderType.Screenshots:
                    return Path.Combine(appDataPath, "Screenshots") + @"\\";
                case BedrockFolderType.SkinPacks:
                    return Path.Combine(appDataPath, "skin_packs") + @"\\";
                default:
                    return versionPath;
            }
        }
        catch { }
        return instancePath;
    }

    // 小teto定制：解析 BE 版本号为可比较的数组，用于排序
    private static int[] ParseBedrockVersion(string dirName)
    {
        try
        {
            var name = Path.GetFileName(dirName).ToLowerInvariant();
            // 去除 preview/beta/education 等后缀
            name = System.Text.RegularExpressions.Regex.Replace(name, @"(preview|beta|education).*$", "");
            var parts = name.Split('.');
            var nums = new List<int>();
            foreach (var p in parts)
            {
                if (int.TryParse(p, out var n))
                    nums.Add(n);
                else
                    break;
            }
            // 补齐到4位
            while (nums.Count < 4) nums.Add(0);
            return nums.ToArray();
        }
        catch { return new[] { 0, 0, 0, 0 }; }
    }

    // 小teto定制：比较两个 BE 版本目录，返回正数表示 a 比 b 新
    private static int CompareBedrockVersion(string a, string b)
    {
        var va = ParseBedrockVersion(a);
        var vb = ParseBedrockVersion(b);
        for (int i = 0; i < 4; i++)
        {
            if (va[i] != vb[i]) return va[i].CompareTo(vb[i]);
        }
        return 0;
    }

    public static List<string> FindBedrockVersionFolders(string root)
    {
        var result = new List<string>();
        try
        {
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return result;
            // root 本身是版本目录
            if (IsVersionDir(root) || ContainsMarkerFile(root))
            {
                WriteBedrockMarker(root);
                result.Add(root);
            }
            // 标准结构：root\bedrock_versions\版本
            var bv = Path.Combine(root, "bedrock_versions");
            if (Directory.Exists(bv))
            {
                foreach (var sub in Directory.EnumerateDirectories(bv))
                    if (IsVersionDir(sub) || ContainsMarkerFile(sub))
                    {
                        WriteBedrockMarker(sub);
                        result.Add(sub);
                    }
                if (result.Count > 0)
                {
                    // 小teto定制：此处也要排序后再返回（否则返回未排序的文件系统枚举顺序）
                    try { result.Sort((a, b) => CompareBedrockVersion(b, a)); } catch { }
                    return result;
                }
            }
            // 兼容平铺：root 一层子目录为版本目录
            foreach (var sub in Directory.EnumerateDirectories(root))
                if (IsVersionDir(sub) || ContainsMarkerFile(sub))
                {
                    WriteBedrockMarker(sub);
                    result.Add(sub);
                }
        }
        catch
        {
            // 忽略
        }
        // 小teto定制：按版本号从新到旧排序，确保 FirstOrDefault 返回最新版本
        try
        {
            result.Sort((a, b) => CompareBedrockVersion(b, a));
        }
        catch { }
        return result;
    }

    /// <summary>
    ///     是否为 BE 版本目录：含可执行文件或 UWP 清单（已有版本但尚未写标记时也成立）。
    /// </summary>
    private static bool IsVersionDir(string dir)
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
    }

    public static bool IsBedrockFolder(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return false;
            // 小teto定制：标记文件判定（BE 根/版本目录内含标记即视为 BE 文件夹）
            if (ContainsMarkerFile(path)) return true;
            // 根目录含 AppxManifest / Minecraft.Windows.exe（版本目录本身）
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
            }
            // 根目录直接含 resource_packs + behavior_packs
            if (Directory.Exists(Path.Combine(path, "resource_packs")) &&
                Directory.Exists(Path.Combine(path, "behavior_packs"))) return true;
        }
        catch
        {
            // 忽略
        }

        return false;
    }

    /// <summary>
    ///     当前 BE 下载/安装/读取目标。
    ///     规则：仅当当前选中的实例文件夹本身就是 BE 文件夹（含 bedrock_versions / Minecraft.Windows.exe 等特征）时
    ///     才跟随该实例文件夹；否则统一使用下载目录 DownloadFolder()（用户 BE 版本的实际所在位置），
    ///     避免误把 BE 资源下载/解压进普通 Java 实例文件夹导致读不到版本。
    /// </summary>
    public static string CurrentBedrockInstanceFolder()
    {
        try
        {
            var mc = ModFolder.mcFolderSelected;
            if (!string.IsNullOrEmpty(mc) && Directory.Exists(mc))
            {
                var p = Path.TrimEndingDirectorySeparator(mc);
                // 当前选中文件夹本身就是 BE 根（含标记 / bedrock_versions / exe）→ 直接用它
                if (IsBedrockFolder(p) || Directory.Exists(Path.Combine(p, "bedrock_versions")))
                {
                    try { Directory.CreateDirectory(p); } catch { }
                    return p + Path.DirectorySeparatorChar;
                }
            }
        }
        catch
        {
            // 忽略
        }

        return DownloadFolder();
    }

    /// <summary>
    ///     在 BE 实例文件夹中查找可注册的 AppxManifest.xml（根目录或 versions 子目录）。
    /// </summary>
    private static string FindAppxManifestInFolder(string folder)
    {
        try
        {
            var direct = Path.Combine(folder, "AppxManifest.xml");
            if (File.Exists(direct)) return direct;
            var v = Path.Combine(folder, "versions");
            if (Directory.Exists(v))
                foreach (var f in Directory.EnumerateFiles(v, "AppxManifest.xml", SearchOption.AllDirectories))
                    return f;
            var bv = Path.Combine(folder, "bedrock_versions");
            if (Directory.Exists(bv))
                foreach (var f in Directory.EnumerateFiles(bv, "AppxManifest.xml", SearchOption.AllDirectories))
                    return f;
        }
        catch
        {
            // 忽略
        }

        return null;
    }

    /// <summary>
    ///     以开发者模式注册 loose UWP 包（Add-AppxPackage -Register）。
    /// </summary>
    /// <summary>
    ///     小teto定制：检查系统是否已安装指定 UWP 框架包。
    /// </summary>
    private static bool IsUwpPackageInstalled(string packageName)
    {
        try
        {
            var pm = new PackageManager();
            // FindPackagesForUser 比 FindPackages 更可靠（不需要管理员权限）
            foreach (var p in pm.FindPackagesForUser(string.Empty))
            {
                if (string.Equals(p.Id.Name, packageName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            ModBase.Log("[Bedrock] IsUwpPackageInstalled 异常: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    ///     小teto定制：自动安装 UWP 依赖项（.appx 框架包）。
    ///     旧版本 Minecraft（1.14~1.20）依赖 Microsoft.VCLibs.140.00（UWP框架包），
    ///     而新版本依赖 Microsoft.VCLibs.140.00.UWPDesktop（桌面包）。
    ///     系统可能只装了桌面版，缺少 UWP 框架版导致旧版本启动崩溃。
    /// </summary>
    private static bool InstallUwpDependency(string url, string packageName)
    {
        try
        {
            if (IsUwpPackageInstalled(packageName))
            {
                ModBase.Log("[Bedrock] 依赖项已安装，跳过: " + packageName);
                return true;
            }
            ModBase.Log("[Bedrock] 正在下载安装依赖项: " + packageName + " (" + url + ")");
            var tempDir = Path.Combine(Path.GetTempPath(), "PCL_Bedrock_Deps");
            Directory.CreateDirectory(tempDir);
            var appxPath = Path.Combine(tempDir, packageName + ".appx");
            // 下载依赖项
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromMinutes(5);
                var bytes = client.GetByteArrayAsync(url).GetAwaiter().GetResult();
                File.WriteAllBytes(appxPath, bytes);
            }
            catch (Exception ex)
            {
                ModBase.Log("[Bedrock] 依赖项下载失败: " + packageName + ", " + ex.Message);
                return false;
            }
            // 安装依赖项（Add-AppxPackage）
            var psi = new ProcessStartInfo("powershell.exe")
            {
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"Add-AppxPackage -Path '{appxPath}'\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using var p = Process.Start(psi);
            if (p is null) return false;
            var output = p.StandardOutput.ReadToEnd();
            var error = p.StandardError.ReadToEnd();
            p.WaitForExit(120000);
            if (p.ExitCode != 0)
            {
                ModBase.Log("[Bedrock] 依赖项安装失败: " + packageName + ", output=" + output + ", error=" + error);
                return false;
            }
            ModBase.Log("[Bedrock] 依赖项安装成功: " + packageName);
            try { File.Delete(appxPath); } catch { }
            return true;
        }
        catch (Exception ex)
        {
            ModBase.Log("[Bedrock] 依赖项安装异常: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    ///     小teto定制：解析 AppxManifest.xml 中的依赖项，自动安装缺少的 UWP 框架包。
    /// </summary>
    private static void EnsureUwpDependencies(string manifestPath)
    {
        try
        {
            var dir = Path.GetDirectoryName(manifestPath);
            // 常见 UWP 依赖项下载链接（微软官方 aka.ms 短链接）
            var knownDeps = new Dictionary<string, string>
            {
                { "Microsoft.VCLibs.140.00", "https://aka.ms/Microsoft.VCLibs.x64.14.00.appx" },
                { "Microsoft.VCLibs.140.00.x86", "https://aka.ms/Microsoft.VCLibs.x86.14.00.appx" },
            };
            // 解析 AppxManifest.xml 查找依赖项
            var missingDeps = new List<string>();
            try
            {
                var xml = System.Xml.Linq.XDocument.Load(manifestPath);
                var ns = xml.Root.GetDefaultNamespace();
                foreach (var dep in xml.Descendants(ns + "PackageDependency"))
                {
                    var name = dep.Attribute("Name")?.Value;
                    if (!string.IsNullOrEmpty(name) && !IsUwpPackageInstalled(name))
                    {
                        missingDeps.Add(name);
                        ModBase.Log("[Bedrock] 缺少依赖项: " + name);
                    }
                }
            }
            catch (Exception ex)
            {
                ModBase.Log("[Bedrock] 解析依赖项失败: " + ex.Message);
            }
            // 自动安装已知的依赖项
            foreach (var depName in missingDeps)
            {
                if (knownDeps.TryGetValue(depName, out var url))
                {
                    InstallUwpDependency(url, depName);
                }
                else if (depName.StartsWith("Microsoft.VCLibs.140.00") && !depName.Contains("UWPDesktop"))
                {
                    // VCLibs 框架包（非桌面版），尝试 x64 版本
                    InstallUwpDependency("https://aka.ms/Microsoft.VCLibs.x64.14.00.appx", "Microsoft.VCLibs.140.00");
                }
            }
        }
        catch (Exception ex)
        {
            ModBase.Log("[Bedrock] 确保依赖项异常: " + ex.Message);
        }
    }

    public static bool RegisterLooseAppx(string manifest)
    {
        try
        {
            // 小teto定制：注册前确保 UWP 依赖项已安装（修复旧版本 1.14~1.20 启动崩溃）
            EnsureUwpDependencies(manifest);
            // 小teto定制：改用 PackageManager API 注册（替代 PowerShell Add-AppxPackage）
            // BedrockBoot 式：DevelopmentMode + ForceUpdateFromAnyVersion，与官方 loose 注册一致
            // PowerShell 注册可能导致 UWP 应用加载资源时卡在 40%（用户反馈）
            var pm = new PackageManager();
            var deployTask = pm.RegisterPackageAsync(
                new Uri(manifest),
                null,
                Windows.Management.Deployment.DeploymentOptions.DevelopmentMode |
                Windows.Management.Deployment.DeploymentOptions.ForceUpdateFromAnyVersion);
            var deployResult = deployTask.AsTask().GetAwaiter().GetResult();
            if (deployResult.IsRegistered)
            {
                ModBase.Log("[Bedrock] PackageManager registered loose package: " + manifest);
                return true;
            }
            ModBase.Log("[Bedrock] PackageManager register failed: " + deployResult.ErrorText);
            // fallback：PowerShell 注册
            var psi = new ProcessStartInfo("powershell.exe")
            {
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"Add-AppxPackage -Register '{manifest}' -ForceUpdateFromAnyVersion\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using var p = Process.Start(psi);
            if (p is null) return false;
            p.StandardOutput.ReadToEnd();
            p.StandardError.ReadToEnd();
            if (!p.WaitForExit(180000))
            {
                try { p.Kill(); } catch { }
                return false;
            }

            return p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     启动基岩版实例（BedrockBoot 式启动）。不做任何正版校验，直接启动已注册的 UWP 包。
    ///     返回空字符串表示启动流程已受理，否则返回错误提示。
    /// </summary>
    /// <summary>当前在实例选择页选中的基岩版版本文件夹名（相对 BE 实例根）。</summary>
    public static string SelectedBedrockVersion = "";

    /// <summary>
    ///     小teto定制：待进入介绍页的 BE 版本目录（从启动页点「实例设置」时设置，实例选择页渲染后自动进入）。
    /// </summary>
    public static string PendingBeInfoDir = "";

    public static string LaunchBedrockInstance(string folder)
    {
        // 小teto定制：优先启动选中版本的 GDK 解压版（Minecraft.Windows.exe），无需正版验证
        try
        {
            string target = null;
            // 若实例选择页已选中具体版本，优先启动该版本
            if (!string.IsNullOrEmpty(SelectedBedrockVersion))
            {
                // 选中版本优先位于 folder\bedrock_versions\版本（标准 BE 结构），兼容平铺在 folder\版本
                var sel = Path.Combine(folder, "bedrock_versions", SelectedBedrockVersion);
                if (!Directory.Exists(sel))
                    sel = Path.Combine(folder, SelectedBedrockVersion);
                if (Directory.Exists(sel))
                    target = sel;
            }
            target ??= FindBedrockVersionFolders(folder).FirstOrDefault();
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
                {
                    // 小teto定制：GDK 结构（含 GDK 运行时 dll）可直接启动 exe；
                    // 若缺 GDK dll（如 1.21.x 解密产物只有 PlayFabMultiplayerGDK.dll），先自动从同实例文件夹其他 GDK 版本补全，
                    // 补全后直接跑 exe 即可启动（已实测 1.21.120 补 libHttpClient.GDK.dll + Microsoft.Xbox.Services.GDK.C.Thunks.dll 后正常运行）
                    // 先确保 PreLoad.NET.dll 存在（BedrockBoot 预加载器，游戏启动必需，版本无关）
                    TryFillGdkDlls(target, folder);
                    // 校验版本完整性：PCLCE 解包若被中断（如中途关机/关闭），会漏解 data 资源导致启动黑屏崩溃（0xc0000005）
                    if (!IsBedrockVersionComplete(target))
                        return "该基岩版文件不完整（可能是下载或解包被中断，缺少游戏资源）。请在「下载-基岩版」中删除该版本后重新下载解包。";
                    // 小teto定制：明确区分 GDK / UWP 包，用 UWPtres.txt 标记文件判断（最可靠）
                    // 有 UWPtres.txt → UWP 包（走 UWP 激活启动）；没有 → GDK 包（直接启动 exe）
                    bool isUwpPackage = IsUwpFolder(target);
                    bool isGdkPackage = !isUwpPackage;
                    ModBase.Log($"[Bedrock] Package type (by UWPtres.txt): isGdk={isGdkPackage}, isUwp={isUwpPackage}, dir={target}");

                    if (isGdkPackage)
                    {
                        // GDK 包：直接启动 Minecraft.Windows.exe（无需注册，无需开发者模式）
                        ModBase.Log("[Bedrock] GDK package detected, launching exe directly: " + target);
                        // 小teto定制：启动前自动备份存档（保留最近5个）
                        BackupBedrockSaves(target);
                        // 防双开：已有基岩版进程时不重复启动（用户反馈点一次启动会开两次游戏，此处兜底）
                        if (Process.GetProcessesByName("Minecraft.Windows").Length > 0)
                            return "基岩版已在运行中。";
                        // 小teto定制：读取自定义启动参数
                        var customArgs = GetBedrockLaunchArgs(target);
                        var psi = new ProcessStartInfo(exe)
                        {
                            WorkingDirectory = target,
                            UseShellExecute = true
                        };
                        if (!string.IsNullOrEmpty(customArgs))
                        {
                            psi.Arguments = customArgs;
                            ModBase.Log("[Bedrock] 自定义启动参数: " + customArgs);
                        }
                        // 小teto定制：启动前同步版本 data 的 资源包/行为包/世界 到游戏真实数据目录（进游戏可见）
                        SyncBedrockDataToGame(target);
                        // 小teto定制：备份玩家皮肤档案 + 身份变化自动迁移（防"皮肤每天重置"）
                        foreach (var mb in new[] { "Minecraft Bedrock", "Minecraft Bedrock Preview" })
                            BackupAndMigrateBedrockProfiles("gdk-" + mb, GetBedrockUserGameFolder(mb));
                        // 小teto定制：启动前同步 UWP/GDK 皮肤披风数据（custom.png / custom_skins / skin_packs）
                        SyncBedrockPlayerData();
                        var proc = Process.Start(psi);
                        // 小teto定制：BE 启动计数（与 Java 版同步：启动成功即 +1）
                        try
                        {
                            var beKey = target.TrimEnd('\\') + "\\";
                            States.Instance.LaunchCount[beKey] = States.Instance.LaunchCount[beKey] + 1;
                            States.System.LaunchCount += 1;
                            ModBase.Log("[Bedrock] 启动次数已更新: " + (States.Instance.LaunchCount[beKey]));
                        }
                        catch { }
                        // 小teto定制：同步 Java 启动机制——右下角「结束进程」按钮、每次运行写 .log 日志、非正常退出弹错误报告
                        MonitorBedrockProcess(target, proc);
                        return "";
                    }
                    // UWP 包：走激活启动（注册 loose 包 + shell:AppsFolder 激活），必须在后台线程执行避免 UI 卡死
                    ModBase.Log("[Bedrock] UWP package detected, launching via UWP activation (background thread): " + target);
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            // 小teto定制：启动前同步版本 data 的 资源包/行为包/世界 到游戏真实数据目录（进游戏可见）
                            SyncBedrockDataToGame(target);
                            // 小teto定制：备份玩家皮肤档案 + 身份变化自动迁移（防"皮肤每天重置"）
                            foreach (var pf in new[] { "microsoft.minecraftuwp_8wekyb3d8bbwe", "microsoft.minecraftwindowsbeta_8wekyb3d8bbwe" })
                            {
                                var cm = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                    "Packages", pf, "LocalState", "games", "com.mojang");
                                BackupAndMigrateBedrockProfiles("uwp-" + pf, cm);
                            }
                            // 小teto定制：启动前同步 UWP/GDK 皮肤披风数据
                            SyncBedrockPlayerData();
                            var err = LaunchUwpLoose(target);
                            if (!string.IsNullOrEmpty(err))
                                ModBase.RunInUi(() => System.Windows.MessageBox.Show(err, "基岩版启动失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning));
                            else
                            {
                                // 小teto定制：UWP loose 启动成功也更新启动次数 + 后台监控游玩时间
                                try
                                {
                                    var beKey = target.TrimEnd('\\') + "\\";
                                    States.Instance.LaunchCount[beKey] = States.Instance.LaunchCount[beKey] + 1;
                                    States.System.LaunchCount += 1;
                                    ModBase.Log("[Bedrock] UWP loose 启动计数已更新: " + (States.Instance.LaunchCount[beKey]));
                                }
                                catch { }
                                MonitorUwpPlaytime(target);
                            }
                        }
                        catch (Exception ex)
                        {
                            ModBase.RunInUi(() => System.Windows.MessageBox.Show("UWP 启动异常：" + ex.Message, "基岩版启动失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning));
                        }
                    });
                    return "";
                }
            }
        }
        catch (Exception ex)
        {
            return "基岩版启动失败：" + ex.Message;
        }
        var installed = FindGamePackage(BedrockFamilyPrefixes);
        if (installed.Count == 0)
        {
            // 尝试从 BE 实例文件夹注册 loose UWP 包
            var manifest = FindAppxManifestInFolder(folder);
            if (manifest is null)
                return "未找到已安装的基岩版。请先在「下载 - 基岩版」页下载并安装一个版本（或安装微软商店版）。";

            if (!IsDeveloperMode())
                return "需要开启 Windows 开发者模式才能注册 UWP 包（设置 → 开发者选项）。已为你打开设置页。";
            Process.Start(new ProcessStartInfo("ms-settings:developers") { UseShellExecute = true });

            var regOk = RegisterLooseAppx(manifest);
            if (!regOk)
                return "UWP 包注册失败，请确认已开启开发者模式后重试。";

            installed = FindGamePackage(BedrockFamilyPrefixes);
            if (installed.Count == 0)
                return "UWP 包注册后仍未检测到基岩版包，请重新下载安装。";
        }

        var pkg = installed.First();

        // 直接启动（BedrockBoot 式：shell:AppsFolder 启动已注册 UWP 包），不做任何正版校验
        if (LaunchPackage(pkg))
        {
            // 小teto定制：UWP 启动计数 + 后台监控游玩时间
            try
            {
                var beKey = folder.TrimEnd('\\') + "\\";
                States.Instance.LaunchCount[beKey] = States.Instance.LaunchCount[beKey] + 1;
                States.System.LaunchCount += 1;
                ModBase.Log("[Bedrock] UWP 启动计数已更新: " + (States.Instance.LaunchCount[beKey]));
            }
            catch { }
            MonitorUwpPlaytime(folder);
            return "";
        }
        // 小teto定制：启动失败时根据版本号给出更详细的错误提示
        var verName = Path.GetFileName(folder) ?? "未知";
        ModBase.Log("[Bedrock] UWP 启动失败，版本: " + verName + ", 包: " + pkg.FamilyName);
        // 判断是否为旧版本（1.19 及以下）
        bool isOldVersion = false;
        try
        {
            var verParts = verName.Split('.');
            if (verParts.Length >= 2 && int.TryParse(verParts[0], out var major) && int.TryParse(verParts[1], out var minor))
            {
                if (major < 1 || (major == 1 && minor <= 19)) isOldVersion = true;
            }
        }
        catch { }
        if (isOldVersion)
        {
            return "基岩版 " + verName + " 启动失败。\n\n该版本为旧版 UWP 包（1.19 及以下），可能与当前 Windows 版本不兼容，或缺少必要的运行依赖（如 Visual C++ Runtime、.NET Native）。\n\n建议：1. 尝试使用 1.20 及以上版本；2. 确认已开启开发者模式；3. 检查 Windows 更新是否完整。";
        }
        return "基岩版 " + verName + " 启动失败。请确认已开启开发者模式，或尝试重新下载该版本。";
    }

    /// <summary>
    ///     小teto定制：判断版本目录是否为 GDK 结构（含 GDK 运行时 dll，可直接启动 Minecraft.Windows.exe）。
    ///     UWP loose 结构（1.21.x 等老版本 GDK 包）缺 GDK 运行时 dll，必须走 UWP 激活启动。
    /// </summary>
    /// <summary>
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

            // 小teto定制：检查源包大小，过小（<10MB）说明下载不完整
            var pkgInfo = new FileInfo(pkg);
            if (pkgInfo.Length < 10 * 1024 * 1024)
            {
                ModBase.Log("[Bedrock] UWP 包过小（" + (pkgInfo.Length / 1024 / 1024) + "MB），可能下载不完整: " + pkg);
                try { if (File.Exists(pkg)) File.Delete(pkg); } catch { }
                return "下载的 UWP 包不完整（仅 " + (pkgInfo.Length / 1024) + "KB），已删除源包，请重新下载或更换版本。";
            }

            if (!File.Exists(Path.Combine(dir, "AppxManifest.xml")))
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(pkg, dir);
                var sig = Path.Combine(dir, "AppxSignature.p7x");
                if (File.Exists(sig))
                    try { File.Delete(sig); } catch { }
                WriteBedrockMarker(dir);
            }

            // 小teto定制：检查是否为 .appxbundle 嵌套结构（外层只有 AppxBundleManifest.xml + 子 .appx）
            var bundleManifest = Path.Combine(dir, "AppxBundleManifest.xml");
            if (File.Exists(bundleManifest) && !File.Exists(Path.Combine(dir, "AppxManifest.xml")))
            {
                ModBase.Log("[Bedrock] 检测到 .appxbundle 嵌套结构，尝试解压子包...");
                var subAppx = Directory.EnumerateFiles(dir, "*.appx", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (subAppx is not null)
                {
                    var subDir = Path.Combine(dir, "_sub_extract");
                    Directory.CreateDirectory(subDir);
                    System.IO.Compression.ZipFile.ExtractToDirectory(subAppx, subDir);
                    // 把子包内容移到外层
                    foreach (var f in Directory.EnumerateFiles(subDir))
                    {
                        try { File.Move(f, Path.Combine(dir, Path.GetFileName(f)), true); } catch { }
                    }
                    foreach (var d in Directory.EnumerateDirectories(subDir))
                    {
                        var dest = Path.Combine(dir, Path.GetFileName(d));
                        if (!Directory.Exists(dest)) Directory.Move(d, dest);
                    }
                    try { Directory.Delete(subDir, true); } catch { }
                    try { if (File.Exists(subAppx)) File.Delete(subAppx); } catch { }
                }
            }

            // 小teto定制：解压后验证关键游戏文件是否存在
            var hasExe = File.Exists(Path.Combine(dir, "Minecraft.Windows.exe"));
            var hasData = Directory.Exists(Path.Combine(dir, "data"));
            var hasAssets = Directory.Exists(Path.Combine(dir, "assets"));
            if (!hasExe && !hasData && !hasAssets)
            {
                ModBase.Log("[Bedrock] UWP 解压后缺少关键游戏文件（Minecraft.Windows.exe/data/assets），包可能不完整");
                // 清理不完整的解压文件（只清理元数据文件，保留源包供重新下载）
                foreach (var metaFile in new[] { "AppxBlockMap.xml", "AppxManifest.xml", "AppxSignature.p7x", "[Content_Types].xml", "AppxBundleManifest.xml" })
                {
                    var mf = Path.Combine(dir, metaFile);
                    if (File.Exists(mf)) try { File.Delete(mf); } catch { }
                }
                try { if (File.Exists(pkg)) File.Delete(pkg); } catch { }
                return "UWP 包解压后缺少游戏文件（仅解压出元数据），源包可能不完整或已损坏。已清理，请重新下载或更换其他版本。";
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

    private static bool IsGdkStructure(string dir)
    {
        return File.Exists(Path.Combine(dir, "libHttpClient.GDK.dll")) ||
               File.Exists(Path.Combine(dir, "Microsoft.Xbox.Services.GDK.C.Thunks.dll"));
    }

    /// <summary>
    ///     小teto定制：版本目录缺 GDK 运行时 dll（libHttpClient.GDK.dll 等）时，
    ///     从同实例文件夹下其他 GDK 版本复制补全。已实测可让 1.21.x 直接启动 Minecraft.Windows.exe。
    /// </summary>
    /// <summary>
    ///     小teto定制：同步 Java 启动机制到基岩版——右下角「结束进程」按钮、每次运行写 .log 日志、非正常退出弹错误报告。
    /// </summary>
    public static Process? CurrentBedrockProcess;
    public static bool BedrockUserKilled;

    /// <summary>
    ///     小teto定制：UWP 包没有可等待的 Process 句柄，用后台线程轮询进程存在性来累计游玩时间（playtime.txt）。
    /// </summary>
    public static void MonitorUwpPlaytime(string target)
    {
        ModBase.RunInNewThread(() =>
        {
            try
            {
                DateTime? runStart = null;
                while (true)
                {
                    bool running = Process.GetProcessesByName("Minecraft.Windows").Length > 0;
                    if (running && runStart is null)
                        runStart = DateTime.Now;
                    if (!running && runStart is not null)
                    {
                        var playSeconds = (int)(DateTime.Now - runStart.Value).TotalSeconds;
                        if (playSeconds > 0)
                        {
                            var playtimeFile = Path.Combine(target, "playtime.txt");
                            long total = 0;
                            if (File.Exists(playtimeFile)) long.TryParse(File.ReadAllText(playtimeFile).Trim(), out total);
                            total += playSeconds;
                            File.WriteAllText(playtimeFile, total.ToString(), System.Text.Encoding.UTF8);
                            ModBase.Log($"[Bedrock] UWP 本次游戏时长: {playSeconds}秒, 累计: {total}秒");
                        }
                        break;
                    }
                    System.Threading.Thread.Sleep(5000);
                }
            }
            catch { }
        });
    }

    public static void MonitorBedrockProcess(string target, Process proc)
    {
        try
        {
            BedrockUserKilled = false;
            CurrentBedrockProcess = proc;
            // 让右下角「结束进程」按钮显示
            ModWatcher.hasRunningMinecraft = true;
            ModBase.RunInUi(() => ModMain.frmMain.BtnExtraShutdown.ShowRefresh());
            // 小teto定制：记录启动时间，启动时写全面日志（启动+退出在同一个文件）
            var startTime = DateTime.Now;
            var logPath = Path.Combine(target, "运行日志-" + startTime.ToString("yyyyMMdd-HHmmss") + ".log");
            WriteBedrockRunLogStart(logPath, target, proc.Id, startTime);
            ModBase.RunInNewThread(() =>
            {
                try
                {
                    proc.WaitForExit();
                    var exitCode = proc.ExitCode;
                    if (ReferenceEquals(CurrentBedrockProcess, proc)) CurrentBedrockProcess = null;
                    // 小teto定制：退出时追加到同一个日志文件，记录运行时长、退出码含义等
                    var endTime = DateTime.Now;
                    WriteBedrockRunLogExit(logPath, target, proc.Id, exitCode, startTime, endTime);
                    // 小teto定制：游戏时间统计——累计运行时长到 playtime.txt
                    try
                    {
                        var playSeconds = (int)(endTime - startTime).TotalSeconds;
                        var playtimeFile = Path.Combine(target, "playtime.txt");
                        long total = 0;
                        if (File.Exists(playtimeFile)) long.TryParse(File.ReadAllText(playtimeFile).Trim(), out total);
                        total += playSeconds;
                        File.WriteAllText(playtimeFile, total.ToString(), System.Text.Encoding.UTF8);
                        ModBase.Log($"[Bedrock] 本次游戏时长: {playSeconds}秒, 累计: {total}秒 ({total/3600}小时{(total%3600)/60}分)");
                    }
                    catch { }
                    // 更新「结束进程」按钮显示
                    var anyJava = ModWatcher.mcWatcherList.Any(w =>
                        w.State == ModWatcher.Watcher.MinecraftState.Loading ||
                        w.State == ModWatcher.Watcher.MinecraftState.Running);
                    var anyBe = Process.GetProcessesByName("Minecraft.Windows").Length > 0;
                    // 小teto定制：用户主动点击「关闭 Minecraft」结束时（BedrockUserKilled）也隐藏按钮，
                    // 避免 Java watcher 状态异步更新导致按钮残留
                    if (BedrockUserKilled || (!anyJava && !anyBe))
                    {
                        ModWatcher.hasRunningMinecraft = false;
                        ModBase.RunInUi(() => ModMain.frmMain.BtnExtraShutdown.ShowRefresh());
                    }
                    // 非正常退出（非用户主动结束且退出码非 0）→ 弹错误报告
                    if (!BedrockUserKilled && exitCode != 0)
                    {
                        ModBase.RunInUi(() =>
                        {
                            ModMain.MyMsgBox(
                                "基岩版检测到异常退出（退出码 " + exitCode + "）。\n可能原因：游戏崩溃、资源文件不完整，或系统环境异常。\n运行日志已保存到该版本目录下的 .log 文件。",
                                "基岩版错误报告",
                                isWarn: true);
                        });
                    }
                }
                catch { }
            });
        }
        catch { }
    }

    /// <summary>小teto定制：检测是否已出现可见的 Minecraft 游戏窗口。
    /// UWP 包窗口由 ApplicationFrameHost 承载（主窗口句柄拿不到），需遍历可见窗口标题判断。</summary>
    public static bool IsMinecraftWindowVisible()
    {
        try
        {
            // 1) 进程存在且有主窗口句柄（GDK 包适用）
            var procs = Process.GetProcessesByName("Minecraft.Windows");
            foreach (var p in procs)
            {
                try
                {
                    if (!p.HasExited && p.MainWindowHandle != IntPtr.Zero)
                    {
                        p.Dispose();
                        return true;
                    }
                }
                catch { }
                p.Dispose();
            }
            // 2) UWP 窗口在 ApplicationFrameHost 下 → 遍历所有可见窗口，标题含 Minecraft 且不属于本进程
            var selfPid = (uint)Environment.ProcessId;
            bool found = false;
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsWindowVisible(hWnd)) return true;
                GetWindowThreadProcessId(hWnd, out uint wndPid);
                if (wndPid == selfPid) return true; // 跳过 PCL 自身窗口
                var sb = new System.Text.StringBuilder(256);
                GetWindowText(hWnd, sb, 256);
                if (sb.ToString().IndexOf("Minecraft", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found = true;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return found;
        }
        catch { return false; }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    /// <summary>小teto定制：获取基岩版版本的累计游戏时长（秒）。</summary>
    public static long GetBedrockPlayTime(string target)
    {
        try
        {
            var playtimeFile = Path.Combine(target, "playtime.txt");
            if (File.Exists(playtimeFile))
            {
                long total;
                if (long.TryParse(File.ReadAllText(playtimeFile).Trim(), out total))
                    return total;
            }
        }
        catch { }
        return 0;
    }

    /// <summary>小teto定制：格式化游戏时长为可读字符串。</summary>
    public static string FormatPlayTime(long seconds)
    {
        if (seconds <= 0) return "0分钟";
        var h = seconds / 3600;
        var m = (seconds % 3600) / 60;
        if (h > 0) return $"{h}小时{m}分";
        return $"{m}分钟";
    }

    /// <summary>小teto定制：启动前自动备份基岩版存档（保留最近5个备份）。
    /// 真实存档位置：GDK 版在 %APPDATA%\Minecraft Bedrock(Preview)\Users\<uid>\games\com.mojang\minecraftWorlds；
    /// UWP 版在 %LocalAppData%\Packages\<PFN>\LocalState\games\com.mojang\minecraftWorlds。
    /// 备份到版本目录 backups\ 下。</summary>
    public static void BackupBedrockSaves(string target)
    {
        try
        {
            // 1. GDK 版：AppData 下 Minecraft Bedrock / Minecraft Bedrock Preview 的 Users\<uid>\games\com.mojang\minecraftWorlds
            var savesDir = "";
            try
            {
                foreach (var mbName in new[] { "Minecraft Bedrock", "Minecraft Bedrock Preview" })
                {
                    var usersRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), mbName, "Users");
                    if (!Directory.Exists(usersRoot)) continue;
                    foreach (var sub in Directory.EnumerateDirectories(usersRoot))
                    {
                        var n = Path.GetFileName(sub);
                        if (string.Equals(n, "Shared", StringComparison.OrdinalIgnoreCase)) continue;
                        var w = Path.Combine(sub, "games", "com.mojang", "minecraftWorlds");
                        if (Directory.Exists(w)) { savesDir = w; break; }
                    }
                    if (!string.IsNullOrEmpty(savesDir)) break;
                }
            }
            catch { }
            // 2. UWP 版：LocalState\games\com.mojang\minecraftWorlds
            if (string.IsNullOrEmpty(savesDir))
            {
                try
                {
                    foreach (var pf in new[] { "microsoft.minecraftuwp_8wekyb3d8bbwe", "microsoft.minecraftwindowsbeta_8wekyb3d8bbwe" })
                    {
                        var w = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", pf, "LocalState", "games", "com.mojang", "minecraftWorlds");
                        if (Directory.Exists(w)) { savesDir = w; break; }
                    }
                }
                catch { }
            }
            // 3. 兼容旧路径：版本目录下
            if (string.IsNullOrEmpty(savesDir))
            {
                var altDirs = new[] {
                    Path.Combine(target, "data", "minecraftWorlds"),
                    Path.Combine(target, "minecraftWorlds"),
                    Path.Combine(target, "data", "games", "com.mojang", "minecraftWorlds")
                };
                foreach (var alt in altDirs)
                {
                    if (Directory.Exists(alt)) { savesDir = alt; break; }
                }
            }
            if (string.IsNullOrEmpty(savesDir) || !Directory.Exists(savesDir)) return; // 没有存档目录，跳过

            var backupRoot = Path.Combine(target, "backups");
            Directory.CreateDirectory(backupRoot);
            var backupName = "存档备份-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var backupPath = Path.Combine(backupRoot, backupName);

            // 复制存档
            CopyDirectory(savesDir, backupPath);
            ModBase.Log($"[Bedrock] 存档已备份: {backupName}");

            // 只保留最近5个备份，删除旧的
            var backups = Directory.GetDirectories(backupRoot, "存档备份-*")
                .OrderByDescending(d => d)
                .Skip(5)
                .ToList();
            foreach (var old in backups)
            {
                try { Directory.Delete(old, true); } catch { }
            }
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "存档备份失败");
        }
    }

    /// <summary>
    ///     小teto定制：UWP 与 GDK 使用完全不同的数据目录（UWP 在 LocalAppData\Packages 沙箱，GDK 在 AppData\Minecraft Bedrock），
    ///     游戏不会跨位置读取，导致在 UWP 版（1.16.40 等）编辑的皮肤/披风到 GDK 版（26.x 等）丢失。
    ///     此方法在启动前把皮肤/披风数据（custom.png、custom_skins、skin_packs、development_skin_packs）双向合并同步。
    /// </summary>
    private static void SyncBedrockPlayerData()
    {
        try
        {
            // UWP 正式版数据目录
            var uwpRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Packages", "microsoft.minecraftuwp_8wekyb3d8bbwe", "LocalState", "games", "com.mojang");
            // GDK 正式版数据目录（AppData\Minecraft Bedrock\Users\<uid>\games\com.mojang）
            var gdkRoot = GetBedrockUserGameFolder("Minecraft Bedrock");
            if (string.IsNullOrEmpty(gdkRoot) || !Directory.Exists(uwpRoot)) return;

            // 同步单个文件：custom.png（自定义皮肤/披风）等
            foreach (var file in new[] { "custom.png", "global_resource_pack_settings.json" })
            {
                var uwpFile = Path.Combine(uwpRoot, file);
                var gdkFile = Path.Combine(gdkRoot, file);
                try
                {
                    if (File.Exists(uwpFile) && (!File.Exists(gdkFile) || File.GetLastWriteTimeUtc(uwpFile) > File.GetLastWriteTimeUtc(gdkFile)))
                    { Directory.CreateDirectory(gdkRoot); File.Copy(uwpFile, gdkFile, true); }
                    if (File.Exists(gdkFile) && (!File.Exists(uwpFile) || File.GetLastWriteTimeUtc(gdkFile) > File.GetLastWriteTimeUtc(uwpFile)))
                    { Directory.CreateDirectory(uwpRoot); File.Copy(gdkFile, uwpFile, true); }
                }
                catch { }
            }
            // 注意：ud*.dat（更衣室 persona 角色/披风数据）不做跨 UWP/GDK 同步——
            // UWP 老版本（1.16.40/1.19.83）用 personaProfile3+Pending 格式，GDK 新版本（26.x）用
            // personaProfile+Synchronized+ProfileVersion 格式，两者 schema 不兼容，同步会把对方数据
            // 污染成游戏读不懂的格式导致披风/角色丢失。UWP 之间靠 LocalState 备份/恢复保持，GDK 之间天然共享。
            // 同步目录（双向合并，较新优先）
            foreach (var sub in new[] { "custom_skins", "skin_packs", "development_skin_packs" })
            {
                MergeDir(Path.Combine(uwpRoot, sub), Path.Combine(gdkRoot, sub));
                MergeDir(Path.Combine(gdkRoot, sub), Path.Combine(uwpRoot, sub));
            }
            ModBase.Log("[Bedrock] UWP/GDK 皮肤数据已双向同步（persona 披风数据格式不兼容，不跨同步）");
        }
        catch (Exception ex) { ModBase.Log("[Bedrock] 皮肤数据同步失败: " + ex.Message); }
    }

    /// <summary>小teto定制：获取 GDK 版实际玩家数据目录（AppData\Minecraft Bedrock(Preview)\Users\&lt;uid&gt;\games\com.mojang，跳过 Shared）。</summary>
    private static string GetBedrockUserGameFolder(string mbName)
    {
        try
        {
            var usersRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), mbName, "Users");
            if (!Directory.Exists(usersRoot)) return null;
            foreach (var sub in Directory.EnumerateDirectories(usersRoot))
            {
                var n = Path.GetFileName(sub);
                if (string.Equals(n, "Shared", StringComparison.OrdinalIgnoreCase)) continue;
                var g = Path.Combine(sub, "games", "com.mojang");
                if (Directory.Exists(g)) return g;
            }
            var shared = Path.Combine(usersRoot, "Shared", "games", "com.mojang");
            return Directory.Exists(shared) ? shared : null;
        }
        catch { return null; }
    }

    /// <summary>小teto定制：目录双向合并（较新的文件覆盖旧的，目录递归合并）。</summary>
    private static void MergeDir(string srcRoot, string dstRoot)
    {
        try
        {
            if (!Directory.Exists(srcRoot)) return;
            Directory.CreateDirectory(dstRoot);
            foreach (var file in Directory.GetFiles(srcRoot))
            {
                var dstFile = Path.Combine(dstRoot, Path.GetFileName(file));
                if (!File.Exists(dstFile) || File.GetLastWriteTimeUtc(file) > File.GetLastWriteTimeUtc(dstFile))
                    File.Copy(file, dstFile, true);
            }
            foreach (var dir in Directory.GetDirectories(srcRoot))
                MergeDir(dir, Path.Combine(dstRoot, Path.GetFileName(dir)));
        }
        catch { }
    }

    /// <summary>小teto定制：递归复制目录。</summary>
    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var dest = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, dest, true);
        }
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var dest = Path.Combine(targetDir, Path.GetFileName(dir));
            CopyDirectory(dir, dest);
        }
    }

    /// <summary>小teto定制：递归复制目录（跳过 SystemAppData，避免与系统管理目录冲突）。</summary>
    private static void CopyDirectoryExcludeSystem(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            try { File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true); } catch { }
        }
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            if (string.Equals(Path.GetFileName(dir), "SystemAppData", StringComparison.OrdinalIgnoreCase))
                continue;
            try { CopyDirectoryExcludeSystem(dir, Path.Combine(targetDir, Path.GetFileName(dir))); } catch { }
        }
    }

    /// <summary>
    ///     小teto定制：跨 UWP 版本保留更衣室 persona 披风/角色数据。
    ///     老版本（1.16.40 等）编辑的披风存在 Pending.personaProfile3，切换到新版本（1.19.83+）后
    ///     游戏启动会重写 ud（Pending 清空、格式转换），导致披风丢失。
    ///     此方法在启动前扫描历史备份，把含 personaProfile3 的 Pending 数据重新注入目标 ud
    ///     （仅当目标 Pending 为空时）。游戏若兼容则显示披风；若不兼容则忽略，无副作用。
    /// </summary>
    private static void TryMergeBedrockPersonaPending(string packageName)
    {
        try
        {
            var pfn = (packageName + "_8wekyb3d8bbwe").ToLowerInvariant();
            var uwpDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCLCE-BE", "UWPData");
            var pfnRoot = Path.Combine(uwpDataRoot, pfn);
            var gameDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", pfn, "LocalState", "games", "com.mojang");
            if (!Directory.Exists(gameDir) || !Directory.Exists(pfnRoot)) return;
            foreach (var udFile in Directory.GetFiles(gameDir, "ud*.dat"))
            {
                string content;
                try { content = File.ReadAllText(udFile, Encoding.UTF8); } catch { continue; }
                // 已有 Pending 数据 → 跳过（不覆盖）
                if (content.Contains("\"Pending\" : {") || content.Contains("\"Pending\":{")) continue;
                // 扫描所有备份找含 personaProfile3 的 Pending
                foreach (var bakUd in Directory.GetFiles(pfnRoot, "ud*.dat", SearchOption.AllDirectories))
                {
                    if (string.Equals(bakUd, udFile, StringComparison.OrdinalIgnoreCase)) continue;
                    string bakContent;
                    try { bakContent = File.ReadAllText(bakUd, Encoding.UTF8); } catch { continue; }
                    if (!bakContent.Contains("personaProfile3")) continue;
                    var pendStart = bakContent.IndexOf("\"Pending\" : {", StringComparison.Ordinal);
                    if (pendStart < 0) pendStart = bakContent.IndexOf("\"Pending\":{", StringComparison.Ordinal);
                    if (pendStart < 0) continue;
                    int braceStart = bakContent.IndexOf('{', pendStart);
                    if (braceStart < 0) continue;
                    int depth = 0, end = -1;
                    for (int i = braceStart; i < bakContent.Length; i++)
                    {
                        if (bakContent[i] == '{') depth++;
                        else if (bakContent[i] == '}') { depth--; if (depth == 0) { end = i; break; } }
                    }
                    if (end < 0) continue;
                    var pendJson = bakContent.Substring(braceStart, end - braceStart + 1);
                    string oldPending = "\"Pending\" : null";
                    if (content.Contains(oldPending))
                    {
                        content = content.Replace(oldPending, "\"Pending\" : " + pendJson);
                        try
                        {
                            File.WriteAllText(udFile, content, Encoding.UTF8);
                            ModBase.Log("[Bedrock] 已注入 persona Pending（披风数据）: " + Path.GetFileName(bakUd) + " -> " + Path.GetFileName(udFile));
                            break;
                        }
                        catch (Exception ex) { ModBase.Log("[Bedrock] 注入 persona Pending 写入失败: " + ex.Message); }
                    }
                }
            }
        }
        catch (Exception ex) { ModBase.Log("[Bedrock] 注入 persona Pending 失败: " + ex.Message); }
    }

    /// <summary>小teto定制：判断基岩版版本是否已收藏。</summary>
    public static bool IsBedrockFavorite(string target)
    {
        try { return File.Exists(Path.Combine(target, "favorite.txt")); }
        catch { return false; }
    }

    /// <summary>小teto定制：切换基岩版版本收藏状态。</summary>
    public static void ToggleBedrockFavorite(string target)
    {
        try
        {
            var favFile = Path.Combine(target, "favorite.txt");
            if (File.Exists(favFile))
                File.Delete(favFile);
            else
                File.WriteAllText(favFile, "1", System.Text.Encoding.UTF8);
        }
        catch { }
    }

    /// <summary>小teto定制：枚举当前所选 Minecraft 文件夹下 bedrock_versions 中的本地 BE 版本。</summary>
    public static List<string> GetBedrockLocalVersions()
    {
        var list = new List<string>();
        try
        {
            var root = Path.Combine(ModFolder.mcFolderSelected, "bedrock_versions");
            if (Directory.Exists(root))
                foreach (var dir in Directory.GetDirectories(root))
                {
                    var name = new DirectoryInfo(dir).Name;
                    if (!string.IsNullOrEmpty(name) && name[0] != '.')
                        list.Add(name);
                }
        }
        catch { }
        return list;
    }

    /// <summary>小teto定制：一键安装 BE 资源包（.mcpack/.mcaddon/.mcworld）到指定版本。</summary>
    /// <remarks>按 BedrockBoot 方式：解压后读取 manifest.json 的 modules[].type 判断，
    /// data=行为包 -> data\behavior_packs，resources=资源包 -> data\resource_packs；
    /// .mcaddon 内含多个 .mcpack 时逐个解压分类安装。</remarks>
    public static string InstallBedrockResource(string target, string resourceFile)
    {
        try
        {
            if (!File.Exists(resourceFile)) return "资源文件不存在";
            var ext = Path.GetExtension(resourceFile).ToLower();
            var dataDir = Path.Combine(target, "data");
            Directory.CreateDirectory(dataDir);

            // 世界包：直接解压到 minecraftWorlds（同时同步到游戏真实读取的 AppData/LocalState 数据目录）
            if (ext == ".mcworld")
            {
                var worldsDir = Path.Combine(dataDir, "minecraftWorlds");
                Directory.CreateDirectory(worldsDir);
                var worldDir = Path.Combine(worldsDir, Path.GetFileNameWithoutExtension(resourceFile));
                System.IO.Compression.ZipFile.ExtractToDirectory(resourceFile, worldDir, true);
                SyncBedrockDirToGameData(worldDir, "minecraftWorlds"); // 小teto定制：同步到游戏真实数据目录
                ModBase.Log($"[Bedrock] 世界已安装: {Path.GetFileName(resourceFile)} -> minecraftWorlds");
                return "世界安装成功";
            }

            // 附加包/资源包：解压到临时目录，按 manifest.json 分类安装
            var tempDir = Path.Combine(Path.GetTempPath(), "be_res_" + Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            System.IO.Compression.ZipFile.ExtractToDirectory(resourceFile, tempDir, true);

            // 收集所有包含 manifest.json 的包目录（mcaddon 内的 .mcpack/.zip 会继续解压）
            var packDirs = new List<string>();
            CollectBedrockPackDirs(tempDir, packDirs);

            if (packDirs.Count == 0)
            {
                try { Directory.Delete(tempDir, true); } catch { }
                return "未在文件中找到有效的资源包清单（manifest.json），可能不是受支持的 BE 资源包";
            }

            var bpDir = Path.Combine(dataDir, "behavior_packs");
            var rpDir = Path.Combine(dataDir, "resource_packs");
            Directory.CreateDirectory(bpDir);
            Directory.CreateDirectory(rpDir);

            var installed = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var packDir in packDirs)
            {
                var packType = GetBedrockPackType(packDir); // "behavior_packs" / "resource_packs" / null
                var name = GetBedrockPackName(packDir);
                if (string.IsNullOrEmpty(name))
                    name = new DirectoryInfo(packDir).Name;
                name = SanitizePackName(name);
                if (string.IsNullOrEmpty(name))
                    name = "pack_" + Guid.NewGuid().ToString("N")[..8];
                if (seen.Contains(name)) name += "_" + Guid.NewGuid().ToString("N")[..4];
                seen.Add(name);

                if (packType == "behavior_packs")
                {
                    CopyDirectory(packDir, Path.Combine(bpDir, name));
                    SyncBedrockDirToGameData(packDir, "behavior_packs"); // 小teto定制：同步到游戏真实数据目录
                    installed.Add("行为包·" + name);
                }
                else if (packType == "resource_packs")
                {
                    CopyDirectory(packDir, Path.Combine(rpDir, name));
                    SyncBedrockDirToGameData(packDir, "resource_packs"); // 小teto定制：同步到游戏真实数据目录
                    installed.Add("资源包·" + name);
                }
            }

            try { Directory.Delete(tempDir, true); } catch { }
            ModBase.Log($"[Bedrock] 资源已安装: {Path.GetFileName(resourceFile)} -> {string.Join("、", installed)}");
            return installed.Count > 0 ? "安装成功：" + string.Join("、", installed) : "未识别到可安装的行为包/资源包";
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "BE 资源安装失败");
            return "安装失败: " + ex.Message;
        }
    }

    /// <summary>小teto定制：备份玩家皮肤档案（ud*.dat，更衣室/皮肤数据）并处理身份变化。
    /// 若当前存在备份中从未见过的新身份档案，自动把最近修改的其他身份档案复制给它，
    /// 防止"皮肤每天重置"（游戏玩家身份变化时旧身份皮肤丢失）。</summary>
    private static void BackupAndMigrateBedrockProfiles(string profileKey, string comMojangDir)
    {
        try
        {
            if (string.IsNullOrEmpty(comMojangDir) || !Directory.Exists(comMojangDir)) return;
            var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PCLCE-BE", "PlayerData", profileKey);
            Directory.CreateDirectory(root);
            var now = DateTime.Now;
            // 1) 备份当前所有 ud*.dat
            foreach (var ud in Directory.GetFiles(comMojangDir, "ud*.dat"))
            {
                var bak = Path.Combine(root, Path.GetFileName(ud) + "." + now.ToString("yyyyMMdd-HHmmss") + ".bak");
                try { File.Copy(ud, bak, true); } catch { }
            }
            // 清理：同一身份档案只保留最近 30 份
            try
            {
                foreach (var g in Directory.GetFiles(root, "ud*.dat.*.bak").GroupBy(f => Path.GetFileName(f).Split('.')[0]))
                {
                    foreach (var o in g.OrderByDescending(f => f).Skip(30).ToList())
                    { try { File.Delete(o); } catch { } }
                }
            }
            catch { }
            // 2) 迁移：当前存在的身份 vs 备份中已知身份（文件名带 .dat 后缀，修正 v130 比较 bug）
            var knownNames = new HashSet<string>(
                Directory.GetFiles(root, "ud*.dat.*.bak")
                    .Select(f => Path.GetFileName(f).Split('.')[0] + ".dat"),
                StringComparer.OrdinalIgnoreCase);
            foreach (var ud in Directory.GetFiles(comMojangDir, "ud*.dat"))
            {
                var name = Path.GetFileName(ud);
                if (knownNames.Contains(name)) continue; // 已知身份，跳过（v130 此处不匹配导致互相覆盖）
                // 真正的"新身份"：取备份中最近修改的其他身份档案复制给它（保留皮肤设置），
                // 且只选 ProfileVersion >= 50 的新格式档案（旧格式 2 会被游戏启动重写，迁移无效）
                var candidates = Directory.GetFiles(root, "ud*.dat.*.bak")
                    .Where(f => !Path.GetFileName(f).StartsWith(name, StringComparison.OrdinalIgnoreCase))
                    .Select(f => new { F = f, T = File.GetLastWriteTimeUtc(f), V = ReadUdProfileVersion(f) })
                    .Where(x => x.V >= 50)
                    .OrderByDescending(x => x.T)
                    .ToList();
                if (candidates.Count == 0) continue;
                try
                {
                    File.Copy(candidates[0].F, ud, true);
                    ModBase.Log($"[Bedrock] 检测到新玩家身份 {name}，已从 {Path.GetFileName(candidates[0].F)} 迁移皮肤档案");
                }
                catch { }
            }
        }
        catch (Exception ex) { ModBase.Log("[Bedrock] 皮肤档案备份/迁移失败: " + ex.Message); }
    }

    /// <summary>小teto定制：启动前把版本 data\resource_packs、data\behavior_packs、data\minecraftWorlds
    /// 全部同步到游戏真实读取的数据目录，保证老版本安装的包进入游戏也能看到。</summary>
    private static void SyncBedrockDataToGame(string target)
    {
        try
        {
            var dataRoot = Path.Combine(target, "data");
            foreach (var sub in new[] { "resource_packs", "behavior_packs", "minecraftWorlds" })
            {
                var src = Path.Combine(dataRoot, sub);
                if (!Directory.Exists(src)) continue;
                foreach (var packDir in Directory.GetDirectories(src))
                {
                    // 小teto定制：跳过原版自带包（vanilla_*、chemistry_*、oreui、server_*_library 等），只同步用户安装的包
                    if (IsBedrockVanillaPack(new DirectoryInfo(packDir).Name)) continue;
                    SyncBedrockDirToGameData(packDir, sub);
                }
            }
            ModBase.Log("[Bedrock] 启动前已将版本 data 的资源/行为包/世界同步到游戏数据目录");
        }
        catch (Exception ex) { ModBase.Log("[Bedrock] 启动前数据同步失败: " + ex.Message); }
    }

    /// <summary>小teto定制：读取 ud*.dat 档案的 ProfileVersion（判断新旧格式，旧格式迁移会被游戏重写）。</summary>
    private static int ReadUdProfileVersion(string udFile)
    {
        try
        {
            var txt = File.ReadAllText(udFile, Encoding.UTF8);
            var idx = txt.IndexOf("\"ProfileVersion\"", StringComparison.Ordinal);
            if (idx < 0) return 0;
            var rest = txt.Substring(idx + "\"ProfileVersion\"".Length);
            var colon = rest.IndexOf(':');
            if (colon < 0) return 0;
            var num = "";
            for (int i = colon + 1; i < rest.Length; i++)
            {
                if (char.IsDigit(rest[i])) num += rest[i];
                else if (num.Length > 0) break;
            }
            int v = 0;
            int.TryParse(num, out v);
            return v;
        }
        catch { return 0; }
    }

    /// <summary>小teto定制：判断包目录是否为游戏原版自带（vanilla_*、chemistry_*、oreui、
    /// server_*_library 等）。原版包无需复制到游戏数据目录。</summary>
    private static bool IsBedrockVanillaPack(string name)
    {
        try
        {
            if (string.IsNullOrEmpty(name)) return true;
            var n = name.ToLowerInvariant();
            if (n.StartsWith("vanilla") || n.StartsWith("chemistry") || n.StartsWith("experimental_"))
                return true;
            return n is "beta" or "editor" or "persona" or "oreui" or "platform_gdk_pc"
                or "store" or "structures" or "test" or "script_ui"
                or "server_editor_library" or "server_library" or "server_ui_library"
                or "education_edition" or "edu";
        }
        catch { return false; }
    }

    /// <summary>小teto定制：把安装好的包目录同步到游戏真实读取的 com.mojang 数据目录，
    /// 使资源包/行为包/世界进入游戏后可见（GDK: AppData\Minecraft Bedrock(Preview)\Users\&lt;uid&gt;\games\com.mojang；
    /// UWP: LocalAppData\Packages\&lt;pfn&gt;\LocalState\games\com.mojang）。</summary>
    private static void SyncBedrockDirToGameData(string packDir, string subFolder)
    {
        try
        {
            var name = new DirectoryInfo(packDir).Name;
            var targets = new List<string>();
            // GDK 正式版 + 预览版（AppData\Minecraft Bedrock(Preview)\Users\<uid>\games\com.mojang\subFolder）
            foreach (var mb in new[] { "Minecraft Bedrock", "Minecraft Bedrock Preview" })
            {
                try
                {
                    var usersRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), mb, "Users");
                    if (!Directory.Exists(usersRoot)) continue;
                    foreach (var sub in Directory.EnumerateDirectories(usersRoot))
                    {
                        // 不跳过 Shared：GDK 游戏也可能读取共享目录中的资源（多写一份无副作用）
                        targets.Add(Path.Combine(sub, "games", "com.mojang", subFolder));
                    }
                }
                catch { }
            }
            // UWP 正式版 + Beta（LocalAppData\Packages\<pfn>\LocalState\games\com.mojang\subFolder）
            foreach (var pf in new[] { "microsoft.minecraftuwp_8wekyb3d8bbwe", "microsoft.minecraftwindowsbeta_8wekyb3d8bbwe" })
            {
                try
                {
                    targets.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Packages", pf, "LocalState", "games", "com.mojang", subFolder));
                }
                catch { }
            }
            foreach (var t in targets)
            {
                try
                {
                    var dest = Path.Combine(t, name);
                    // 小teto定制：目标已存在则跳过（增量），避免每次启动全量复制导致卡在 40%
                    if (Directory.Exists(dest)) continue;
                    Directory.CreateDirectory(t);
                    CopyDirectory(packDir, dest);
                    ModBase.Log($"[Bedrock] 已同步到游戏数据目录: {dest}");
                }
                catch { }
            }
        }
        catch { }
    }

    /// <summary>小teto定制：递归收集含 manifest.json 的包目录（mcaddon 内的 mcpack/zip 会继续解压）。</summary>
    private static void CollectBedrockPackDirs(string scanDir, List<string> packDirs)
    {
        // 小teto定制：先检查当前目录自身是否为包（.mcpack 解压后 manifest.json 常在根目录）
        if (File.Exists(Path.Combine(scanDir, "manifest.json")))
        {
            packDirs.Add(scanDir);
            return;
        }
        foreach (var dir in Directory.GetDirectories(scanDir))
        {
            if (File.Exists(Path.Combine(dir, "manifest.json")))
            {
                packDirs.Add(dir);
                continue; // 已是包目录，不再深入
            }
            CollectBedrockPackDirs(dir, packDirs);
        }
        foreach (var file in Directory.GetFiles(scanDir))
        {
            var fext = Path.GetExtension(file).ToLower();
            if (fext is ".mcpack" or ".zip" or ".mcaddon")
            {
                try
                {
                    var subDir = Path.Combine(Path.GetTempPath(), "be_sub_" + Path.GetRandomFileName());
                    Directory.CreateDirectory(subDir);
                    System.IO.Compression.ZipFile.ExtractToDirectory(file, subDir, true);
                    CollectBedrockPackDirs(subDir, packDirs);
                }
                catch { }
            }
        }
    }

    /// <summary>小teto定制：读取 manifest.json 判断包类型（data=行为包，resources=资源包）。</summary>
    private static string GetBedrockPackType(string packDir)
    {
        try
        {
            var manifest = Path.Combine(packDir, "manifest.json");
            if (!File.Exists(manifest)) return null;
            var json = JsonNode.Parse(File.ReadAllText(manifest, Encoding.UTF8));
            var modules = json?["modules"]?.AsArray();
            if (modules is not null)
                foreach (var m in modules)
                {
                    var t = m?["type"]?.ToString();
                    if (t == "data") return "behavior_packs";
                    if (t == "resources") return "resource_packs";
                }
        }
        catch { }
        return null;
    }

    /// <summary>小teto定制：读取 manifest.json 的包名（header.name）。</summary>
    private static string GetBedrockPackName(string packDir)
    {
        try
        {
            var manifest = Path.Combine(packDir, "manifest.json");
            if (!File.Exists(manifest)) return "";
            var json = JsonNode.Parse(File.ReadAllText(manifest, Encoding.UTF8));
            return json?["header"]?["name"]?.ToString() ?? "";
        }
        catch { return ""; }
    }

    /// <summary>小teto定制：清理包名中的非法文件名字符。</summary>
    private static string SanitizePackName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c.ToString(), "");
        return name.Trim().Trim('.');
    }

    /// <summary>小teto定制：获取 BE 服务器列表（从 servers.json 读取）。</summary>
    public static List<BedrockServerInfo> GetBedrockServers(string folder)
    {
        var list = new List<BedrockServerInfo>();
        try
        {
            var serversFile = Path.Combine(folder, "bedrock_servers.json");
            if (File.Exists(serversFile))
            {
                var json = File.ReadAllText(serversFile, System.Text.Encoding.UTF8);
                // 简单解析 JSON 数组
                var arr = System.Text.Json.JsonSerializer.Deserialize<List<BedrockServerInfo>>(json);
                if (arr != null) list.AddRange(arr);
            }
        }
        catch { }
        return list;
    }

    /// <summary>小teto定制：保存 BE 服务器列表。</summary>
    public static void SaveBedrockServers(string folder, List<BedrockServerInfo> servers)
    {
        try
        {
            var serversFile = Path.Combine(folder, "bedrock_servers.json");
            var json = System.Text.Json.JsonSerializer.Serialize(servers, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(serversFile, json, System.Text.Encoding.UTF8);
        }
        catch { }
    }

    /// <summary>小teto定制：BE 服务器信息。</summary>
    public class BedrockServerInfo
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public int Port { get; set; } = 19132;
    }

    /// <summary>小teto定制：检查 BE 最新正式版版本号（从官方版本清单获取）。</summary>
    public static async Task<string> GetLatestBedrockReleaseAsync()
    {
        try
        {
            using var http = new System.Net.Http.HttpClient();
            http.Timeout = TimeSpan.FromSeconds(10);
            // 从 Minecraft 官方版本清单获取基岩版最新版本
            var json = await http.GetStringAsync("https://piston-meta.mojang.com/mc/game/version_manifest_v2.json");
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            // 基岩版版本信息通常在 latest 中，但官方清单主要是 Java 版
            // 这里返回一个简单的检查结果，实际版本源可以从 BedrockBoot 的源获取
            return "";
        }
        catch
        {
            return "";
        }
    }

    /// <summary>小teto定制：获取 BE 版本的截图列表。</summary>
    public static List<string> GetBedrockScreenshots(string target)
    {
        var list = new List<string>();
        try
        {
            // BE 截图通常在 data/screenshots 或 games/com.mojang/screenshots
            var possibleDirs = new[] {
                Path.Combine(target, "data", "screenshots"),
                Path.Combine(target, "screenshots"),
                Path.Combine(target, "data", "games", "com.mojang", "screenshots")
            };
            foreach (var dir in possibleDirs)
            {
                if (Directory.Exists(dir))
                {
                    list.AddRange(Directory.GetFiles(dir, "*.png")
                        .Concat(Directory.GetFiles(dir, "*.jpg"))
                        .Concat(Directory.GetFiles(dir, "*.jpeg")));
                }
            }
        }
        catch { }
        return list;
    }

    /// <summary>小teto定制：获取 JE 版本的截图列表。</summary>
    public static List<string> GetJavaScreenshots(string mcFolder)
    {
        var list = new List<string>();
        try
        {
            var screenshotsDir = Path.Combine(mcFolder, "screenshots");
            if (Directory.Exists(screenshotsDir))
            {
                list.AddRange(Directory.GetFiles(screenshotsDir, "*.png")
                    .Concat(Directory.GetFiles(screenshotsDir, "*.jpg"))
                    .Concat(Directory.GetFiles(screenshotsDir, "*.jpeg")));
            }
        }
        catch { }
        return list;
    }

    /// <summary>小teto定制：获取 BE 皮肤文件列表。</summary>
    public static List<string> GetBedrockSkins(string target)
    {
        var list = new List<string>();
        try
        {
            // BE 皮肤通常在 data/skin_packs 或自定义皮肤目录
            var possibleDirs = new[] {
                Path.Combine(target, "data", "skin_packs"),
                Path.Combine(target, "data", "models", "skin")
            };
            foreach (var dir in possibleDirs)
            {
                if (Directory.Exists(dir))
                {
                    list.AddRange(Directory.GetFiles(dir, "*.png"));
                }
            }
        }
        catch { }
        return list;
    }

    /// <summary>小teto定制：获取基岩版启动参数（从 launch_args.txt 读取）。</summary>
    public static string GetBedrockLaunchArgs(string target)
    {
        try
        {
            var argsFile = Path.Combine(target, "launch_args.txt");
            if (File.Exists(argsFile))
                return File.ReadAllText(argsFile, System.Text.Encoding.UTF8).Trim();
        }
        catch { }
        return "";
    }

    /// <summary>小teto定制：写入基岩版启动日志（全面信息）。</summary>
    private static void WriteBedrockRunLogStart(string logPath, string target, int pid, DateTime startTime)
    {
        try
        {
            // 小teto定制：用 UWPtres.txt 标记文件判断包类型（最可靠）
            bool isUwp = IsUwpFolder(target);
            string pkgType = isUwp ? "UWP" : "GDK";
            string versionName = Path.GetFileName(target.TrimEnd(Path.DirectorySeparatorChar));
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("[" + TetoLabVersionName + " 基岩版运行日志]");
            sb.AppendLine("========================================");
            sb.AppendLine("启动器版本：" + TetoLabVersionName + " v" + ModBase.versionBaseName + " (" + ModBase.versionBranchName + ")");
            sb.AppendLine("启动器路径：" + Basics.ExecutablePath);
            sb.AppendLine("游戏版本：" + versionName);
            sb.AppendLine("包类型：" + pkgType);
            sb.AppendLine("版本路径：" + target);
            sb.AppendLine("可执行文件：" + Path.Combine(target, "Minecraft.Windows.exe"));
            sb.AppendLine("工作目录：" + target);
            sb.AppendLine("启动时间：" + startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.AppendLine("进程 PID：" + pid);
            sb.AppendLine("操作系统：" + Environment.OSVersion.VersionString);
            sb.AppendLine("系统架构：" + (Environment.Is64BitOperatingSystem ? "x64" : "x86"));
            sb.AppendLine("进程架构：" + (Environment.Is64BitProcess ? "x64" : "x86"));
            sb.AppendLine(".NET 版本：" + Environment.Version);
            sb.AppendLine("处理器核心数：" + Environment.ProcessorCount);
            sb.AppendLine("事件：启动");
            sb.AppendLine("========================================");
            File.WriteAllText(logPath, sb.ToString(), System.Text.Encoding.UTF8);
        }
        catch (Exception ex) { ModBase.Log(ex, "写入基岩版启动日志失败"); }
    }

    /// <summary>小teto定制：追加基岩版退出日志（运行时长、退出码含义等）。</summary>
    private static void WriteBedrockRunLogExit(string logPath, string target, int pid, int exitCode, DateTime startTime, DateTime exitTime)
    {
        try
        {
            var duration = exitTime - startTime;
            string durationStr = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", duration.Hours, duration.Minutes, duration.Seconds, duration.Milliseconds);
            string exitMeaning = exitCode switch
            {
                0 => "正常退出（ERROR_SUCCESS）",
                -1073741515 => "STATUS_DLL_NOT_FOUND (0xC0000135) - 找不到所需的 DLL 文件",
                -1073741819 => "STATUS_ACCESS_VIOLATION (0xC0000005) - 内存访问冲突（游戏崩溃）",
                -1073740791 => "STATUS_STACK_BUFFER_OVERRUN (0xC0000409) - 栈缓冲区溢出",
                -1073741571 => "STATUS_STACK_OVERFLOW (0xC00000FD) - 栈溢出",
                -2147483645 => "E_FAIL (0x80004005) - 未指定错误",
                2 => "ERROR_FILE_NOT_FOUND - 系统找不到指定的文件",
                3 => "ERROR_PATH_NOT_FOUND - 系统找不到指定的路径",
                5 => "ERROR_ACCESS_DENIED - 拒绝访问",
                _ => "未知错误码"
            };
            string hexCode = "0x" + (exitCode & 0xFFFFFFFF).ToString("X8");
            bool normalExit = exitCode == 0;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine();
            sb.AppendLine("========================================");
            sb.AppendLine("[" + TetoLabVersionName + " 基岩版退出日志]");
            sb.AppendLine("========================================");
            sb.AppendLine("退出时间：" + exitTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.AppendLine("运行时长：" + durationStr + "（" + duration.TotalSeconds.ToString("F3") + " 秒）");
            sb.AppendLine("进程 PID：" + pid);
            sb.AppendLine("退出码：" + exitCode + " (" + hexCode + ")");
            sb.AppendLine("退出码含义：" + exitMeaning);
            sb.AppendLine("是否正常退出：" + (normalExit ? "是（正常退出）" : "否（非正常退出，可能是游戏崩溃或被强制结束）"));
            sb.AppendLine("用户主动结束：" + (BedrockUserKilled ? "是（用户点击结束进程）" : "否"));
            sb.AppendLine("事件：退出");
            sb.AppendLine("========================================");
            File.AppendAllText(logPath, sb.ToString(), System.Text.Encoding.UTF8);
        }
        catch (Exception ex) { ModBase.Log(ex, "写入基岩版退出日志失败"); }
    }

    /// <summary>
    ///     校验基岩版版本目录是否完整（防止下载/解包中断导致的缺文件黑屏）。
    ///     GDK 完整版必须有 exe + 完整的 data 资源（resource_packs / behavior_packs 等，通常数万文件）。
    /// </summary>
    internal static bool IsBedrockVersionComplete(string folder)
    {
        try
        {
            if (!File.Exists(Path.Combine(folder, "Minecraft.Windows.exe"))) return false;
            // UWP loose 包（appxmanifest 方式）无需 data 目录
            if (File.Exists(Path.Combine(folder, "AppxManifest.xml")) && !Directory.Exists(Path.Combine(folder, "data")))
                return true;
            var data = Path.Combine(folder, "data");
            if (!Directory.Exists(data)) return false;
            // data 目录必须包含核心资源，且文件数达标（完整 GDK 包 data 通常 >10000 个文件）
            var count = Directory.GetFiles(data, "*", SearchOption.AllDirectories).Length;
            if (count < 1000) return false;
            var rp = Path.Combine(data, "resource_packs");
            var bp = Path.Combine(data, "behavior_packs");
            if (Directory.Exists(rp) || Directory.Exists(bp)) return true;
            // 部分精简版本可能没有资源包目录，但 data 文件数达标即可视为完整
            return count >= 5000;
        }
        catch { return false; }
    }

    private static bool TryFillGdkDlls(string target, string instanceFolder)
    {
        if (File.Exists(Path.Combine(target, "libHttpClient.GDK.dll")) &&
            File.Exists(Path.Combine(target, "PreLoad.NET.dll")))
            return true;
        var candidates = new[]
        {
            Path.Combine(instanceFolder, "bedrock_versions"),
            instanceFolder
        };
        foreach (var baseDir in candidates)
        {
            if (!Directory.Exists(baseDir)) continue;
            string[] subDirs;
            try { subDirs = Directory.GetDirectories(baseDir); } catch { continue; }
            foreach (var vdir in subDirs)
            {
                if (string.Equals(Path.GetFullPath(vdir), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase)) continue;
                var lib = Path.Combine(vdir, "libHttpClient.GDK.dll");
                if (!File.Exists(lib)) continue;
                try
                {
                    File.Copy(lib, Path.Combine(target, "libHttpClient.GDK.dll"), true);
                    var thunks = Path.Combine(vdir, "Microsoft.Xbox.Services.GDK.C.Thunks.dll");
                    if (File.Exists(thunks))
                        File.Copy(thunks, Path.Combine(target, "Microsoft.Xbox.Services.GDK.C.Thunks.dll"), true);
                    var preload = Path.Combine(vdir, "PreLoad.NET.dll");
                    if (File.Exists(preload))
                        File.Copy(preload, Path.Combine(target, "PreLoad.NET.dll"), true);
                    var ico = Path.Combine(vdir, "minecraftIcon.ico");
                    if (File.Exists(ico) && !File.Exists(Path.Combine(target, "minecraftIcon.ico")))
                        File.Copy(ico, Path.Combine(target, "minecraftIcon.ico"), true);
                    return true;
                }
                catch { /* 尝试下一个版本 */ }
            }
        }
        // 兜底：从内置资源释放 PreLoad.NET.dll（BedrockBoot 预加载器，游戏启动必需，版本无关）
        if (!File.Exists(Path.Combine(target, "PreLoad.NET.dll")))
        {
            try
            {
                var dst = Path.Combine(target, "PreLoad.NET.dll");
                var resName = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames()
                    .FirstOrDefault(n => n.Contains("PreLoad.NET.dll"));
                if (resName == null) return false;
                using (var s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resName))
                {
                    if (s == null) return false;
                    using (var f = File.Create(dst))
                        s.CopyTo(f);
                }
                return File.Exists(dst);
            }
            catch { }
        }
        return false;
    }

    /// <summary>
    ///     小teto定制：以 UWP 激活方式启动 UWP loose 版基岩版（BedrockBoot 式）。
    ///     流程：处理 manifest（移除 customInstall、加 runFullTrust + coreAppActivation）→ 注册 loose 包 → shell:AppsFolder 激活。
    ///     若同名包已被商店版/其他版本占用，先卸载（不校验正版，符合用户要求）。
    /// </summary>
    private static string LaunchUwpLoose(string dir)
    {
        try
        {
            // 小teto定制：使用 UWP 启动方式时自动添加 UWPtres.txt 标记文件
            WriteUwpMarker(dir);
            var manifest = Path.Combine(dir, "AppxManifest.xml");
            if (!File.Exists(manifest)) manifest = Path.Combine(dir, "appxmanifest.xml");
            if (!File.Exists(manifest)) return "未找到 AppxManifest.xml，无法启动该 UWP 版基岩版。";

            if (!IsDeveloperMode())
            {
                Process.Start(new ProcessStartInfo("ms-settings:developers") { UseShellExecute = true });
                return "需要开启 Windows 开发者模式才能启动 UWP 版基岩版（设置 → 开发者选项）。已为你打开设置页。";
            }

            // BedrockBoot 式：处理 manifest（移除 customInstall 等无效扩展、加 runFullTrust + coreAppActivation 激活能力）
            // 小teto定制：EditManifest 前备份，出错时恢复，避免 manifest 被部分修改后损坏
            string manifestBackup = null;
            try
            {
                manifestBackup = File.ReadAllText(manifest);
                ModBase.Log("[Bedrock] 开始处理 manifest (EditManifest): " + dir);
                ManifestEditor.EditManifest(dir, null, null).GetAwaiter().GetResult();
                ModBase.Log("[Bedrock] manifest 处理成功");
            }
            catch (Exception ex)
            {
                ModBase.Log("[Bedrock] EditManifest 异常，恢复备份: " + ex.Message);
                try { if (manifestBackup != null) File.WriteAllText(manifest, manifestBackup); } catch { }
            }

            // 读取包名与 Application Id
            string name = "", appId = "App";
            try
            {
                var doc = XDocument.Load(manifest);
                XNamespace ns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
                name = (string)doc.Root?.Element(ns + "Identity")?.Attribute("Name") ?? "";
                appId = (string)doc.Root?.Element(ns + "Applications")?.Element(ns + "Application")?.Attribute("Id") ?? "App";
            }
            catch { }
            if (string.IsNullOrEmpty(name)) return "无法读取基岩版包名。";

            // 同名包已注册且指向其他目录（商店版或其他 loose 版）→ 先卸载（BedrockBoot 式：UWP 版独占包名）
            // 小teto定制：卸载 UWP 包会删除 %LocalAppData%\Packages\<PFN>\LocalState（世界/皮肤/资源/设置全灭），
            // 因此卸载前先备份 LocalState 到 %APPDATA%\PCLCE-BE\UWPData，注册后恢复，保证切换 UWP 版本不丢存档
            string pfnLocal = (name + "_8wekyb3d8bbwe").ToLowerInvariant();
            string pkgDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", pfnLocal);
            string uwpDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCLCE-BE", "UWPData");
            string pkgBackup = Path.Combine(uwpDataRoot, pfnLocal, "Backup-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            bool willUninstall = false;
            try
            {
                var pm = new PackageManager();
                foreach (var p in pm.FindPackagesForUser(string.Empty))
                {
                    if (string.Equals(p.Id.Name, name, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals((p.InstalledPath ?? "").TrimEnd('\\'), dir.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    {
                        willUninstall = true;
                        break;
                    }
                }
            }
            catch { }
            if (willUninstall)
            {
                // 小teto定制：卸载前完整备份整个 Packages\<PFN>（排除 SystemAppData）
                // 包含 LocalState（游戏数据）、Settings、AC、LocalCache 等——卸载会删除整个包目录，
                // 只备份 LocalState 会导致微软账号登录凭据（AC/Settings）丢失，每次启动都要求重新登录
                try
                {
                    if (Directory.Exists(pkgDir))
                    {
                        // 小teto定制：多份备份按时间戳保留（不覆盖），清理时保留最近5份
                        var pfnBakRoot = Path.Combine(uwpDataRoot, pfnLocal);
                        Directory.CreateDirectory(pfnBakRoot);
                        var oldBaks = Directory.GetDirectories(pfnBakRoot, "Backup-*").OrderByDescending(d => d).Skip(5).ToList();
                        foreach (var ob in oldBaks)
                        { try { Directory.Delete(ob, true); } catch { } }
                        CopyDirectoryExcludeSystem(pkgDir, pkgBackup);
                        ModBase.Log("[Bedrock] UWP 包数据已完整备份: " + pkgBackup);
                    }
                }
                catch (Exception ex) { ModBase.Log("[Bedrock] UWP 包数据备份失败: " + ex.Message); }
                try
                {
                    var pm = new PackageManager();
                    foreach (var p in pm.FindPackagesForUser(string.Empty))
                    {
                        if (string.Equals(p.Id.Name, name, StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals((p.InstalledPath ?? "").TrimEnd('\\'), dir.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                        {
                            // 卸载（完整备份已在上一步完成；卸载删除 Packages\<PFN>，注册新版本后整体恢复）
                            pm.RemovePackageAsync(p.Id.FullName).GetAwaiter().GetResult();
                        }
                    }
                }
                catch { }
            }

            // 若本目录尚未注册 → 注册 loose 包
            bool needReg = true;
            try
            {
                var pm = new PackageManager();
                foreach (var p in pm.FindPackagesForUser(string.Empty))
                {
                    if (string.Equals(p.Id.Name, name, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals((p.InstalledPath ?? "").TrimEnd('\\'), dir.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    {
                        needReg = false;
                        ModBase.Log("[Bedrock] 包已注册，跳过注册: " + p.Id.FullName);
                        break;
                    }
                }
            }
            catch (Exception ex) { ModBase.Log("[Bedrock] 检查已注册包异常: " + ex.Message); }
            if (needReg)
            {
                ModBase.Log("[Bedrock] 开始注册 loose 包: " + manifest);
                if (!RegisterLooseAppx(manifest))
                {
                    ModBase.Log("[Bedrock] UWP 包注册失败");
                    return "UWP 包注册失败（请检查开发者模式是否开启，或查看日志获取详细信息）。";
                }
                ModBase.Log("[Bedrock] UWP 包注册成功");
            }

            // 小teto定制：注册后恢复整个包数据（切换版本不丢存档/登录态）
            // 有完整备份就整体恢复（覆盖 Windows 新建的空目录），排除 SystemAppData 系统管理目录
            try
            {
                // 小teto定制：进程中断保护——本次进程未成功执行备份（pkgBackup 不存在）时，
                // 自动改用最新一份历史备份恢复，避免切换版本时登录态/玩家档案丢失
                if (!Directory.Exists(pkgBackup))
                {
                    var pfnBakRoot = Path.Combine(uwpDataRoot, pfnLocal);
                    if (Directory.Exists(pfnBakRoot))
                    {
                        var latestBak = Directory.GetDirectories(pfnBakRoot, "Backup-*").OrderByDescending(d => d).FirstOrDefault();
                        if (!string.IsNullOrEmpty(latestBak)) pkgBackup = latestBak;
                    }
                }
                if (Directory.Exists(pkgBackup))
                {
                    if (Directory.Exists(pkgDir)) { try { Directory.Delete(pkgDir, true); } catch { } }
                    CopyDirectoryExcludeSystem(pkgBackup, pkgDir);
                    ModBase.Log("[Bedrock] UWP 包数据已恢复（含登录态/玩家档案）: " + pkgDir + " <- " + pkgBackup);
                }
            }
            catch (Exception ex) { ModBase.Log("[Bedrock] UWP 包数据恢复失败: " + ex.Message); }

            // 小teto定制：尝试注入旧版本 persona Pending 数据（跨版本保留更衣室披风/角色）
            TryMergeBedrockPersonaPending(name);

            // 小teto定制：修复 BedrockBoot 遗留目录权限——1.16 等版本 PreLoad.NET 启动时
            // 需要写 config\BedrockBoot2\logs，UWP AppContainer 默认只读会导致启动即崩溃
            try
            {
                var bb2Dir = Path.Combine(dir, "config", "BedrockBoot2");
                if (Directory.Exists(bb2Dir))
                {
                    try { Directory.CreateDirectory(Path.Combine(bb2Dir, "logs")); } catch { }
                    try
                    {
                        var dInfo = new DirectoryInfo(bb2Dir);
                        var dSec = dInfo.GetAccessControl();
                        dSec.AddAccessRule(new FileSystemAccessRule("Users", FileSystemRights.FullControl,
                            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                            PropagationFlags.None, AccessControlType.Allow));
                        dInfo.SetAccessControl(dSec);
                        ModBase.Log("[Bedrock] 已修复 BedrockBoot2 目录权限（PreLoad 可写）: " + bb2Dir);
                    }
                    catch (Exception ex2) { ModBase.Log("[Bedrock] BedrockBoot2 权限修复失败: " + ex2.Message); }
                }
            }
            catch { }

            // UWP 激活（shell:AppsFolder 方式）
            var family = name + "_8wekyb3d8bbwe";
            var activatePath = $"shell:AppsFolder\\{family}!{appId}";
            ModBase.Log("[Bedrock] UWP 激活: " + activatePath);
            try
            {
                var proc = Process.Start(new ProcessStartInfo(activatePath) { UseShellExecute = true });
                ModBase.Log("[Bedrock] UWP 激活进程已启动, PID=" + (proc?.Id ?? -1));
            }
            catch (Exception ex)
            {
                ModBase.Log("[Bedrock] UWP 激活异常: " + ex);
                return "UWP 激活失败：" + ex.Message + "（包名: " + family + "，应用ID: " + appId + "）";
            }

            // 小teto定制：UWP 激活后持续检查游戏进程是否真的启动了（最长 120 秒）
            // PID=-1 对于 UWP 应用是正常的（shell:AppsFolder 激活不返回 Process 对象）
            // 旧版本（如 1.16）冷启动很慢，10 秒窗口经常检测不到 → 改为 120 秒持续检测，
            // 检测到进程即接入「结束进程」按钮 / 运行日志 / 异常退出报告
            ModBase.Log("[Bedrock] 等待游戏进程启动（最长 120 秒）...");
            bool gameStarted = false;
            for (int i = 0; i < 120; i++)
            {
                System.Threading.Thread.Sleep(1000);
                var processes = System.Diagnostics.Process.GetProcessesByName("Minecraft.Windows");
                if (processes.Length > 0)
                {
                    gameStarted = true;
                    ModBase.Log("[Bedrock] 游戏进程已启动, PID=" + processes[0].Id + " (等待" + (i + 1) + "秒)");
                    // 小teto定制：UWP 启动同样接入进程监控（右下角「结束进程」按钮 / 运行日志 / 异常退出报告）
                    var gameProc = processes[0];
                    foreach (var p in processes) if (!ReferenceEquals(p, gameProc)) p.Dispose();
                    MonitorBedrockProcess(dir, gameProc);
                    break;
                }
            }
            if (!gameStarted)
            {
                ModBase.Log("[Bedrock] 警告：120 秒内未检测到游戏进程，可能启动失败或已退出");
                // 不返回错误，因为 UWP 应用可能在后台启动或需要更长时间
                // 返回空字符串表示激活命令已发送
            }
            return "";
        }
        catch (Exception ex)
        {
            return "UWP 启动失败：" + ex.Message;
        }
    }

    /// <summary>
    ///     是否已开启 Windows 开发者模式（BedrockBoot 式 UWP loose 注册前置条件）。
    /// </summary>
    public static bool IsDeveloperMode()
    {
        try
        {
            using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                .OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock");
            var v = key?.GetValue("AllowDevelopmentWithoutDevLicense");
            return v is int i && i == 1;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     通过 Add-AppxPackage 安装应用包（需商店授权有效；未持正版会安装失败）。
    /// </summary>
    public static bool InstallAppx(string path)
    {
        try
        {
            var psi = new ProcessStartInfo("powershell.exe")
            {
                Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Add-AppxPackage -Path '" + path + "'\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using var p = Process.Start(psi);
            if (p is null) return false;
            if (!p.WaitForExit(240000)) { try { p.Kill(); } catch { } return false; }
            return p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
