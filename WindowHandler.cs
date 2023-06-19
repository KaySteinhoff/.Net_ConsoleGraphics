using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ConsoleGraphics
{
    public class WindowHandler
    {
        #region Imports
        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            int dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSecurityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetCurrentConsoleFont(
            IntPtr hConsoleOutput,
            bool bMaximumWindow,
            [Out][MarshalAs(UnmanagedType.LPStruct)] ConsoleFontInfo lpConsoleCurrentFont);

        [StructLayout(LayoutKind.Sequential)]
        internal class ConsoleFontInfo
        {
            internal int nFont;
            internal Coord dwFontSize;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Coord
        {
            [FieldOffset(0)]
            internal short X;
            [FieldOffset(2)]
            internal short Y;
        }

        private const int GENERIC_READ = unchecked((int)0x80000000);
        private const int GENERIC_WRITE = 0x40000000;
        private const int FILE_SHARE_READ = 1;
        private const int FILE_SHARE_WRITE = 2;
        private const int INVALID_HANDLE_VALUE = -1;
        private const int OPEN_EXISTING = 3;

        #region Window Menu
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_SIZE = 0xF000;
        #endregion

        #region Disable QuickEdit
        const uint ENABLE_QUICK_EDIT = 0x0040;

        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        #endregion

        private static Size GetConsoleFontSize()
        {
            // getting the console out buffer handle
            IntPtr outHandle = CreateFile("CONOUT$", GENERIC_READ | GENERIC_WRITE,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                0,
                IntPtr.Zero);
            int errorCode = Marshal.GetLastWin32Error();
            if (outHandle.ToInt32() == INVALID_HANDLE_VALUE)
            {
                throw new IOException("Unable to open CONOUT$", errorCode);
            }

            ConsoleFontInfo cfi = new ConsoleFontInfo();
            if (!GetCurrentConsoleFont(outHandle, false, cfi))
            {
                throw new InvalidOperationException("Unable to get font information.");
            }

            return new Size(cfi.dwFontSize.X, cfi.dwFontSize.Y);
        }
        #endregion

        float fontRatio = 0;
        Image img;
        Graphics buffer;

        #region Properties
        public Color ClearColor { get; set; }
        public float Ratio
        {
            get
            {
                return fontRatio;
            }
        }

        public Graphics Buffer
        {
            get
            {
                return buffer;
            }
        }
        #endregion

        #region Constructor
        public WindowHandler(int width, int height, Color clearColor)
        {
            //Setup Variables for constructor
            Size s = GetConsoleFontSize();
            IntPtr window = GetConsoleWindow();

            //Set class variables
            fontRatio = s.Height / s.Width;
            img = new Bitmap(width, height);
            buffer = Graphics.FromImage(img);
            this.ClearColor = clearColor;

            //Wipe Window to ClearColor
            Graphics g = Graphics.FromHwnd(window);
            buffer.Clear(ClearColor);
            g.DrawImage(img, 0, 0);

            //Resize window
            Console.SetWindowSize(width/s.Width, height/s.Height);
            Console.BufferWidth = width / s.Width;
            Console.BufferHeight = height / s.Height;
            Console.SetWindowSize(width/s.Width, height/s.Height);

            DisableQuickEdit();

            //Disable maximizing
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MAXIMIZE, MF_BYCOMMAND | 0x00000001);
        }
        #endregion

        #region Internal Functions
        bool EnableQuickEdit()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                // ERROR: Unable to get console mode.
                return false;
            }

            consoleMode &= ENABLE_QUICK_EDIT;

            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode
                return false;
            }

            return true;
        }

        bool DisableQuickEdit()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                // ERROR: Unable to get console mode.
                return false;
            }

            consoleMode &= ~ENABLE_QUICK_EDIT;

            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode
                return false;
            }

            return true;
        }

        #endregion

        #region Public Functions
        public void DrawImage(Image img, Point location, Size dimensions)
        {
            Rectangle imgRect = new Rectangle(
                location.X,
                location.Y,
                dimensions.Width,
                dimensions.Height);
            buffer.DrawImage(img, imgRect);
        }

        public void DrawLine(Point p1, Point p2, Color color)
        {
            buffer.DrawLine(new Pen(new SolidBrush(color)), p1, p2);
        }

        public void DrawTriangle(Point p1, Point p2, Point p3, Color color, bool fill = true)
        {
            if (!fill)
                buffer.DrawLines(new Pen(new SolidBrush(color)), new Point[] { p1, p2, p3, p1 });
            else
                buffer.FillPolygon(new SolidBrush(color), new Point[] { p1, p2, p3, p1 });
        }

        public void DrawEllipse(Point location, int width, int height, Color color, bool fill = true)
        {
            if(!fill)
                buffer.DrawEllipse(new Pen(new SolidBrush(color)), new Rectangle(location, new Size(width, height)));
            else
                buffer.FillEllipse(new SolidBrush(color), new Rectangle(location, new Size(width, height)));
        }

        public void DrawCircle(Point location, int radius, Color color, bool fill = true)
        {
            if(!fill)
                buffer.DrawEllipse(new Pen(new SolidBrush(color)), new Rectangle(location, new Size(radius*2, radius*2)));
            else
                buffer.FillEllipse(new SolidBrush(color), new Rectangle(location, new Size(radius*2, radius*2)));
        }

        public void Render()
        {
            Graphics render = Graphics.FromHwnd(GetConsoleWindow());
            render.DrawImage(img, 0, 0);
            buffer.Clear(ClearColor);
        }
        #endregion

        public void Destroy()
        {
            EnableQuickEdit();
        }

    }
}
