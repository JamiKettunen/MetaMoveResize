using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
//using System.Media;

using MetaMoveResize.Properties;
using MouseKeyboardLibrary;

namespace MetaMoveResize
{
    public partial class MainForm : Form
    {
        #region DllImports

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect { public int Left; public int Top; public int Right; public int Bottom; }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool enable);
        
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
        
        // Cursors

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CopyIcon(IntPtr hcur);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(int uiAction, int uiParam, IntPtr pvParam, int fWinIni);

        #endregion

        #region Constants

        //private const int   GWL_EXSTYLE = -20;
        private const int  GWL_STYLE = -16;

        // Styles
        private const long WS_BORDER  = 0x00800000L;
        private const long WS_SIZEBOX = 0x00040000L;
        private const long WS_TILED   = 0x00000000L;

        private const int SPI_SETCURSORS = 0x57;

        // System cursor handle references (uint)
        private const uint OCR_NORMAL   = 32512;
        private const uint OCR_CROSS    = 32515;
        private const uint OCR_SIZENS   = 32645;
        private const uint OCR_SIZEWE   = 32644;
        private const uint OCR_SIZEALL  = 32646;
        private const uint OCR_SIZENESW = 32643;
        private const uint OCR_SIZENWSE = 32642;

        #endregion

        private bool _windowActive = true;  // Is this Application focused?
        private int  _metaKeyVal   = 164;   // Meta key value
        private bool _metaDown     = false; // Is the meta key held down?
        private bool _dragging     = false; // Are we dragging the active Window?
        private bool _resizing     = false; // Are we resizing the active Window?
        private int  _anchorX      = 0;     // MouseX anchor point for dragging & resizing
        private int  _anchorY      = 0;     // MouseY anchor point for dragging & resizing
        
        private IntPtr _fgHwnd   = IntPtr.Zero; // Foreground window handle
        private int _fgSizeW     = 0;
        private int _fgLocationX = 0;
        private int _fgSizeH     = 0;
        private int _fgLocationY = 0;
        private int _fgResizeCorner = 4; // 1 = TopLeft, 2 = TopRight, 3 = BottomLeft, 4 = BottomRight

        private MouseHook _mouseHook = new MouseHook();
        private KeyboardHook _keyboardHook = new KeyboardHook();

        #region Custom functions for DllImports

        private string GetWindowClassByHwnd(IntPtr hWnd)
        {
            try
            {
                StringBuilder lpClassName = new StringBuilder(256);
                int nRet = GetClassName(hWnd, lpClassName, 256);
                if (nRet < 1) return "";
                return lpClassName.ToString();
            }
            catch { return ""; }
        }

        /*private IntPtr GetParentRecursive(IntPtr hWnd)
        {
            IntPtr lastParent = hWnd;

            for(short i = 0; i < short.MaxValue; i++)
            {
                IntPtr tmp = GetParent(lastParent);
                if (tmp == IntPtr.Zero) break;
            }

            return lastParent;
        }*/

        #endregion
        
        public MainForm()
        {
            _keyboardHook.KeyDown += new KeyEventHandler(KeyboardHook_KeyDown);
            _keyboardHook.KeyUp += new KeyEventHandler(KeyboardHook_KeyUp);
            _keyboardHook.Start();

            _mouseHook.MouseDown += new MouseEventHandler(MouseHook_MouseDown);
            _mouseHook.MouseMove += new MouseEventHandler(MouseHook_MouseMove);
            _mouseHook.MouseUp += new MouseEventHandler(MouseHook_MouseUp);
            _mouseHook.Start();
            
            InitializeComponent();
        }

        #region KB Hook

        private void KeyboardHook_KeyDown(object sender, KeyEventArgs e)
        {
            // Is this Application focused & waiting for MetaKey?
            if (_windowActive && btnSetMetaKey.Text == "[WAITING]")
            {
                // Stop changing MetaKey
                if (e.KeyCode == Keys.Escape) { btnSetMetaKey.Text = Convert.ToString(btnSetMetaKey.Tag); }
                // Set new MetaKey
                else
                {
                    _metaKeyVal = e.KeyValue;
                    btnSetMetaKey.Tag = _metaKeyVal;
                    btnSetMetaKey.Text = Convert.ToString(btnSetMetaKey.Tag);
                    this.ActiveControl = lblMetaKey;
                }

                _metaDown = false;
            }
            // Not waiting for new MetaKey => Check if MetaKey down... TODO: Improve
            else
            {
                if (e.KeyValue == _metaKeyVal)
                    _metaDown = true;
            }
        }

        private void KeyboardHook_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyValue == _metaKeyVal)
                _metaDown = false;
        }

        #endregion

        #region Mouse Hook
        
        private void MouseHook_MouseMove(object sender, MouseEventArgs e)
        {
            if ( (!_metaDown || (!_dragging && !_resizing) ) || _fgHwnd == IntPtr.Zero ) return; // Don't execute if not dragging or resizing

            //Debug.WriteLine("Moving | Drag: " + _dragging + ", Resize: " + _resizing + ", Anchor { X: " + _anchorX + ", Y: " + _anchorY + " }");
            //Debug.WriteLine("Window { W: " + _fgSizeW + ", H: " + _fgSizeH + ", X: " + _fgLocationX + ", Y: " + _fgLocationY + " }");

            int mouseX = MouseSimulator.X;
            int mouseY = MouseSimulator.Y;

            //Debug.WriteLine("Anchor { X: " + _anchorX + ", Y: " + _anchorY + " } | Mouse { X: " + mouseX + ", Y: " + mouseY + " }");

            int dx = (mouseX - _anchorX);
            int yx = (mouseY - _anchorY);

            if (_dragging)
            {
                MoveWindow(_fgHwnd, _fgLocationX + dx, _fgLocationY + yx, _fgSizeW, _fgSizeH, false);
            }
            else if (_resizing)
            {
                if (_fgResizeCorner == 4) // Bottom-Right
                {
                    MoveWindow(_fgHwnd, _fgLocationX, _fgLocationY, _fgSizeW + dx, _fgSizeH + yx, true);
                }
                else if (_fgResizeCorner == 3) // Bottom-Left
                {
                    MoveWindow(_fgHwnd, _fgLocationX + dx, _fgLocationY, _fgSizeW - dx, _fgSizeH + yx, true);
                }
                else if (_fgResizeCorner == 1) // Top-Left
                {
                    MoveWindow(_fgHwnd, _fgLocationX + dx, _fgLocationY + yx, _fgSizeW - dx, _fgSizeH - yx, true);
                }
                else if (_fgResizeCorner == 2) // Top-Right
                {
                    MoveWindow(_fgHwnd, _fgLocationX, _fgLocationY + yx, _fgSizeW + dx, _fgSizeH - yx, true);
                }
            }
        }

        private bool _m1Down = false;
        private bool _m2Down = false;

        // "Shell_TrayWnd", "Windows.UI.Core.CoreWindow", "MultitaskingViewFrame"
        private List<string> _ignoreClasses = new List<string>(new string[] { "MultitaskingViewFrame", "Shell_TrayWnd", "NotifyIconOverflowWindow", "Windows.UI.Core.CoreWindow" });

        private void MouseHook_MouseDown(object sender, MouseEventArgs e)
        {
            _m1Down = (e.Button == MouseButtons.Left);
            _m2Down = (e.Button == MouseButtons.Right);

            // TOOD Use GetWindowPlacement to avoid multiple P/Invoke calls?
            if (_metaDown && (_m1Down || _m2Down)) // Moving (_metaDown && e.Button == MouseButtons.Left)
            {
                _fgHwnd = IntPtr.Zero; // Keep as Zero till sizing & everything else has been figured out
                IntPtr tmpHandle = GetForegroundWindow();

                if (tmpHandle != IntPtr.Zero)
                {
                    // Check if non-Windows hWnd...
                    string hWndClass = GetWindowClassByHwnd(tmpHandle);
                    if (_ignoreClasses.Contains(hWndClass)) return;
                    /*if (hWndClass == "Windows.UI.Core.CoreWindow") // Check against explorer process
                    {
                        // TODO Check if hWnd part of explorer.exe process handles
                    }*/
                    
                    _anchorX = MouseSimulator.X;
                    _anchorY = MouseSimulator.Y;

                    if (_m2Down) // Resizing
                    {
                        long style = GetWindowLongPtr(tmpHandle, GWL_STYLE).ToInt64();
                        _resizing = (style & WS_SIZEBOX) == WS_SIZEBOX || !((style & WS_BORDER) == WS_BORDER); // TODO Fix "|| !((style & WS_BORDER) == WS_BORDER)"
                        Debug.WriteLine("Resizeable: " + _resizing);

                        if (!_resizing) return; // Fixed size Window => DO NOT RESIZE!
                        //SystemSounds.Exclamation.Play();
                        
                        _fgResizeCorner = (_anchorY - _fgLocationY < _fgSizeH / 2) ? 1 : 3;
                        _fgResizeCorner += (_anchorX - _fgLocationX < _fgSizeW / 2) ? 0 : 1;
                        
                        // Set resize cursor to match resizing corner
                        uint cur = (_fgResizeCorner == 2 || _fgResizeCorner == 3) ? OCR_SIZENESW : OCR_SIZENWSE;
                        SetSystemCursor(CopyIcon(LoadCursor(IntPtr.Zero, (int)cur)), OCR_NORMAL);  // 1: Replacement, 2: Target
                    }
                    else // Moving
                    {
                        // TODO Add checks here to make sure Window should be draggable?
                        _dragging = true;
                        Debug.WriteLine("Draggable: " + _dragging);
                        if (!_dragging) return;
                        
                        // Set movement cursor
                        SetSystemCursor(CopyIcon(LoadCursor(IntPtr.Zero, (int)OCR_SIZEALL)), OCR_NORMAL);  // 1: Replacement, 2: Target
                    }

                    Debug.WriteLine((_m2Down ? "Resizi" : "Draggi") + "ng hWnd '" + tmpHandle + "', Class = '" + hWndClass + "'");
                    
                    EnableWindow(tmpHandle, false); // Disable window to not handle unwanted events while resizing / dragging; TODO Supress "Ding!" sound
                    UpdateWindowRect(tmpHandle);
                    _fgHwnd = tmpHandle;
                }
            }
        }

        private void MouseHook_MouseUp(object sender, MouseEventArgs e)
        {
            _m1Down = !(e.Button == MouseButtons.Left);
            _m2Down = !(e.Button == MouseButtons.Right);
            
            if (!_m1Down || !_m2Down) // _metaDown &&
            {
                // Reset cursor back to normal
                SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, 0); // Reload all system cursors

                if (!_m2Down) _resizing = false; // Not resizing any more
                else _dragging = false;          // Not moving any more

                UpdateWindowRect(_fgHwnd);

                if (_fgHwnd != IntPtr.Zero) EnableWindow(_fgHwnd, true);
            }
        }

        #endregion

        #region App UI

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Load last MetaKeyVal
            _metaKeyVal = Settings.Default.MetaKeyVal;
            btnSetMetaKey.Tag = _metaKeyVal;
            btnSetMetaKey.Text = Convert.ToString(btnSetMetaKey.Tag);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, 0); // Reload all system cursors

            // Save current MetaKeyVal
            if(Settings.Default.MetaKeyVal != _metaKeyVal)
            {
                Settings.Default.MetaKeyVal = _metaKeyVal;
                Settings.Default.Save();
            }
        }

        private void btnSetMetaKey_Click(object sender, EventArgs e)
        {
            btnSetMetaKey.Tag = btnSetMetaKey.Text;
            btnSetMetaKey.Text = "[WAITING]";
        }
        
        #endregion

        private void MainForm_Activated(object sender, EventArgs e)
        {
            _windowActive = true;
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            _windowActive = false;
        }

        private void UpdateWindowRect(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return;

            GetWindowRect(hWnd, out Rect r);
            _fgLocationY = r.Top;
            _fgLocationX = r.Left;
            _fgSizeH = r.Bottom - r.Top;
            _fgSizeW = r.Right - r.Left;
        }
    }
}
