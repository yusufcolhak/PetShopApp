// ============================================================
// PetShop Sipariş Yönetim Sistemi
// Program.cs  -  Giriş Noktası
// ============================================================

namespace PetShopApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Modern Windows görünümünü etkinleştir
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ApplicationConfiguration.Initialize();

            // Yüksek DPI desteği
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // Ana formu başlat
            Application.Run(new MainForm());
        }
    }
}