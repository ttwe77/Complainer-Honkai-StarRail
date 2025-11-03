/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Timers;
using System.Windows.Forms;

namespace TrayNotifier
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new TrayAppContext());
        }
    }

    class TrayAppContext : ApplicationContext
    {
        readonly NotifyIcon notifyIcon;
        readonly System.Timers.Timer timer;
        readonly string appFolder;
        readonly string configPath;
        readonly string noticeFolder; // 新增：notices文件夹路径
        readonly string appIconPath; // 新增：应用程序图标路径
        Dictionary<string, NoticeFile> cache = new Dictionary<string, NoticeFile>(StringComparer.OrdinalIgnoreCase);
        readonly Random rnd;
        bool balloonShowing = false;
        int intervalSeconds = 3600;
        List<string> noticeFiles = new List<string>();

        public TrayAppContext()
        {
            appFolder = AppContext.BaseDirectory;
            configPath = Path.Combine(appFolder, "config.ini");
            noticeFolder = Path.Combine(appFolder, "notices"); // 新增：notices子文件夹
            appIconPath = Path.Combine(appFolder, "icon.ico"); // 新增：项目目录下的 icon.ico
            rnd = new Random();

            notifyIcon = new NotifyIcon
            {
                Visible = true,
                Text = "说怪话Complainer",
                // 使用项目目录下的 icon.ico（存在时），否则回退到 SystemIcons.Application
                Icon = File.Exists(appIconPath) ? new Icon(appIconPath) : SystemIcons.Application
            };

            // Context menu
            var menu = new ContextMenuStrip();
            var sendNow = new ToolStripMenuItem("立即发送通知");
            sendNow.Click += (s, e) => { TryShowNotification(); };
            var about = new ToolStripMenuItem("关于");
            about.Click += (s, e) => { ShowAboutDialog(); };
            var exit = new ToolStripMenuItem("退出");
            exit.Click += (s, e) => Exit();
            menu.Items.Add(about);
            menu.Items.Add(sendNow);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exit);
            notifyIcon.ContextMenuStrip = menu;

            notifyIcon.BalloonTipClosed += (s, e) => { balloonShowing = false; RestoreDefaultIcon(); };
            notifyIcon.BalloonTipClicked += (s, e) => { balloonShowing = false; RestoreDefaultIcon(); };
            notifyIcon.MouseDoubleClick += (s, e) =>
            {
                // 双击也立即发送（可选）
                TryShowNotification();
            };

            EnsureSampleFilesAndLoadConfig();

            timer = new System.Timers.Timer(intervalSeconds * 1000);
            timer.Elapsed += (s, e) => TryShowNotification();
            timer.AutoReset = true;
            timer.Start();
        }

        void ShowAboutDialog()
        {
            using (var f = new AboutForm())
            {
                // 将托盘图标传过去以便 About 窗口使用（可选）
                try { f.Icon = notifyIcon.Icon; } catch { }
                f.ShowDialog();
            }
        }

        void EnsureSampleFilesAndLoadConfig()
        {
            // If no config, create one
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath,
@"# interval_seconds = 发送周期（秒）
interval_seconds=60
# notice_files = 指定一个或多个 notice json 文件，多个用分号分隔
notice_files=notices.json
");
            }

            // parse config
            var cfg = ParseIni(configPath);
            if (cfg.TryGetValue("interval_seconds", out var ivStr) && int.TryParse(ivStr, out int iv) && iv > 0)
                intervalSeconds = iv;
            else
                intervalSeconds = 3600;

            noticeFiles.Clear();

            if (cfg.TryGetValue("notice_files", out var nfStr) && !string.IsNullOrWhiteSpace(nfStr))
            {
                foreach (var part in nfStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var p = part.Trim();
                    // 始终放到notices文件夹下
                    if (!Path.IsPathRooted(p))
                        p = Path.Combine(noticeFolder, p);
                    noticeFiles.Add(p);
                }
            }

            if (noticeFiles.Count == 0)
            {
                // default
                var defaultFile = Path.Combine(noticeFolder, "notices.json");
                noticeFiles.Add(defaultFile);
            }

            // ensure notice directory & sample json exists
            if (!Directory.Exists(noticeFolder))
                Directory.CreateDirectory(noticeFolder);

            foreach (var nf in noticeFiles)
            {
                var dir = Path.GetDirectoryName(nf);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(nf))
                {
                    // write notices
                    var sample = new NoticeFile
                    {
                        notifications = new List<Notice>
                        {
                            new Notice { title = "说怪话", body = "你好会上班呀～" },
                            new Notice { title = "说怪话", body = "老板真忙呢～" },
                            new Notice { title = "说怪话", body = "老板好认真呀～" },
                            new Notice { title = "说怪话", body = "老板发大财呢～" },
                            new Notice { title = "说怪话", body = "老板捡到宝了～" },
                            new Notice { title = "说怪话", body = "老板好英明～" },
                            new Notice { title = "说怪话", body = "老板真厉害呀～" },
                            new Notice { title = "说怪话", body = "好威风呀～" },
                            new Notice { title = "说怪话", body = "真拼呢～" },
                            new Notice { title = "说怪话", body = "老板真有想法呢～" },
                            new Notice { title = "说怪话", body = "老板真稳健呢～" },
                            new Notice { title = "说怪话", body = "老板好严格～" },
                            new Notice { title = "说怪话", body = "下班真早呢～" }
                        }
                    };
                    var json = JsonSerializer.Serialize(sample, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(nf, json);

                    // create icons folder and a simple ico placeholder if not exists
                    var iconFolder = Path.Combine(appFolder, "icons");
                    if (!Directory.Exists(iconFolder)) Directory.CreateDirectory(iconFolder);
                    var icoPath = Path.Combine(iconFolder, "icon.ico");
                    if (!File.Exists(icoPath))
                    {
                        // write a tiny placeholder .ico from SystemIcons.Application
                        using (var bmp = SystemIcons.Application.ToBitmap())
                        {
                            using (var fs = new FileStream(icoPath, FileMode.Create))
                            {
                                IconFromBitmap(bmp).Save(fs);
                            }
                        }
                    }
                }
            }
        }

        static Icon IconFromBitmap(Bitmap bmp)
        {
            // Convert Bitmap to Icon via HIcon. Not perfect, but works for tray.
            var hIcon = bmp.GetHicon();
            var ico = Icon.FromHandle(hIcon);
            return ico;
        }

        Dictionary<string, string> ParseIni(string path)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith("#") || line.StartsWith(";")) continue;
                var idx = line.IndexOf('=');
                if (idx <= 0) continue;
                var key = line.Substring(0, idx).Trim();
                var val = line.Substring(idx + 1).Trim();
                dict[key] = val;
            }
            return dict;
        }

        void TryShowNotification()
        {
            if (balloonShowing) return; // 保证只有一个通知显示
            Notice selected = null;
            string usedFile = null;

            // choose a random notice file first (if multiple)
            var availableFiles = new List<string>();
            foreach (var f in noticeFiles)
                if (File.Exists(f)) availableFiles.Add(f);
            if (availableFiles.Count == 0) return;

            usedFile = availableFiles[rnd.Next(availableFiles.Count)];

            try
            {
                // load or get from cache
                if (!cache.TryGetValue(usedFile, out var nf))
                {
                    var txt = File.ReadAllText(usedFile);
                    nf = JsonSerializer.Deserialize<NoticeFile>(txt, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (nf == null || nf.notifications == null || nf.notifications.Count == 0)
                        return;
                    cache[usedFile] = nf;
                }

                var list = cache[usedFile].notifications;
                if (list == null || list.Count == 0) return;
                selected = list[rnd.Next(list.Count)];
            }
            catch
            {
                // 出错就跳过
                return;
            }

            if (selected == null) return;

            // 不再根据通知切换托盘图标，始终使用应用程序图标（项目目录下的 icon.ico）

            // show balloon tip
            balloonShowing = true;
            int timeoutMs = 10000;
            if (selected.timeout_seconds.HasValue)
                timeoutMs = Math.Max(3000, selected.timeout_seconds.Value * 1000);
            // ShowBalloonTip expects timeout in milliseconds but historically used as milliseconds in newer frameworks.
            notifyIcon.ShowBalloonTip(timeoutMs, selected.title ?? "通知", selected.body ?? "", ToolTipIcon.None);

            // Restore icon when balloon closed is handled by event handlers
        }

        void RestoreDefaultIcon()
        {
            // restore default icon (Application icon)
            try
            {
                // 仍然使用项目目录下的 icon.ico（若存在），否则使用 SystemIcons.Application
                notifyIcon.Icon = File.Exists(appIconPath) ? new Icon(appIconPath) : SystemIcons.Application;
            }
            catch { }
        }

        void Exit()
        {
            timer?.Stop();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }
    }

    // JSON model
    class NoticeFile
    {
        public List<Notice> notifications { get; set; }
    }

    class Notice
    {
        public string title { get; set; }
        public string body { get; set; }
        // path relative to app folder or absolute
        public string icon { get; set; }
        // optional per-notification timeout in seconds
        public int? timeout_seconds { get; set; }
    }

    // AboutForm moved outside of TrayAppContext
    public class AboutForm : Form
    {
        public AboutForm()
        {
            Text = "关于";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(800, 400);

            // 稍微好看一点的字体和布局
            var lblTitle = new Label
            {
                Text = "说怪话Complainer",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(14, 14)
            };

            var lblAuthor = new Label
            {
                Text = "作者: ttwe77",
                AutoSize = true,
                Location = new Point(14, 50)
            };

            var lblLicense = new Label
            {
                Text = "开源协议: GNU AGPL v3",
                AutoSize = true,
                Location = new Point(14, 78)
            };

            var link = new LinkLabel
            {
                Text = "查看协议全文（GNU AGPL v3）",
                AutoSize = true,
                Location = new Point(14, 104)
            };
            link.Links.Add(0, link.Text.Length, "https://www.gnu.org/licenses/agpl-3.0.en.html");
            link.LinkClicked += (s, e) =>
            {
                var url = e.Link.LinkData as string;
                if (!string.IsNullOrEmpty(url))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    }
                    catch
                    {
                        // 忽略打开失败
                    }
                }
            };

            var txt = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Location = new Point(14, 136),
                Size = new Size(ClientSize.Width - 28, 40),
                Text = "本程序由作者 ttwe77 发布，遵循 GNU AGPL v3 开源协议。"
            };

            var btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Size = new Size(120, 40),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnOk.Location = new Point(ClientSize.Width - btnOk.Width - 14, ClientSize.Height - btnOk.Height - 10);

            Controls.AddRange(new Control[] { lblTitle, lblAuthor, lblLicense, link, txt, btnOk });
            AcceptButton = btnOk;
        }
    }
}
