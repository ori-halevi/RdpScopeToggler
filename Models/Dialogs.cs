using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace RdpScopeToggler.Models
{
    public class DialogButtonConfig
    {
        public string Text { get; set; }
        public Action? OnClick { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsCancel { get; set; } = false;

        // Optional resource key of a Style (from Application.Current.Resources) to apply to
        // the button. Lets callers pick the existing app styles (ConnectButton, SimpleButton,
        // DisconnectButton, …) per-button without teaching the dialog about any of them.
        public string? StyleKey { get; set; }
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
