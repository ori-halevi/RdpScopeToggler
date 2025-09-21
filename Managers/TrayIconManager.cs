using RdpScopeToggler.Helpers;
using System;
using System.Drawing;
using System.Windows;

namespace RdpScopeToggler.Managers
{
    public class TrayIconManager : IDisposable
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;

        public TrayIconManager()
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
        }

        /// <summary>
        /// Initialize the tray icon with icon path, tooltip and context menu actions
        /// </summary>
        public void Initialize(string iconPath, string tooltip,
            Action onOpenWindow, Action onExit)
        {
            notifyIcon.Icon = new Icon(iconPath);
            notifyIcon.Text = tooltip;
            notifyIcon.Visible = true;

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            const string RepoApiUrl = "https://github.com/ori-halevi/RdpScopeToggler/releases";

            contextMenu.Items.Add("GitHub", null, (s, ea) =>
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = RepoApiUrl,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            });
            
            contextMenu.Items.Add(TranslationHelper.Translate("OpenWindow_translator"), null, (s, ea) => onOpenWindow?.Invoke());
            contextMenu.Items.Add(TranslationHelper.Translate("Exit_translator"), null, (s, ea) => onExit?.Invoke());

            notifyIcon.ContextMenuStrip = contextMenu;

            notifyIcon.MouseClick += (s, ea) =>
            {
                if (ea.Button == System.Windows.Forms.MouseButtons.Left)
                    onOpenWindow?.Invoke();
            };
        }

        public void ShowStillRunningWarning()
        {
            // הצגת בועת התראה קטנה מה־NotifyIcon
            notifyIcon.BalloonTipTitle = "Rdp Scope Toggler";
            notifyIcon.BalloonTipText = "התוכנה עדיין פועלת ברקע";
            notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            notifyIcon.ShowBalloonTip(500); // משך הזמן במילישניות
        }

        public void AttachWindow(Window window)
        {
            window.Closing += (sender, e) =>
            {
                ShowStillRunningWarning(); // הצגת הבועה
            };
        }


        public void Dispose()
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }
    }
}
