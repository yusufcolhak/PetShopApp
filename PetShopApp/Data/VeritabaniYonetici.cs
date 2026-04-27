// ============================================================
// PetShop Sipariş Yönetim Sistemi
// Data/VeritabaniYonetici.cs  -  SQLite Veri Katmanı
// ============================================================

using Microsoft.Data.Sqlite;
using PetShopApp.Models;

namespace PetShopApp.Data
{
    public class VeritabaniYonetici
    {
        // Veritabanı dosyası kullanıcının AppData klasöründe saklanır
        private static readonly string DbYolu = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PetShopApp", "petshop.db");

        private string BaglantiCumlesi => $"Data Source={DbYolu}";

        public VeritabaniYonetici()
        {
            // Klasör yoksa oluştur
            string? klasor = Path.GetDirectoryName(DbYolu);
            if (klasor != null && !Directory.Exists(klasor))
                Directory.CreateDirectory(klasor);

            TabloOlustur();
        }

        // İlk çalıştırmada tabloyu oluşturur
        private void TabloOlustur()
        {
            using var con = new SqliteConnection(BaglantiCumlesi);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Siparisler (
                    SiparisId      INTEGER PRIMARY KEY AUTOINCREMENT,
                    MusteriAd      TEXT    NOT NULL,
                    MusteriSoyad   TEXT    NOT NULL,
                    MusteriTelefon TEXT    DEFAULT '',
                    HayvanTuru     INTEGER NOT NULL,
                    MamaTuru       TEXT    NOT NULL,
                    Miktar         INTEGER NOT NULL DEFAULT 1,
                    BirimFiyat     REAL    NOT NULL DEFAULT 0,
                    SiparisTarihi  TEXT    NOT NULL,
                    TeslimTarihi   TEXT    NOT NULL,
                    TeslimEdildi   INTEGER NOT NULL DEFAULT 0,
                    Notlar         TEXT    DEFAULT ''
                )";
            cmd.ExecuteNonQuery();
        }

        // ── CRUD İŞLEMLERİ ──────────────────────────────────────

        public void SiparisEkle(Siparis s)
        {
            using var con = new SqliteConnection(BaglantiCumlesi);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Siparisler
                (MusteriAd, MusteriSoyad, MusteriTelefon, HayvanTuru, MamaTuru,
                 Miktar, BirimFiyat, SiparisTarihi, TeslimTarihi, TeslimEdildi, Notlar)
                VALUES (@ad, @soyad, @tel, @hayvan, @mama,
                        @miktar, @fiyat, @sipTar, @teslimTar, @teslim, @not)";
            PrmEkle(cmd, s);
            cmd.ExecuteNonQuery();
        }

        public void SiparisGuncelle(Siparis s)
        {
            using var con = new SqliteConnection(BaglantiCumlesi);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"
                UPDATE Siparisler SET
                    MusteriAd=@ad, MusteriSoyad=@soyad, MusteriTelefon=@tel,
                    HayvanTuru=@hayvan, MamaTuru=@mama, Miktar=@miktar,
                    BirimFiyat=@fiyat, SiparisTarihi=@sipTar, TeslimTarihi=@teslimTar,
                    TeslimEdildi=@teslim, Notlar=@not
                WHERE SiparisId=@id";
            cmd.Parameters.AddWithValue("@id", s.SiparisId);
            PrmEkle(cmd, s);
            cmd.ExecuteNonQuery();
        }

        public void SiparisSil(int siparisId)
        {
            using var con = new SqliteConnection(BaglantiCumlesi);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = "DELETE FROM Siparisler WHERE SiparisId=@id";
            cmd.Parameters.AddWithValue("@id", siparisId);
            cmd.ExecuteNonQuery();
        }

        // ── SORGULAR ────────────────────────────────────────────

        public List<Siparis> TumSiparisleriGetir()
            => Sorgula("SELECT * FROM Siparisler ORDER BY SiparisId DESC");

        // Bugünkü teslim tarihi olan siparişler (teslim edilmemiş önce)
        public List<Siparis> BugunkuTeslimatlar()
        {
            string bugun = DateTime.Today.ToString("yyyy-MM-dd");
            return Sorgula(
                $"SELECT * FROM Siparisler WHERE TeslimTarihi='{bugun}' ORDER BY TeslimEdildi ASC, SiparisId ASC");
        }

        // Aktif (Bekleyen) tüm siparişler ve bugünkü tüm teslimatlar
        public List<Siparis> AktifSiparisleriGetir()
        {
            string bugun = DateTime.Today.ToString("yyyy-MM-dd");
            return Sorgula(
                $"SELECT * FROM Siparisler WHERE TeslimEdildi=0 OR TeslimTarihi='{bugun}' ORDER BY TeslimEdildi ASC, TeslimTarihi ASC, SiparisId ASC");
        }

        // Belirli tarih aralığındaki siparişler
        public List<Siparis> TarihAraligiSorgu(DateTime baslangic, DateTime bitis)
            => Sorgula($@"
                SELECT * FROM Siparisler
                WHERE TeslimTarihi >= '{baslangic:yyyy-MM-dd}'
                  AND TeslimTarihi <= '{bitis:yyyy-MM-dd}'
                ORDER BY TeslimTarihi ASC, SiparisId ASC");

        // ── YARDIMCI METODLAR ───────────────────────────────────

        private static void PrmEkle(SqliteCommand cmd, Siparis s)
        {
            cmd.Parameters.AddWithValue("@ad",       s.MusteriAd);
            cmd.Parameters.AddWithValue("@soyad",    s.MusteriSoyad);
            cmd.Parameters.AddWithValue("@tel",      s.MusteriTelefon ?? "");
            cmd.Parameters.AddWithValue("@hayvan",   (int)s.HayvanTuru);
            cmd.Parameters.AddWithValue("@mama",     s.MamaTuru);
            cmd.Parameters.AddWithValue("@miktar",   s.Miktar);
            cmd.Parameters.AddWithValue("@fiyat",    (double)s.BirimFiyat);
            cmd.Parameters.AddWithValue("@sipTar",   s.SiparisTarihi.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@teslimTar",s.TeslimTarihi.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@teslim",   s.TeslimEdildi ? 1 : 0);
            cmd.Parameters.AddWithValue("@not",      s.Notlar ?? "");
        }

        private List<Siparis> Sorgula(string sql)
        {
            var liste = new List<Siparis>();
            using var con = new SqliteConnection(BaglantiCumlesi);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            using var okuyucu = cmd.ExecuteReader();

            while (okuyucu.Read())
            {
                liste.Add(new Siparis
                {
                    SiparisId      = okuyucu.GetInt32(0),
                    MusteriAd      = okuyucu.GetString(1),
                    MusteriSoyad   = okuyucu.GetString(2),
                    MusteriTelefon = okuyucu.IsDBNull(3) ? "" : okuyucu.GetString(3),
                    HayvanTuru     = (HayvanTuru)okuyucu.GetInt32(4),
                    MamaTuru       = okuyucu.GetString(5),
                    Miktar         = okuyucu.GetInt32(6),
                    BirimFiyat     = (decimal)okuyucu.GetDouble(7),
                    SiparisTarihi  = DateTime.ParseExact(okuyucu.GetString(8), "yyyy-MM-dd", null),
                    TeslimTarihi   = DateTime.ParseExact(okuyucu.GetString(9), "yyyy-MM-dd", null),
                    TeslimEdildi   = okuyucu.GetInt32(10) == 1,
                    Notlar         = okuyucu.IsDBNull(11) ? "" : okuyucu.GetString(11)
                });
            }
            return liste;
        }
    }
}
