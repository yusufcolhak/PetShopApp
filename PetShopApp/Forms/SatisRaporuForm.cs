// ============================================================
// PetShop Sipariş Yönetim Sistemi
// Forms/SatisRaporuForm.cs  -  Tarih Aralığı Satış Raporu
// ============================================================

using PetShopApp.Data;
using PetShopApp.Models;

namespace PetShopApp.Forms
{
    public class SatisRaporuForm : Form
    {
        private readonly VeritabaniYonetici _db;

        private static readonly Color C_BG      = Color.FromArgb(15, 27, 45);
        private static readonly Color C_PANEL   = Color.FromArgb(10, 20, 35);
        private static readonly Color C_CARD    = Color.FromArgb(26, 41, 66);
        private static readonly Color C_TOOLBAR = Color.FromArgb(19, 32, 53);
        private static readonly Color C_ACCENT  = Color.FromArgb(79, 195, 247);
        private static readonly Color C_SUCCESS = Color.FromArgb(102, 187, 106);
        private static readonly Color C_WARNING = Color.FromArgb(255, 167, 38);
        private static readonly Color C_PURPLE  = Color.FromArgb(206, 147, 216);
        private static readonly Color C_TEXT    = Color.FromArgb(236, 239, 241);
        private static readonly Color C_SUB     = Color.FromArgb(144, 164, 174);

        private DateTimePicker dtpBaslangic  = null!;
        private DateTimePicker dtpBitis      = null!;
        private DataGridView   dgv           = null!;
        private Label          lblToplamTutar= null!;
        private Label          lblToplamSayi = null!;
        private Label          lblTeslimSayi = null!;
        private Label          lblOrtFiyat   = null!;
        private Panel          pnlBreakdown  = null!;

        public SatisRaporuForm(VeritabaniYonetici db)
        {
            _db = db;
            ArayuzOlustur();
            // Varsayılan: Tüm Yıl
            dtpBaslangic.Value = DateTime.Today.AddYears(-1);
            dtpBitis.Value     = DateTime.Today;
            Filtrele();
        }

        private void ArayuzOlustur()
        {
            Text          = "📊  Satış Raporu";
            Size          = new Size(1150, 720);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = C_BG;
            ForeColor     = C_TEXT;
            Font          = new Font("Segoe UI", 9.5f);

            // ── HEADER ──────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = C_PANEL };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "📊  Satış Raporu — Tarih Aralığı Filtreleme",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = C_ACCENT, AutoSize = true, Location = new Point(20, 16)
            });

            // ── FİLTRE ÇUBUĞU ───────────────────────────────────
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top, Height = 62,
                BackColor = C_TOOLBAR, Padding = new Padding(15, 12, 15, 12)
            };

            int fx = 18;
            pnlFilter.Controls.Add(new Label { Text = "📅 Başlangıç:", Location = new Point(fx,20), AutoSize=true, ForeColor=C_SUB });
            fx += 110;
            dtpBaslangic = new DateTimePicker { Location=new Point(fx,16), Size=new Size(130,30), Format=DateTimePickerFormat.Short, Font=new Font("Segoe UI",10f) };
            pnlFilter.Controls.Add(dtpBaslangic); fx += 145;

            pnlFilter.Controls.Add(new Label { Text = "📅 Bitiş:", Location=new Point(fx,20), AutoSize=true, ForeColor=C_SUB });
            fx += 75;
            dtpBitis = new DateTimePicker { Location=new Point(fx,16), Size=new Size(130,30), Format=DateTimePickerFormat.Short, Font=new Font("Segoe UI",10f) };
            pnlFilter.Controls.Add(dtpBitis); fx += 150;

            var btnFiltrele = YapButon("🔍  Filtrele", new Point(fx,14), 110, C_ACCENT, C_PANEL, bold:true);
            btnFiltrele.Click += (s,e) => Filtrele();
            pnlFilter.Controls.Add(btnFiltrele); fx += 125;

            // Hızlı filtre
            void HizliFiltre(string label, Action action, int w = 72)
            {
                var b = YapButon(label, new Point(fx, 14), w, C_CARD, C_TEXT);
                b.Click += (s,e) => action();
                pnlFilter.Controls.Add(b);
                fx += w + 6;
            }

            HizliFiltre("Bugün",     () => { dtpBaslangic.Value = dtpBitis.Value = DateTime.Today; Filtrele(); });
            HizliFiltre("Bu Hafta",  () => { dtpBaslangic.Value = DateTime.Today.AddDays(-6); dtpBitis.Value = DateTime.Today; Filtrele(); }, 78);
            HizliFiltre("Bu Ay",     () => { dtpBaslangic.Value = new DateTime(DateTime.Today.Year,DateTime.Today.Month,1); dtpBitis.Value = DateTime.Today; Filtrele(); });
            HizliFiltre("Bu Yıl",    () => { dtpBaslangic.Value = new DateTime(DateTime.Today.Year,1,1); dtpBitis.Value = DateTime.Today; Filtrele(); });

            // ── ALT ÖZET PANELİ ─────────────────────────────────
            var pnlSummary = new Panel
            {
                Dock = DockStyle.Bottom, Height = 120, BackColor = C_PANEL
            };

            pnlSummary.Controls.Add(new Label
            {
                Text="ÖZET", Location=new Point(20,8), AutoSize=true,
                Font=new Font("Segoe UI",8f,FontStyle.Bold), ForeColor=C_SUB
            });

            var pnlCards = new Panel { Location=new Point(20,26), Size=new Size(1100,82), BackColor=Color.Transparent };
            pnlCards.Controls.Add(OzetKart("💰 Toplam Ciro",      "₺0,00", C_SUCCESS, 0,   out lblToplamTutar));
            pnlCards.Controls.Add(OzetKart("🧾 Sipariş Sayısı",   "0",     C_ACCENT,  190, out lblToplamSayi));
            pnlCards.Controls.Add(OzetKart("✅ Teslim Edildi",    "0",     C_SUCCESS, 380, out lblTeslimSayi));
            pnlCards.Controls.Add(OzetKart("📈 Ort. Sipariş",    "₺0,00", C_WARNING, 570, out lblOrtFiyat));

            // Hayvan bazlı özet
            pnlBreakdown = new Panel { Location=new Point(770,0), Size=new Size(320,82), BackColor=Color.Transparent };
            pnlCards.Controls.Add(pnlBreakdown);

            pnlSummary.Controls.Add(pnlCards);

            // ── GRID ────────────────────────────────────────────
            dgv = YapGrid();

            Controls.Add(dgv);
            Controls.Add(pnlSummary);
            Controls.Add(pnlFilter);
            Controls.Add(pnlHeader);

            pnlHeader.BringToFront();
            pnlFilter.BringToFront();
            pnlSummary.BringToFront();
        }

        private static Panel OzetKart(string baslik, string deger, Color renk, int x, out Label lblDeger)
        {
            var pnl = new Panel
            {
                Location = new Point(x, 0), Size = new Size(185, 82),
                BackColor = Color.FromArgb(26, 41, 66)
            };
            pnl.Controls.Add(new Panel { Width=4, Dock=DockStyle.Left, BackColor=renk });
            pnl.Controls.Add(new Label { Text=baslik, Dock=DockStyle.Top, Height=26, ForeColor=Color.FromArgb(144,164,174), Font=new Font("Segoe UI",8f), Padding=new Padding(10,6,0,0) });
            var lbl = new Label { Text=deger, Dock=DockStyle.Fill, ForeColor=renk, Font=new Font("Segoe UI",18f,FontStyle.Bold), Padding=new Padding(10,0,0,0), TextAlign=ContentAlignment.MiddleLeft };
            pnl.Controls.Add(lbl);
            lblDeger = lbl;
            return pnl;
        }

        private void Filtrele()
        {
            if (dtpBaslangic.Value.Date > dtpBitis.Value.Date)
            {
                MessageBox.Show("Başlangıç tarihi bitiş tarihinden büyük olamaz!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var liste = _db.TarihAraligiSorgu(dtpBaslangic.Value, dtpBitis.Value);
            var tr    = new System.Globalization.CultureInfo("tr-TR");

            // ── GRID DOLDUR ─────────────────────────────────────
            dgv.Rows.Clear();
            foreach (var s in liste)
            {
                int ri = dgv.Rows.Add(
                    s.SiparisId, s.TumAd, s.MusteriTelefon,
                    s.HayvanTuruTR, s.MamaTuru, s.Miktar,
                    s.BirimFiyat.ToString("N2", tr),
                    s.ToplamFiyat.ToString("N2", tr),
                    s.TeslimTarihi.ToString("dd.MM.yyyy"),
                    s.TeslimEdildi ? "✅ Teslim" : "⏳ Bekliyor"
                );

                if (s.TeslimEdildi)
                    dgv.Rows[ri].Cells["Durum"].Style.ForeColor = C_SUCCESS;
            }

            // ── ÖZET HESAPLA ────────────────────────────────────
            decimal toplam   = liste.Sum(s => s.ToplamFiyat);
            int     sayi     = liste.Count;
            int     teslim   = liste.Count(s => s.TeslimEdildi);
            decimal ort      = sayi > 0 ? toplam / sayi : 0;

            lblToplamTutar.Text = toplam.ToString("C2", tr);
            lblToplamSayi.Text  = sayi.ToString();
            lblTeslimSayi.Text  = $"{teslim} / {sayi}";
            lblOrtFiyat.Text    = ort.ToString("C2", tr);

            // ── HAYVAN BAZLI OZET ────────────────────────────────
            pnlBreakdown.Controls.Clear();
            var gruplar = liste
                .GroupBy(s => s.HayvanTuruTR)
                .Select(g => (Hayvan: g.Key, Sayi: g.Count(), Toplam: g.Sum(s=>s.ToplamFiyat)))
                .OrderByDescending(g => g.Toplam)
                .Take(4)
                .ToList();

            int bx = 0;
            foreach (var g in gruplar)
            {
                var card = new Panel
                {
                    Location  = new Point(bx, 0), Size = new Size(100, 82),
                    BackColor = C_CARD
                };
                
                var lblAdet = new Label { Text=$"{g.Sayi} adet", Location = new Point(0, 6), Size = new Size(100, 16), Font=new Font("Segoe UI",8.5f), ForeColor=C_ACCENT, TextAlign=ContentAlignment.MiddleCenter };
                var lblHayvan = new Label { Text=g.Hayvan, Location = new Point(0, 24), Size = new Size(100, 22), Font=new Font("Segoe UI",9.5f,FontStyle.Bold), ForeColor=C_SUB, TextAlign=ContentAlignment.MiddleCenter };
                var lblFiyat = new Label { Text=g.Toplam.ToString("N0",tr)+"₺", Location = new Point(0, 48), Size = new Size(100, 28), Font=new Font("Segoe UI",11.5f,FontStyle.Bold), ForeColor=C_SUCCESS, TextAlign=ContentAlignment.MiddleCenter };
                
                card.Controls.AddRange(new Control[] { lblAdet, lblHayvan, lblFiyat });
                pnlBreakdown.Controls.Add(card);
                bx += 105;
            }
        }

        // ── YARDIMCI ────────────────────────────────────────────
        private DataGridView YapGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = C_CARD,
                BorderStyle = BorderStyle.None, GridColor = Color.FromArgb(30,50,80),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = C_CARD, ForeColor = C_TEXT,
                    SelectionBackColor = Color.FromArgb(79,195,247,55),
                    SelectionForeColor = C_TEXT,
                    Font = new Font("Segoe UI", 9.5f), Padding = new Padding(4,5,4,5)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = C_PANEL, ForeColor = C_ACCENT,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    SelectionBackColor = C_PANEL, Padding = new Padding(4,5,4,5)
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(18,30,50) },
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40, RowHeadersVisible = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false, RowTemplate = { Height = 38 }
            };

            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Id",        HeaderText="#",           FillWeight=4  });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Musteri",   HeaderText="Müşteri",     FillWeight=16 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Telefon",   HeaderText="Telefon",     FillWeight=11 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Hayvan",    HeaderText="Hayvan",      FillWeight=10 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Mama",      HeaderText="Mama / Yem",  FillWeight=17 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Miktar",    HeaderText="Miktar",      FillWeight=6  });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="BirimFiyat",HeaderText="Birim (₺)",   FillWeight=9  });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Toplam",    HeaderText="Toplam (₺)",  FillWeight=10 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="TeslimTar", HeaderText="Teslim Tar.", FillWeight=10 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Durum",     HeaderText="Durum",       FillWeight=8  });
            return g;
        }

        private static Button YapButon(string text, Point konum, int w, Color bg, Color fg, bool bold=false)
        {
            var b = new Button
            {
                Text = text, Location = konum, Size = new Size(w, 34),
                FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = fg,
                Font = new Font("Segoe UI", 9.5f, bold ? FontStyle.Bold : FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}
