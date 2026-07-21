using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

internal static class Program
{
    private const string ApiBase = "https://steamhub.156354.xyz/api/games/";
    private static readonly string[] ToolFiles = { "OpenSteamTool.dll", "dwmapi.dll", "xinput1_4.dll" };

    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "Steam 游戏激活工具";
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

        try
        {
            string steamDir = FindSteamDirectory();
            if (String.IsNullOrEmpty(steamDir))
            {
                WriteError("未检测到 Steam 安装目录。");
                Console.Write("请输入 Steam 安装目录: ");
                steamDir = NormalizePath(Console.ReadLine());
            }
            if (!IsSteamDirectory(steamDir))
                throw new InvalidOperationException("Steam 安装目录无效，目录中应包含 steam.exe。");

            if (args.Length > 0 && args[0].Equals("install", StringComparison.OrdinalIgnoreCase))
                return Install(steamDir) ? 0 : 1;
            if (args.Length > 0 && args[0].Equals("activate", StringComparison.OrdinalIgnoreCase))
                return Activate(steamDir, args.Length > 1 ? args[1] : null) ? 0 : 1;

            while (true)
            {
                Header(steamDir);
                Console.WriteLine("  [1] 安装 / 修复 OpenSteamTools");
                Console.WriteLine("  [2] 激活 Steam 游戏");
                Console.WriteLine("  [3] 检查安装状态");
                Console.WriteLine("  [0] 退出");
                Console.Write("\n请选择: ");
                string choice = (Console.ReadLine() ?? "").Trim();
                Console.WriteLine();

                if (choice == "0") return 0;
                if (choice == "1") Install(steamDir);
                else if (choice == "2") Activate(steamDir, null);
                else if (choice == "3") ShowInstallStatus(steamDir);
                else WriteError("请输入 0、1、2 或 3。");

                Console.WriteLine("\n按任意键返回主菜单...");
                Console.ReadKey(true);
            }
        }
        catch (Exception ex)
        {
            WriteError(ex.Message);
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey(true);
            return 1;
        }
    }

    private static bool Install(string steamDir)
    {
        Console.WriteLine("正在安装 OpenSteamTools...");
        try
        {
            EnsureSteamStopped();
            var oldFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string file in Directory.GetFiles(steamDir, "xinput1_*.*", SearchOption.TopDirectoryOnly)) oldFiles.Add(file);
            foreach (string file in Directory.GetFiles(steamDir, "dwmapi.*", SearchOption.TopDirectoryOnly)) oldFiles.Add(file);
            foreach (string file in oldFiles)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
                Console.WriteLine("  已删除: " + Path.GetFileName(file));
            }

            foreach (string name in ToolFiles)
            {
                ExtractResource(name, Path.Combine(steamDir, name));
                Console.WriteLine("  已复制: " + name);
            }
            if (!InstallationIsValid(steamDir, true)) throw new IOException("文件写入后的完整性检查未通过。");
            WriteSuccess("安装完成。建议重启 Steam 后再激活游戏。");
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            WriteError("没有写入 Steam 目录的权限，请右键本程序并选择“以管理员身份运行”。");
            return false;
        }
        catch (Exception ex)
        {
            WriteError("安装失败: " + ex.Message);
            return false;
        }
    }

    private static bool Activate(string steamDir, string suppliedAppId)
    {
        if (!InstallationIsValid(steamDir, true))
        {
            WriteError("安装检查未通过，请先选择 [1] 安装 / 修复 OpenSteamTools。");
            return false;
        }

        string appId = suppliedAppId;
        if (String.IsNullOrWhiteSpace(appId))
        {
            Console.Write("请输入 Steam 游戏 AppID: ");
            appId = Console.ReadLine();
        }
        appId = (appId ?? "").Trim();
        ulong parsed;
        if (!UInt64.TryParse(appId, out parsed) || parsed == 0)
        {
            WriteError("AppID 格式错误，应为纯数字。");
            return false;
        }

        try
        {
            Console.WriteLine("正在查询游戏 " + appId + "...");
            GameResponse response = GetGame(appId);
            if (response == null || !response.success || response.data == null || String.IsNullOrEmpty(response.data.download_token))
            {
                WriteError("游戏 " + appId + " 未收录。");
                return false;
            }
            WriteSuccess("游戏 " + appId + " 已找到" + (String.IsNullOrEmpty(response.data.name) ? "。" : ": " + response.data.name));

            string tempRoot = Path.Combine(Path.GetTempPath(), "SteamUnlock", Guid.NewGuid().ToString("N"));
            string zipPath = Path.Combine(tempRoot, appId + ".zip");
            Directory.CreateDirectory(tempRoot);
            try
            {
                string url = ApiBase + Uri.EscapeDataString(appId) + "/download?token=" + Uri.EscapeDataString(response.data.download_token);
                Console.WriteLine("正在下载配置包...");
                DownloadFile(url, zipPath);
                string luaDir = Path.Combine(steamDir, "config", "lua");
                Directory.CreateDirectory(luaDir);
                int count = ExtractGameFiles(zipPath, luaDir);
                if (count == 0) throw new InvalidDataException("下载包中没有 .json、.manifest 或 .lua 文件。");
                WriteSuccess("激活成功，已写入 " + count + " 个文件。");
                Console.WriteLine("如未生效，请完全退出并重新启动 Steam。");
                return true;
            }
            finally
            {
                try { if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true); } catch { }
            }
        }
        catch (WebException ex) { WriteError("网络请求失败: " + GetWebError(ex)); return false; }
        catch (UnauthorizedAccessException) { WriteError("没有写入 Steam 目录的权限，请以管理员身份运行。"); return false; }
        catch (Exception ex) { WriteError("激活失败: " + ex.Message); return false; }
    }

    private static GameResponse GetGame(string appId)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiBase + Uri.EscapeDataString(appId));
        request.Method = "GET";
        request.UserAgent = "SteamUnlock/1.0";
        request.Timeout = 20000;
        using (WebResponse response = request.GetResponse())
        using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            return new JavaScriptSerializer().Deserialize<GameResponse>(reader.ReadToEnd());
    }

    private static void DownloadFile(string url, string destination)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.UserAgent = "SteamUnlock/1.0";
        request.Timeout = 60000;
        request.ReadWriteTimeout = 60000;
        using (WebResponse response = request.GetResponse())
        using (Stream input = response.GetResponseStream())
        using (FileStream output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None)) input.CopyTo(output);
    }

    private static int ExtractGameFiles(string zipPath, string destination)
    {
        int count = 0;
        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string extension = Path.GetExtension(entry.Name);
                if (!extension.Equals(".json", StringComparison.OrdinalIgnoreCase) &&
                    !extension.Equals(".manifest", StringComparison.OrdinalIgnoreCase) &&
                    !extension.Equals(".lua", StringComparison.OrdinalIgnoreCase)) continue;
                string name = Path.GetFileName(entry.Name);
                if (String.IsNullOrEmpty(name)) continue;
                using (Stream input = entry.Open())
                using (FileStream output = new FileStream(Path.Combine(destination, name), FileMode.Create, FileAccess.Write, FileShare.None)) input.CopyTo(output);
                count++;
            }
        }
        return count;
    }

    private static void ExtractResource(string name, string destination)
    {
        using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream("Embedded." + name))
        {
            if (input == null) throw new FileNotFoundException("程序内缺少资源 " + name);
            using (FileStream output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None)) input.CopyTo(output);
        }
    }

    private static bool InstallationIsValid(string steamDir, bool printDetails)
    {
        bool valid = true;
        foreach (string name in ToolFiles)
        {
            string target = Path.Combine(steamDir, name);
            bool ok = File.Exists(target) && ResourceMatchesFile(name, target);
            if (printDetails)
            {
                Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine("  " + (ok ? "[正常] " : "[缺失/不匹配] ") + name);
                Console.ResetColor();
            }
            valid &= ok;
        }
        return valid;
    }

    private static bool ResourceMatchesFile(string name, string path)
    {
        using (SHA256 sha = SHA256.Create())
        using (Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Embedded." + name))
        using (Stream file = File.OpenRead(path))
        {
            if (resource == null) return false;
            return Convert.ToBase64String(sha.ComputeHash(resource)) == Convert.ToBase64String(sha.ComputeHash(file));
        }
    }

    private static void ShowInstallStatus(string steamDir)
    {
        if (InstallationIsValid(steamDir, true)) WriteSuccess("OpenSteamTools 安装状态正常。");
        else WriteError("安装状态异常，请选择 [1] 进行修复。");
    }

    private static string FindSteamDirectory()
    {
        string[] registryKeys = { @"HKEY_CURRENT_USER\Software\Valve\Steam", @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam" };
        string[] valueNames = { "SteamPath", "InstallPath" };
        foreach (string key in registryKeys)
            foreach (string valueName in valueNames)
            {
                string path = NormalizePath(Registry.GetValue(key, valueName, null) as string);
                if (IsSteamDirectory(path)) return path;
            }
        string[] common = { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam"), @"C:\Steam" };
        foreach (string path in common) if (IsSteamDirectory(path)) return path;
        return null;
    }

    private static string NormalizePath(string path)
    {
        if (String.IsNullOrWhiteSpace(path)) return null;
        path = Environment.ExpandEnvironmentVariables(path.Trim().Trim('"')).Replace('/', '\\');
        try { return Path.GetFullPath(path).TrimEnd('\\'); } catch { return null; }
    }

    private static bool IsSteamDirectory(string path)
    {
        return !String.IsNullOrEmpty(path) && Directory.Exists(path) && File.Exists(Path.Combine(path, "steam.exe"));
    }

    private static string GetWebError(WebException ex)
    {
        HttpWebResponse response = ex.Response as HttpWebResponse;
        return response == null ? ex.Message : ((int)response.StatusCode) + " " + response.StatusDescription;
    }

    private static void EnsureSteamStopped()
    {
        if (System.Diagnostics.Process.GetProcessesByName("steam").Length == 0) return;
        Console.Write("检测到 Steam 正在运行。请先完全退出 Steam，然后按 Enter 继续...");
        Console.ReadLine();
        if (System.Diagnostics.Process.GetProcessesByName("steam").Length != 0)
            throw new InvalidOperationException("Steam 仍在运行，请从托盘菜单退出 Steam 后重试。");
    }

    private static void Header(string steamDir)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================\n        Steam 游戏激活工具 v1.0\n========================================");
        Console.ResetColor();
        Console.WriteLine("Steam: " + steamDir + "\n");
    }

    private static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("[成功] " + message); Console.ResetColor();
    }

    private static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("[错误] " + message); Console.ResetColor();
    }

    private sealed class GameResponse { public bool success { get; set; } public GameData data { get; set; } }
    private sealed class GameData { public string name { get; set; } public string download_token { get; set; } }
}
