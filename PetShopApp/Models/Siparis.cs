// ============================================================
// PetShop Sipariş Yönetim Sistemi
// Models/Siparis.cs  -  Veri Modeli
// ============================================================

namespace PetShopApp.Models
{
    // Desteklenen hayvan türleri
    public enum HayvanTuru
    {
        Kedi = 0,
        Kopek = 1,
        Kus = 2,
        Balik = 3,
        Hamster = 4,
        Diger = 5
    }

    // Tek bir siparişi temsil eden sınıf
    public class Siparis
    {
        public int      SiparisId       { get; set; }
        public string   MusteriAd       { get; set; } = "";
        public string   MusteriSoyad    { get; set; } = "";
        public string   MusteriTelefon  { get; set; } = "";
        public HayvanTuru HayvanTuru    { get; set; }
        public string   MamaTuru        { get; set; } = "";
        public int      Miktar          { get; set; } = 1;
        public decimal  BirimFiyat      { get; set; }
        public DateTime SiparisTarihi   { get; set; } = DateTime.Today;
        public DateTime TeslimTarihi    { get; set; } = DateTime.Today;
        public bool     TeslimEdildi    { get; set; }
        public string   Notlar          { get; set; } = "";

        // Hesaplanan alanlar
        public decimal ToplamFiyat => Miktar * BirimFiyat;
        public string  TumAd       => $"{MusteriAd} {MusteriSoyad}".Trim();

        // Hayvan türünü emoji ile Türkçe olarak döndürür
        public string HayvanTuruTR => HayvanTuru switch
        {
            HayvanTuru.Kedi    => "🐱 Kedi",
            HayvanTuru.Kopek   => "🐶 Köpek",
            HayvanTuru.Kus     => "🐦 Kuş",
            HayvanTuru.Balik   => "🐟 Balık",
            HayvanTuru.Hamster => "🐹 Hamster",
            _                  => "🐾 Diğer"
        };
    }
}
