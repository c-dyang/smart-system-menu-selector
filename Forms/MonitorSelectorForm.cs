using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SmartSystemMenu.Native.Structs;

using DrawingPoint = System.Drawing.Point;

namespace SmartSystemMenu.Forms
{
    /// <summary>
    /// 显示器选择器：鼠标位置弹出小窗，按物理布局绘制各显示器色块+编号，
    /// 点击目标屏后返回所选显示器句柄。
    /// </summary>
    public class MonitorSelectorForm : Form
    {
        private IntPtr _selectedMonitor;

        public MonitorSelectorForm(IList<IntPtr> monitorHandles, IntPtr currentMonitor)
        {
            _selectedMonitor = currentMonitor;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            BackColor = Color.FromArgb(30, 30, 30);

            // 收集显示器几何信息（复用项目已有 MonitorInfo）
            var bounds = new List<Rectangle>();
            var primaries = new List<bool>();
            foreach (var h in monitorHandles)
            {
                var info = new MonitorInfo();
                info.Init();
                if (GetMonitorInfo(h, ref info))
                {
                    bounds.Add(Rectangle.FromLTRB(info.rcMonitor.Left, info.rcMonitor.Top,
                        info.rcMonitor.Right, info.rcMonitor.Bottom));
                    primaries.Add((info.dwFlags & MONITORINFOF_PRIMARY) != 0);
                }
            }
            if (bounds.Count == 0) return;

            // 虚拟桌面边界
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var b in bounds)
            {
                minX = Math.Min(minX, b.Left); minY = Math.Min(minY, b.Top);
                maxX = Math.Max(maxX, b.Right); maxY = Math.Max(maxY, b.Bottom);
            }
            int vw = maxX - minX, vh = maxY - minY;
            if (vw <= 0 || vh <= 0) return;

            // 指示器尺寸（限最大 340x240）
            double scale = Math.Min(340.0 / vw, 240.0 / vh);
            int iw = Math.Max(80, (int)(vw * scale));
            int ih = Math.Max(60, (int)(vh * scale));

            // 鼠标位置弹出（右下偏移，超屏回退）
            var cursor = Cursor.Position;
            int locX = cursor.X + 20, locY = cursor.Y + 20;
            var wa = Screen.FromPoint(cursor).WorkingArea;
            if (locX + iw > wa.Right) locX = cursor.X - iw - 20;
            if (locY + ih > wa.Bottom) locY = cursor.Y - ih - 20;
            if (locX < wa.Left) locX = wa.Left + 8;
            if (locY < wa.Top) locY = wa.Top + 8;
            Location = new DrawingPoint(locX, locY);
            ClientSize = new Size(iw, ih);

            // 画显示器色块
            const int margin = 8;
            double k = Math.Min((iw - margin * 2.0) / vw, (ih - margin * 2.0) / vh);
            double ox = margin + ((iw - margin * 2.0) - vw * k) / 2;
            double oy = margin + ((ih - margin * 2.0) - vh * k) / 2;

            for (int i = 0; i < monitorHandles.Count; i++)
            {
                var handle = monitorHandles[i];
                var b = bounds[i];
                var panel = new Panel
                {
                    Left = (int)(ox + (b.Left - minX) * k),
                    Top = (int)(oy + (b.Top - minY) * k),
                    Width = (int)(b.Width * k),
                    Height = (int)(b.Height * k),
                    BackColor = primaries[i] ? Color.FromArgb(200, 0, 120, 220) : Color.FromArgb(200, 0, 180, 110),
                    Tag = handle
                };
                int idx = i + 1;
                panel.Paint += (s, e) =>
                {
                    var p = (Panel)s;
                    e.Graphics.DrawRectangle(Pens.White, 0, 0, p.Width - 1, p.Height - 1);
                    using (var font = new Font("Segoe UI", Math.Max(14f, p.Height * 0.35f), FontStyle.Bold))
                    {
                        var sz = e.Graphics.MeasureString(idx.ToString(), font);
                        e.Graphics.DrawString(idx.ToString(), font, Brushes.White,
                            (p.Width - sz.Width) / 2, (p.Height - sz.Height) / 2);
                    }
                };
                panel.MouseClick += (s, e) =>
                {
                    _selectedMonitor = (IntPtr)((Panel)s).Tag;
                    DialogResult = DialogResult.OK;
                    Close();
                };
                Controls.Add(panel);
            }

            // Esc / 点空白取消
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); } };
            MouseClick += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        }

        public IntPtr SelectedMonitorHandle { get { return _selectedMonitor; } }

        #region Win32

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        private const uint MONITORINFOF_PRIMARY = 0x1;

        #endregion
    }
}
