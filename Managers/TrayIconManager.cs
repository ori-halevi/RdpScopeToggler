using System;
using System.Windows;
using System.Drawing;

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

            contextMenu.Items.Add("פתח חלון", null, (s, ea) => onOpenWindow?.Invoke());
            contextMenu.Items.Add("יציאה", null, (s, ea) => onExit?.Invoke());

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
