using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RdpScopeToggler.Models
{
    public class DialogButtonConfig
    {
        public string Text { get; set; }
        public Action? OnClick { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsCancel { get; set; } = false;
    }

    public class GenericDialogOptions
    {
        public string Title { get; set; } = "הודעה";
        public string Message { get; set; } = "";
        public ImageSource? Icon { get; set; }
        public bool IsModal { get; set; } = true;
        public bool Topmost { get; set; } = true;
        public bool LockOwnerWindow { get; set; } = true;
        public List<DialogButtonConfig> Buttons { get; set; } = new();
        public Action? OnClose { get; set; }
    }
}
