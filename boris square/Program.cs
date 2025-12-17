using borissquare;
using System;
using System.Windows.Forms;

namespace FlashingSquare
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
#if NET6_0_OR_GREATER
            // New minimal startup helper on .NET 6+
            ApplicationConfiguration.Initialize();
#else
            // Fallback for older .NET versions where ApplicationConfiguration doesn't exist
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#endif
            Application.Run(new MainForm());
        }
    }
}