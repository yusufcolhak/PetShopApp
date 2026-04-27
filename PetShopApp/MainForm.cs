
// PetShop Sipariş Yönetim Sistemi
// MainForm.cs  -  Ana Pencere (Bugünkü Teslimatlar Dashboard)

using PetShopApp.Data; // Veritabanı işlemleri
using PetShopApp.Forms; // Kullanıcı arayüzü
using PetShopApp.Models; // Sipariş gibi veri yapılar için

namespace PetShopApp
{
    public partial class MainForm : Form
    {
        // Veritabanı işlemlerini yönetmesi için 
        private readonly VeritabaniYonetici _db = new();

        // ── RENK PALETİ ─────────────────────────────────────────
        // "static readonly" = tüm Form örnekleri bu renkleri paylaşır, bellekte tek kopya tutulur.
        // Color.FromArgb(R, G, B) = Kırmızı/Yeşil/Mavi değerleriyle özel renk oluşturur (0-255 arası).
        private static readonly Color C_BG = Color.FromArgb(15, 27, 45);   // Koyu lacivert arka plan
        private static readonly Color C_PANEL = Color.FromArgb(10, 20, 35);   // Daha koyu panel
        private static readonly Color C_SIDEBAR = Color.FromArgb(17, 29, 48);   // Kenar çubuğu
        private static readonly Color C_CARD = Color.FromArgb(26, 41, 66);   // Kart arka planı
        private static readonly Color C_ACCENT = Color.FromArgb(79, 195, 247); // Mavi vurgu rengi
        private static readonly Color C_SUCCESS = Color.FromArgb(102, 187, 106);// Yeşil (başarı)
        private static readonly Color C_WARNING = Color.FromArgb(255, 167, 38); // Turuncu (uyarı)
        private static readonly Color C_PURPLE = Color.FromArgb(206, 147, 216);// Mor (ciro)
        private static readonly Color C_DANGER = Color.FromArgb(239, 83, 80);  // Kırmızı (tehlike)
        private static readonly Color C_TEXT = Color.FromArgb(236, 239, 241);// Ana yazı rengi (açık gri)
        private static readonly Color C_SUB = Color.FromArgb(144, 164, 174);// İkincil yazı (daha soluk)

        // ── KONTROLLER ───
        private DataGridView dgvBugun = null!; // Bugünkü siparişleri gösteren tablo
        private Panel pnlStatParent = null!; // İstatistik kartlarını tutan kapsayıcı panel
        private Label lblStatToplam = null!; // Toplam sipariş sayısı etiketi
        private Label lblStatBekleyen = null!; // Bekleyen sipariş sayısı etiketi
        private Label lblStatTeslim = null!; // Teslim edilen sipariş sayısı etiketi
        private Label lblStatCiro = null!; // Günlük ciro etiketi
        private Label lblSectionDate = null!; // Bölüm başlığındaki tarih etiketi
        private string _aktifFiltre = "Tümü"; // Şu an seçili filtre ("Tümü" / "Bekleyen" / "Teslim")
        private bool _isUpdating = false;  // Veri güncellenirken olayların tekrar tetiklenmesini önler

        public MainForm()
        {
            InitializeComponent(); // Designer tarafından otomatik üretilen kod çalışır (varsa)
            ArayuzOlustur();       // Tüm UI bileşenlerini kod ile inşa et
            BugunkuTeslimatlariYukle(); // Uygulama açılırken ilk veriyi çek        Veritabanından veri çekilir / Grid’e satırlar eklenir /Teslim durumuna göre sayılar hesaplanır
        }                                                                                                         // Ciro hesaplanır
                                                                                 // Teslim edilen siparişler gri renkte gösterilerek kullanıcıya görsel fark sunuluyor.

        //  ARAYÜZ OLUŞTURMA  
        private void ArayuzOlustur()
        {
            // Form'un temel özellikleri ayarlanıyor
            Text = "🐾 PetShop Sipariş Yönetim Sistemi"; // Başlık çubuğu yazısı
            Size = new Size(1280, 760);  // Başlangıç pencere boyutu
            MinimumSize = new Size(1024, 640);  // Kullanıcı pencereyi bu boyutun altına küçültemez
            StartPosition = FormStartPosition.CenterScreen; // Form ekranın ortasında açılır
            BackColor = C_BG;   // Form arka plan rengi
            ForeColor = C_TEXT; // Varsayılan yazı rengi 
            Font = new Font("Segoe UI", 9.5f); // Tüm forma uygulanacak varsayılan font

            // ── HEADER ───   
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = C_PANEL
            };

            // Uygulama başlığı etiketi - AutoSize = boyutu metne göre otomatik ayarlar
            pnlHeader.Controls.Add(new Label
            {
                Text = "🐾  PetShop Sipariş Yönetim Sistemi",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = C_ACCENT,
                AutoSize = true,
                Location = new Point(22, 14)
            });

            var lblTarih = new Label
            {
                // DateTime.Now.ToString ile tarih Türkçe formatlanır: "Pazartesi, 27 Nisan 2026"
                Text = DateTime.Now.ToString("dddd, dd MMMM yyyy",
                new System.Globalization.CultureInfo("tr-TR")),
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = C_SUB,
                AutoSize = true
            };

            // Header yeniden boyutlandığında tarih etiketi sağa yaslanır.
            pnlHeader.Resize += (s, e) =>
                lblTarih.Location = new Point(pnlHeader.Width - lblTarih.Width - 20, 24);
            pnlHeader.Controls.Add(lblTarih);

            // ── SIDEBAR ─────────────────────────────────────────
            // Dock = DockStyle.Left: Panel formun sol kenarına yapışır
            var pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 225,
                BackColor = C_SIDEBAR
            };

            // Sidebar'ın en üstündeki logo alanı
            var pnlLogo = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.FromArgb(8, 16, 28) };
            pnlLogo.Controls.Add(new Label
            {
                Text = "🏪  PetShop",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = C_TEXT,
                Dock = DockStyle.Fill,          // Label, panelin tamamını kaplar
                TextAlign = ContentAlignment.MiddleCenter // Yazı panelin tam ortasında
            });
            pnlSidebar.Controls.Add(pnlLogo);

            var navItems = new (string İkon, string Etiket, string Tag)[]
            {
                ("🏠", "Ana Sayfa",      "ana"),
                ("➕", "Yeni Sipariş",  "yeni"),
                ("📋", "Tüm Siparişler","liste"),
                ("📊", "Satış Raporu",  "rapor"),
            };

            int ny = 80; 
            foreach (var (ikon, etiket, tag) in navItems)
            {
                bool aktif = tag == "ana"; // Ana sayfa başlangıçta aktif (seçili) görünür

                var btn = new Button
                {
                    Text = $"  {ikon}  {etiket}", 
                    Tag = tag,
                    Size = new Size(200, 46),
                    Location = new Point(12, ny),
                    FlatStyle = FlatStyle.Flat, 
                    
                    BackColor = aktif ? Color.FromArgb(79, 195, 247, 30) : Color.Transparent,
                    ForeColor = aktif ? C_ACCENT : C_TEXT,
                    Font = new Font("Segoe UI", 10.5f, aktif ? FontStyle.Bold : FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(12, 0, 0, 0), // Sol iç boşluk 
                    Cursor = Cursors.Hand 
                };
                btn.FlatAppearance.BorderSize = 0; // Buton çerçevesini kaldırır
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 79, 195, 247); 
                btn.Click += NavButon_Click; // Tüm nav butonları aynı Click metodunu kullanır
                pnlSidebar.Controls.Add(btn);
                ny += 52; 
            }

            // Sidebar'ın en altına sabitlenmiş versiyon etiketi
            pnlSidebar.Controls.Add(new Label
            {
                Text = "v1.0  ©2026 PetShop",
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(70, 100, 130),
                Dock = DockStyle.Bottom, 
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter
            });

            // ── İÇERİK PANELİ (CONTENT)
            // DockStyle.Fill: Kalan tüm alanı kaplar (Header ve Sidebar'dan sonra kalan alan)
            var pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_BG,
                Padding = new Padding(20, 16, 20, 16) // İç kenar boşlukları (sol, üst, sağ, alt)
            };

            // İSTATİSTİK KARTLARI (Toplam / Bekleyen / Teslim / Ciro)
            pnlStatParent = new Panel
            {
                Dock = DockStyle.Top,
                Height = 96,
                BackColor = Color.Transparent
            };
            BekleStatCards(); // Kartları oluşturur ve label referanslarını (lblStatToplam vb.) doldurur
            pnlContent.Controls.Add(pnlStatParent);

            // BÖLÜM BAŞLIĞI (tarih + yenile butonu)
            var pnlSecHead = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.Transparent };
            lblSectionDate = new Label
            {
                Text = $"📦  Aktif Siparişler  ·  " +
                            DateTime.Today.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = C_TEXT,
                AutoSize = true,
                Location = new Point(0, 12)
            };

            var btnYenile = new Button
            {
                Text = "🔄  Yenile",
                Size = new Size(110, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = C_CARD,
                ForeColor = C_ACCENT,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                Location = new Point(820, 8)
            };
            btnYenile.FlatAppearance.BorderColor = C_ACCENT;
            btnYenile.FlatAppearance.BorderSize = 1;
            btnYenile.Click += (s, e) => BugunkuTeslimatlariYukle();

            pnlSecHead.Controls.Add(lblSectionDate);
            pnlSecHead.Controls.Add(btnYenile);

            // BUGÜNKÜ TESLİMAT GRID                                                                GRID YAPISI---- KOYU TEMA  -- Alternatif Satır Renkleri 
            dgvBugun = YapGrid();                                                                                  // Full satır seçimi -- Sabit yükseklik
            // Checkbox değiştiğinde (teslim edildi işaretlenince) veritabanını günceller                           Bu sayede daha modern bir görünüm elde edildi.
            dgvBugun.CellValueChanged += DgvBugun_CellValueChanged;
            // Checkbox'a tıklanınca hemen commit et, focus kaybolmasını beklememeni sağlıyor
            dgvBugun.CurrentCellDirtyStateChanged += DgvBugun_CurrentCellDirtyStateChanged;

            
            pnlContent.Controls.Add(dgvBugun);      
            pnlContent.Controls.Add(pnlSecHead);    
            pnlContent.Controls.Add(pnlStatParent); 

            // Form'a tüm ana paneller eklenir
            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);
            Controls.Add(pnlHeader);

            pnlHeader.SendToBack();
            pnlSidebar.SendToBack();
            pnlContent.BringToFront();

            BugunkuTeslimatlariYukle();
        }

        // İstatistik kartlarını baştan oluşturur.
        // Filtre değişince veya veri güncellenince yeniden çağrılır,
        // seçili kartın arka planı renklendirilerek aktif filtre vurgulanır.
        private void BekleStatCards()
        {
            pnlStatParent.Controls.Clear(); // Önceki kartları temizle

            var kartlar = new (string Baslik, Color Renk, Action<Label> Assign, string Filtre)[]
            {
                ("📦 Aktif Siparişler", C_ACCENT,  lbl => lblStatToplam   = lbl, "Tümü"),
                ("⏳ Bekleyen",          C_WARNING, lbl => lblStatBekleyen = lbl, "Bekleyen"),
                ("✅ Teslim Edildi",    C_SUCCESS, lbl => lblStatTeslim   = lbl, "Teslim"),
                ("💰 Günlük Ciro",      C_PURPLE,  lbl => lblStatCiro     = lbl, "") // Filtrelenemez
            };

            int x = 0; 
            foreach (var (baslik, renk, assign, filtre) in kartlar)
            {
                var pnl = new Panel
                {
                    Location = new Point(x, 0),
                    Size = new Size(248, 84),
                    BackColor = C_CARD,
                    Cursor = string.IsNullOrEmpty(filtre) ? Cursors.Default : Cursors.Hand
                };

                var pnlRenk = new Panel { Width = 4, Dock = DockStyle.Left, BackColor = renk };
                pnl.Controls.Add(pnlRenk);

                // Kartın üst kısmındaki küçük başlık ("📦 Aktif Siparişler" gibi)
                var lblBaslik = new Label
                {
                    Text = baslik,
                    Dock = DockStyle.Top,
                    Height = 26,
                    ForeColor = C_SUB,
                    Font = new Font("Segoe UI", 8.5f),
                    Padding = new Padding(12, 6, 0, 0)
                };
                pnl.Controls.Add(lblBaslik);

                // Büyük sayı/değer etiketi 
                var lblDeger = new Label
                {
                    Text = "—", // Veri yüklenene kadar tire gösterilir
                    Dock = DockStyle.Fill,
                    ForeColor = renk,
                    Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                    Padding = new Padding(12, 0, 0, 0),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                pnl.Controls.Add(lblDeger);
                assign(lblDeger); 

                // Aktif filtreyle eşleşen kartın arka planını hafifçe renklendir (seçili görünümü)
                if (_aktifFiltre == filtre)
                {
                    pnl.BackColor = Color.FromArgb(40, renk.R, renk.G, renk.B);
                }

                // Karta tıklanınca filtreyi değiştir ve listeyi güncelle.
                // EventHandler: (object sender, EventArgs e) imzalı delege tipi.
                // Aynı handler'ı panel ve tüm alt kontrollerine bağlıyoruz,
                // çünkü alt kontroller tıklamayı üst panele iletmeyebilir.
                if (!string.IsNullOrEmpty(filtre))
                {
                    EventHandler tikla = (s, e) =>
                    {
                        _aktifFiltre = filtre;    // Seçili filtreyi güncelle
                        BekleStatCards();          // Kartları yeniden çiz (aktif vurgusu için)
                        BugunkuTeslimatlariYukle();// Grid'i yeni filtreyle yenile
                    };
                    pnl.Click += tikla;
                    pnlRenk.Click += tikla;
                    lblBaslik.Click += tikla;
                    lblDeger.Click += tikla;
                    lblBaslik.Cursor = Cursors.Hand;
                    lblDeger.Cursor = Cursors.Hand;
                }

                pnlStatParent.Controls.Add(pnl);
                x += 262; 
            }
        }

        //  VERİ YÜKLEMESİ

        private void BugunkuTeslimatlariYukle()
        {
            // _isUpdating bayrağı: Bu metod çalışırken CellValueChanged gibi olaylar
            // tetiklenip metodu tekrar çağırırsa sonsuz döngü oluşur. Bayrak bunu önler.
            if (_isUpdating) return;
            _isUpdating = true;

            try
            {
                var liste = _db.AktifSiparisleriGetir(); // Veritabanından bugünkü siparişleri çek
                var tr = new System.Globalization.CultureInfo("tr-TR"); // Türkçe para/sayı formatı

                // Sütunlar sadece ilk yüklemede oluşturulur.
                // Sonraki çağrılarda sadece satırlar temizlenir, sütunlar tekrar eklenmez.
                if (dgvBugun.Columns.Count == 0)
                {
                    // DataGridViewTextBoxColumn: Düz metin hücresi
                    // DataGridViewCheckBoxColumn: Onay kutusu hücresi (Teslim edildi için)
                    // FillWeight: Sütunun Fill modunda diğer sütunlara göre göreli genişliği
                    dgvBugun.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "#", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
                    dgvBugun.Columns.Add(new DataGridViewTextBoxColumn { Name = "Musteri", HeaderText = "Müşteri", FillWeight = 40 });
                    dgvBugun.Columns.Add(new DataGridViewTextBoxColumn { Name = "Telefon", HeaderText = "Telefon", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
                    dgvBugun.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hayvan", HeaderText = "Hayvan Türü", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
                    dgvBugun.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mama", HeaderText = "Mama / Yem", FillWeight = 40 });
                    dgvBugun.Columns.Add(new DataGridViewTextBoxColumn { Name = "Miktar", HeaderText = "Miktar", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
                    dgvBugun.Columns.Add(new DataGridViewTextBoxColumn { Name = "Birim", HeaderText = "Birim (₺)", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
                    dgvBugun.Columns.Add(new DataGridViewTextBoxColumn { Name = "Toplam", HeaderText = "Toplam (₺)", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
                    dgvBugun.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Teslim", HeaderText = "Teslim Edildi", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, ReadOnly = false });
                }
                dgvBugun.Rows.Clear(); // Önceki satırları temizle (sütunlara dokunma)

                int bekleyen = 0, teslim = 0;
                decimal ciro = 0;

                foreach (var s in liste)
                {
                    // Teslim durumuna göre sayaçları artır
                    if (s.TeslimEdildi) teslim++;
                    else bekleyen++;

                    ciro += s.ToplamFiyat; // Tüm siparişler ciro hesabına dahil

                    // Aktif filtreyle eşleşmeyen siparişler grid'e eklenmez, döngü devam eder
                    if (_aktifFiltre == "Bekleyen" && s.TeslimEdildi) continue;
                    if (_aktifFiltre == "Teslim" && !s.TeslimEdildi) continue;

                    
                    int ri = dgvBugun.Rows.Add(
                        s.SiparisId, s.TumAd, s.MusteriTelefon,
                        s.HayvanTuruTR, s.MamaTuru, s.Miktar,
                        s.BirimFiyat.ToString("N2", tr),   
                        s.ToplamFiyat.ToString("N2", tr),
                        s.TeslimEdildi                     
                    );

                    // Teslim edilmiş siparişlerin satırı soluklaştırılır (okunmuş gibi görünür)
                    if (s.TeslimEdildi)
                    {
                        
                        for (int c = 0; c < dgvBugun.Columns.Count - 1; c++)
                            dgvBugun.Rows[ri].Cells[c].Style.ForeColor = Color.FromArgb(100, 130, 100);
                        // Müşteri adı sütununu tam yeşil yap (vurgu)
                        dgvBugun.Rows[ri].Cells["Musteri"].Style.ForeColor = C_SUCCESS;
                    }
                }

                // İstatistik kartlarındaki sayıları güncelle
                lblStatToplam.Text = liste.Count.ToString();
                lblStatBekleyen.Text = bekleyen.ToString();
                lblStatTeslim.Text = teslim.ToString();
                lblStatCiro.Text = ciro.ToString("C2", tr);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Hata - BugunkuTeslimatlariYukle");
            }
            finally
            {
                _isUpdating = false;
            }
        }

        
        //  OLAY İŞLEYİCİLERİ
        
        private void DgvBugun_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dgvBugun.IsCurrentCellDirty)
            {
                dgvBugun.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        // Herhangi bir hücrenin değeri değiştiğinde tetiklenir.
        // Sadece "Teslim" sütunundaki CheckBox değişikliğine tepki verir.
        private void DgvBugun_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_isUpdating) return; // Toplu yükleme sırasında tetiklenirse görmezden gel
            if (e.RowIndex < 0 || dgvBugun.Columns[e.ColumnIndex].Name != "Teslim") return;

            // Değişen satırdan sipariş ID ve yeni teslim durumunu oku
            int siparisId = Convert.ToInt32(dgvBugun.Rows[e.RowIndex].Cells["Id"].Value);
            bool teslimEdildi = Convert.ToBoolean(dgvBugun.Rows[e.RowIndex].Cells["Teslim"].Value);

            // Veritabanında ilgili siparişi bul 
            var siparis = _db.TumSiparisleriGetir().FirstOrDefault(s => s.SiparisId == siparisId);
            if (siparis != null)
            {
                siparis.TeslimEdildi = teslimEdildi;
                _db.SiparisGuncelle(siparis); // Değişikliği veritabanına kaydet
                BugunkuTeslimatlariYukle();   // Grid ve istatistikleri yenile
            }
        }

        // Sidebar'daki tüm navigasyon butonlarının ortak Click olayı.
        // btn.Tag değerine göre hangi sayfanın açılacağına karar verilir.
        private void NavButon_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return; // Pattern matching ile güvenli tür dönüşümü

            switch (btn.Tag?.ToString())
            {
                case "yeni":
                    // Yeni sipariş formu modal olarak açılır (ShowDialog = kullanıcı kapatana kadar bekle)
                    // DialogResult.OK = Kullanıcı kaydet butonuna bastı demek
                    if (new SiparisForm(_db).ShowDialog() == DialogResult.OK)
                    {
                        _aktifFiltre = "Tümü"; // Yeni sipariş sonrası filtreyi sıfırla
                        BekleStatCards();
                        BugunkuTeslimatlariYukle();
                    }
                    break;

                case "liste":
                    new SiparisListesiForm(_db).ShowDialog();
                    BugunkuTeslimatlariYukle(); // Liste formunda değişiklik yapılmış olabilir, yenile
                    break;

                case "rapor":
                    new SatisRaporuForm(_db).ShowDialog(); // Rapor sayfası, veri değiştirmez
                    break;

                case "ana":
                default:
                    BugunkuTeslimatlariYukle(); // Ana sayfaya dönünce sadece veriyi yenile
                    break;
            }
        }

        
        //  YARDIMCI METODLAR
        

        // Stillendirilmiş bir DataGridView oluşturur ve döndürür.
        // Tüm renk/font/davranış ayarları burada merkezi olarak yönetilir.
        private DataGridView YapGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,    // İçerik panelinde kalan alanı kapla
                BackgroundColor = C_CARD,
                BorderStyle = BorderStyle.None,  // Grid'in dış çerçevesini kaldır
                GridColor = Color.FromArgb(30, 50, 80), 

                // Varsayılan hücre stili (tüm hücreler bu stili miras alır)
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = C_CARD,
                    ForeColor = C_TEXT,
                    SelectionBackColor = Color.FromArgb(45, 70, 100), 
                    SelectionForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f),
                    Padding = new Padding(4, 6, 4, 6) 
                },

                // Sütun başlık satırının stili
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = C_PANEL,
                    ForeColor = C_ACCENT,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    SelectionBackColor = C_PANEL,
                    Padding = new Padding(4, 6, 4, 6)
                },

                // Çift-tek satır renklendirmesi için alternatif satır stili
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(18, 30, 50) 
                },

                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, // Başlık yüksekliği sabit
                ColumnHeadersHeight = 42,
                RowHeadersVisible = false,  // Sol taraftaki satır numarası sütununu gizle
                AllowUserToAddRows = false,  // Kullanıcı yeni satır ekleyemez
                AllowUserToDeleteRows = false,  // Kullanıcı satır silemez
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, // Tıklamada tüm satır seçilir
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,    // Sütunlar genişliği paylaşır
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, // Sadece yatay çizgiler
                EnableHeadersVisualStyles = false,  // Windows tema stilini devre dışı bırak (özel stil için)
                MultiSelect = false,  // Aynı anda sadece 1 satır seçilebilir
                RowTemplate = { Height = 42 } // Her satırın sabit yüksekliği
            };
        }
    }
}