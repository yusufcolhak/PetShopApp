// ============================================================
// PetShop Sipariş Yönetim Sistemi
// Forms/SiparisForm.cs  -  Sipariş Ekle / Düzenle Formu
// ============================================================

using PetShopApp.Data;
using PetShopApp.Models;

namespace PetShopApp.Forms
{
    public class SiparisForm : Form
    {
        private readonly VeritabaniYonetici _db;
        private readonly Siparis? _mevcutSiparis;

        // ── RENK PALETİ ─────────────────────────────────────────
        private static readonly Color C_BG      = Color.FromArgb(15, 27, 45);
        private static readonly Color C_PANEL   = Color.FromArgb(10, 20, 35);
        private static readonly Color C_CARD    = Color.FromArgb(26, 41, 66);
        private static readonly Color C_INPUT   = Color.FromArgb(19, 32, 53);
        private static readonly Color C_ACCENT  = Color.FromArgb(79, 195, 247);
        private static readonly Color C_SUCCESS = Color.FromArgb(102, 187, 106);
        private static readonly Color C_TEXT    = Color.FromArgb(236, 239, 241);
        private static readonly Color C_SUB     = Color.FromArgb(144, 164, 174);
        private static readonly Color C_BORDER  = Color.FromArgb(40, 60, 90);

        // ── KONTROLLER ──────────────────────────────────────────
        private TextBox         txtAd        = null!;
        private TextBox         txtSoyad     = null!;
        private TextBox         txtTelefon   = null!;
        private ComboBox        cmbHayvan    = null!;
        private ComboBox        cmbMama      = null!;
        private NumericUpDown   nudMiktar    = null!;
        private NumericUpDown   nudBirimFiyat= null!;
        private Label           lblToplam    = null!;
        private DateTimePicker  dtpSiparis   = null!;
        private DateTimePicker  dtpTeslim    = null!;
        private TextBox         txtNotlar    = null!;

        public SiparisForm(VeritabaniYonetici db, Siparis? mevcutSiparis = null)
        {
            _db            = db;
            _mevcutSiparis = mevcutSiparis;
            ArayuzOlustur();
            AutoCompleteAyarla();
            if (_mevcutSiparis != null) FormDoldur();
        }

        private void AutoCompleteAyarla()
        {
            var siparisler = _db.TumSiparisleriGetir();
            
            var adListesi = new AutoCompleteStringCollection();
            adListesi.AddRange(siparisler.Select(s => s.MusteriAd).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray());
            txtAd.AutoCompleteCustomSource = adListesi;
            txtAd.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtAd.AutoCompleteSource = AutoCompleteSource.CustomSource;

            var soyadListesi = new AutoCompleteStringCollection();
            soyadListesi.AddRange(siparisler.Select(s => s.MusteriSoyad).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray());
            txtSoyad.AutoCompleteCustomSource = soyadListesi;
            txtSoyad.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtSoyad.AutoCompleteSource = AutoCompleteSource.CustomSource;

            var telListesi = new AutoCompleteStringCollection();
            telListesi.AddRange(siparisler.Select(s => s.MusteriTelefon).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray());
            txtTelefon.AutoCompleteCustomSource = telListesi;
            txtTelefon.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtTelefon.AutoCompleteSource = AutoCompleteSource.CustomSource;

            var mamaListesi = siparisler.Select(s => s.MamaTuru).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            foreach (var m in mamaListesi)
            {
                if (!cmbMama.Items.Contains(m))
                    cmbMama.Items.Add(m);
            }
            cmbMama.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbMama.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

        // ════════════════════════════════════════════════════════
        //  ARAYÜZ OLUŞTURMA
        // ════════════════════════════════════════════════════════
        private void ArayuzOlustur()
        {
            Text            = _mevcutSiparis == null ? "➕  Yeni Sipariş Ekle" : "✏️  Sipariş Düzenle";
            Size            = new Size(580, 700);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = C_BG;
            ForeColor       = C_TEXT;
            Font            = new Font("Segoe UI", 9.5f);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;

            // ── HEADER ──────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = C_PANEL };
            pnlHeader.Controls.Add(new Label
            {
                Text     = Text,
                Font     = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor= C_ACCENT,
                AutoSize = true,
                Location = new Point(20, 16)
            });

            // ── BUTONLAR (ALT) ───────────────────────────────────
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 62, BackColor = C_PANEL };

            var btnKaydet = YapButon("💾  Kaydet", new Point(310, 12), 140, C_ACCENT, Color.FromArgb(10,20,35));
            btnKaydet.Font   = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnKaydet.Click += BtnKaydet_Click;

            var btnIptal = YapButon("✖  İptal", new Point(165, 12), 130, Color.FromArgb(55,70,90), C_TEXT);
            btnIptal.DialogResult = DialogResult.Cancel;

            pnlBottom.Controls.Add(btnKaydet);
            pnlBottom.Controls.Add(btnIptal);

            // ── KAYDIRILEBILIR İÇERİK ───────────────────────────
            var scroll = new Panel
            {
                Dock      = DockStyle.Fill,
                AutoScroll= true,
                Padding   = new Padding(0)
            };

            var inner = new Panel { AutoSize = true, Width = 558, Location = new Point(0,0) };

            int y = 50;

            // MÜŞTERİ BİLGİLERİ
            BolumEkle(inner, "👤 Müşteri Bilgileri", ref y);
            var pnlAd = YapRowPanel(inner, y, 520);
            txtAd    = YapTextBox(pnlAd, "Ad *",    0,  0, 244);
            txtSoyad = YapTextBox(pnlAd, "Soyad *", 264, 0, 244);
            y += 75;

            txtTelefon = YapTekAlan(inner, "📱 Telefon", ref y, 520, "0(5XX) XXX XX XX");

            y += 14;
            // HAYVAN & MAMA
            BolumEkle(inner, "🐾 Hayvan ve Mama Bilgileri", ref y);

            var pnlHM = YapRowPanel(inner, y, 520);

            cmbHayvan = YapComboBox(pnlHM, "🦮 Hayvan Türü *", 0, new[]
            {
                "🐱 Kedi","🐶 Köpek","🐦 Kuş","🐟 Balık","🐹 Hamster","🐾 Diğer"
            }, 244, dropDown: false);
            cmbHayvan.SelectedIndex = 0;

            cmbMama = YapComboBox(pnlHM, "🥩 Mama / Yem Türü *", 264, new[]
            {
                "Royal Canin Kitten","Royal Canin Adult","Royal Canin Senior",
                "Purina Pro Plan","Purina ONE","Hill's Science Diet",
                "Whiskas Yetişkin","Felix Pouch","Friskies",
                "Pedigree Yetişkin","Pedigree Puppy","Chappi",
                "Vitakraft Kuş Yemi","Versele-Laga","Trixie Kuş Yemi",
                "Tetra Goldfish","JBL NovoGranoMix","Sera Vipan",
                "Hamster Mix","Vitakraft Hamster","Diğer"
            }, 244, dropDown: true);
            y += 75;

            // MİKTAR / FİYAT
            var pnlFiyat = YapRowPanel(inner, y, 520);

            var lblM = YapEtiket(pnlFiyat, "📦 Miktar *", 0, 0);
            nudMiktar = new NumericUpDown
            {
                Location  = new Point(0, 20), Size = new Size(150, 34),
                Minimum=1, Maximum=9999, Value=1,
                BackColor = C_INPUT, ForeColor = C_TEXT,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center
            };
            nudMiktar.ValueChanged += HesaplaToplamFiyat;
            nudMiktar.KeyUp += HesaplaToplamFiyat;

            var lblB = YapEtiket(pnlFiyat, "💰 Birim Fiyat (₺) *", 165, 0);
            nudBirimFiyat = new NumericUpDown
            {
                Location  = new Point(165, 20), Size = new Size(150, 34),
                Minimum=0, Maximum=999999, Value=0, DecimalPlaces=2, Increment=10,
                BackColor = C_INPUT, ForeColor = C_TEXT,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center
            };
            nudBirimFiyat.ValueChanged += HesaplaToplamFiyat;
            nudBirimFiyat.KeyUp += HesaplaToplamFiyat;

            var lblTL = YapEtiket(pnlFiyat, "💎 Toplam", 335, 0);
            lblToplam = new Label
            {
                Text      = "₺0,00",
                Location  = new Point(335, 18),
                Size      = new Size(178, 36),
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = C_SUCCESS,
                BackColor = Color.FromArgb(20, 102, 187, 106),
                TextAlign = ContentAlignment.MiddleRight,
                BorderStyle = BorderStyle.FixedSingle
            };

            pnlFiyat.Controls.AddRange(new Control[] { nudMiktar, nudBirimFiyat, lblToplam });
            y += 75;

            y += 14;
            // TARİH
            BolumEkle(inner, "📅 Tarih Bilgileri", ref y);

            var pnlTar = YapRowPanel(inner, y, 520);
            var lblSipTar = YapEtiket(pnlTar, "📋 Sipariş Tarihi *", 0, 0);
            dtpSiparis = new DateTimePicker
            {
                Location = new Point(0, 20), Size = new Size(244, 32),
                Format   = DateTimePickerFormat.Short,
                Font     = new Font("Segoe UI", 10f), Value = DateTime.Today
            };
            var lblTeslamTar = YapEtiket(pnlTar, "📦 Teslim Tarihi *", 264, 0);
            dtpTeslim = new DateTimePicker
            {
                Location = new Point(264, 20), Size = new Size(244, 32),
                Format   = DateTimePickerFormat.Short,
                Font     = new Font("Segoe UI", 10f), Value = DateTime.Today
            };
            pnlTar.Controls.AddRange(new Control[] { dtpSiparis, dtpTeslim });
            y += 75;

            y += 14;
            // NOTLAR
            BolumEkle(inner, "📝 Ek Notlar (isteğe bağlı)", ref y);
            txtNotlar = new TextBox
            {
                Location    = new Point(20, y), Size = new Size(520, 72),
                Multiline   = true, ScrollBars = ScrollBars.Vertical,
                BackColor   = C_INPUT, ForeColor = C_TEXT,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 9.5f),
                PlaceholderText = "Özel teslimat notu, müşteri isteği vs..."
            };
            inner.Controls.Add(txtNotlar);
            y += 82;
            inner.Height = y + 10;
            scroll.Controls.Add(inner);

            Controls.Add(pnlHeader);
            Controls.Add(pnlBottom);
            Controls.Add(scroll);
            pnlHeader.BringToFront();
            pnlBottom.BringToFront();
        }

        // ════════════════════════════════════════════════════════
        //  YARDIMCI UI METODLARI
        // ════════════════════════════════════════════════════════
        private static void BolumEkle(Panel parent, string baslik, ref int y)
        {
            parent.Controls.Add(new Label
            {
                Text     = baslik, Location = new Point(20, y), AutoSize = true,
                Font     = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor= Color.FromArgb(79, 195, 247)
            });
            parent.Controls.Add(new Panel
            {
                Location  = new Point(20, y + 22), Size = new Size(520, 1),
                BackColor = Color.FromArgb(40, 60, 90)
            });
            y += 34;
        }

        private static Panel YapRowPanel(Panel parent, int y, int w)
        {
            var p = new Panel { Location = new Point(20, y), Size = new Size(w, 70), BackColor = Color.Transparent };
            parent.Controls.Add(p);
            return p;
        }

        private static Label YapEtiket(Panel parent, string text, int x, int y)
        {
            var lbl = new Label { Text = text, Location = new Point(x, y), AutoSize = true, ForeColor = Color.FromArgb(144,164,174), Font = new Font("Segoe UI", 8.5f) };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private TextBox YapTextBox(Panel parent, string etiket, int x, int y, int w)
        {
            YapEtiket(parent, etiket, x, y);
            var txt = new TextBox
            {
                Location = new Point(x, y + 20), Size = new Size(w, 32),
                BackColor = C_INPUT, ForeColor = C_TEXT,
                BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10f)
            };
            parent.Controls.Add(txt);
            return txt;
        }

        private TextBox YapTekAlan(Panel parent, string etiket, ref int y, int w, string placeholder = "")
        {
            parent.Controls.Add(new Label
            {
                Text     = etiket, Location = new Point(20, y), AutoSize = true,
                ForeColor= C_SUB, Font = new Font("Segoe UI", 8.5f)
            });
            var txt = new TextBox
            {
                Location    = new Point(20, y + 18), Size = new Size(w, 32),
                BackColor   = C_INPUT, ForeColor = C_TEXT,
                BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10f),
                PlaceholderText = placeholder
            };
            parent.Controls.Add(txt);
            y += 56;
            return txt;
        }

        private ComboBox YapComboBox(Panel parent, string etiket, int x, string[] items, int w, bool dropDown)
        {
            YapEtiket(parent, etiket, x, 0);
            var cmb = new ComboBox
            {
                Location     = new Point(x, 20), Size = new Size(w, 32),
                BackColor    = C_INPUT, ForeColor = C_TEXT,
                DropDownStyle= dropDown ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList,
                FlatStyle    = FlatStyle.Flat, Font = new Font("Segoe UI", 10f)
            };
            cmb.Items.AddRange(items);
            parent.Controls.Add(cmb);
            return cmb;
        }

        private static Button YapButon(string text, Point konum, int w, Color bg, Color fg)
        {
            var btn = new Button
            {
                Text         = text, Location = konum, Size = new Size(w, 38),
                FlatStyle    = FlatStyle.Flat, BackColor = bg, ForeColor = fg,
                Font         = new Font("Segoe UI", 10f), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ════════════════════════════════════════════════════════
        //  OLAY İŞLEYİCİLERİ
        // ════════════════════════════════════════════════════════
        private void HesaplaToplamFiyat(object? sender, EventArgs e)
        {
            decimal toplam = nudMiktar.Value * nudBirimFiyat.Value;
            lblToplam.Text = toplam.ToString("C2", new System.Globalization.CultureInfo("tr-TR"));
        }

        private void FormDoldur()
        {
            if (_mevcutSiparis == null) return;
            txtAd.Text         = _mevcutSiparis.MusteriAd;
            txtSoyad.Text      = _mevcutSiparis.MusteriSoyad;
            txtTelefon.Text    = _mevcutSiparis.MusteriTelefon;
            cmbHayvan.SelectedIndex = (int)_mevcutSiparis.HayvanTuru;
            cmbMama.Text       = _mevcutSiparis.MamaTuru;
            nudMiktar.Value    = _mevcutSiparis.Miktar;
            nudBirimFiyat.Value= _mevcutSiparis.BirimFiyat;
            dtpSiparis.Value   = _mevcutSiparis.SiparisTarihi;
            dtpTeslim.Value    = _mevcutSiparis.TeslimTarihi;
            txtNotlar.Text     = _mevcutSiparis.Notlar;
            HesaplaToplamFiyat(null, EventArgs.Empty);
        }

        private void BtnKaydet_Click(object? sender, EventArgs e)
        {
            // Doğrulama
            if (string.IsNullOrWhiteSpace(txtAd.Text) || string.IsNullOrWhiteSpace(txtSoyad.Text))
            {
                MessageBox.Show("Lütfen müşteri adı ve soyadını giriniz!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAd.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(cmbMama.Text))
            {
                MessageBox.Show("Lütfen mama / yem türünü giriniz!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbMama.Focus();
                return;
            }
            if (nudBirimFiyat.Value <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir birim fiyat giriniz!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudBirimFiyat.Focus();
                return;
            }

            var s = _mevcutSiparis ?? new Siparis();
            s.MusteriAd      = txtAd.Text.Trim();
            s.MusteriSoyad   = txtSoyad.Text.Trim();
            s.MusteriTelefon = txtTelefon.Text.Trim();
            s.HayvanTuru     = (HayvanTuru)cmbHayvan.SelectedIndex;
            s.MamaTuru       = cmbMama.Text.Trim();
            s.Miktar         = (int)nudMiktar.Value;
            s.BirimFiyat     = nudBirimFiyat.Value;
            s.SiparisTarihi  = dtpSiparis.Value;
            s.TeslimTarihi   = dtpTeslim.Value;
            s.Notlar         = txtNotlar.Text.Trim();

            if (_mevcutSiparis == null)
                _db.SiparisEkle(s);
            else
                _db.SiparisGuncelle(s);

            MessageBox.Show("Sipariş başarıyla kaydedildi! ✅", "Başarılı",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
