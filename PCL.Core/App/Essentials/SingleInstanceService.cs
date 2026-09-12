using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using PCL.Core.App.IoC;
using PCL.Core.IO;
using PCL.Core.Utils;

namespace PCL.Core.App.Essentials;

[LifecycleService(LifecycleState.BeforeLoading, Priority = -2134567890)]
[LifecycleScope("single-instance", "单例", false)]
public sealed partial class SingleInstanceService
{
    private static FileStream? _lockStream;
    private static readonly string _LockFilePath = Path.Combine(Paths.SharedLocalData, "instance_teto.lock"); // 小teto实验室定制版：独立单例锁，可与官方版共存

    private static void _TryRpc(string processId, string content)
    {
        var pipeName = $"{RpcService.PipePrefix}@{processId}";
        using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
        pipe.Connect(1000);
        using var sw = new StreamWriter(pipe, PipeComm.PipeEncoding);
        sw.WriteLine(content);
        sw.Write(PipeComm.PipeEndingChar);
        sw.Flush();
    }

    [LifecycleStart]
    private static void _Start()
    {
        try
        {
            // 小teto定制：确保单例锁父目录存在（修复 %LOCALAPPDATA%\PCLCE 目录缺失导致启动即退出的问题）
            var lockDir = Path.GetDirectoryName(_LockFilePath);
            if (!string.IsNullOrEmpty(lockDir)) Directory.CreateDirectory(lockDir);
            var stream = File.Open(_LockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            Context.Debug("未发现重复实例，正在向单例锁写入信息");
            using var sw = new StreamWriter(stream, Encoding.ASCII, 8, true);
            sw.Write(Basics.CurrentProcessId);
            sw.Flush();
            _lockStream = stream;
        }
        catch (Exception)
        {
            try
            {
                using var stream = File.Open(_LockFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                var pid = reader.ReadToEnd();
                Context.Info($"发现重复实例 {pid}，尝试传递参数并拉起主窗口");
                try
                {
                    _TryRpc(pid, "REQ cli\n" + JsonSerializer.Serialize(StartupService.UnhandledCommands, JsonCompat.SerializerOptions));
                    _TryRpc(pid, "REQ activate");
                }
                catch (Exception ex) { Context.Warn("RPC 通信失败", ex); }
            }
            catch (Exception ex) { Context.Error("读取单例锁出错", ex); }
            finally { Context.RequestExit(1); }
        }
    }

    [LifecycleStop]
    private static void _Stop()
    {
        if (_lockStream is null) return;
        Context.Debug("正在删除单例锁");
        _lockStream.Dispose();
        File.Delete(_LockFilePath);
    }
}
