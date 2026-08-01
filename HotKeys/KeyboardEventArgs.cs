using System;

namespace SmartSystemMenu.HotKeys
{
    class KeyboardEventArgs : EventArgs
    {
        public int MenuItemId { get; }

        public bool NextMonitor { get; set; }

        public bool PreviousMonitor { get; set; }

        public bool SelectMonitor { get; set; }

        public bool Succeeded { get; set; }

        public KeyboardEventArgs()
        {
        }

        public KeyboardEventArgs(int menuItemId)
        {
            MenuItemId = menuItemId;
        }
    }
}
