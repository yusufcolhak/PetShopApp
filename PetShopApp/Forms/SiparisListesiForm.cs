// ============================================================
// PetShop Sipariş Yönetim Sistemi
// Forms/SiparisListesiForm.cs  -  Tüm Siparişler Listesi
// ============================================================

using PetShopApp.Data;
using PetShopApp.Models;

namespace PetShopApp.Forms
{
    public class SiparisListesiForm : Form
    {
        private readonly VeritabaniYonetici _db;
        private List<Siparis> _tumListe = new();
        private bool          _isUpdating = false;

        private static readonly Color C_BG      = Color.FromArgb(15, 27, 45);
        private static readonly Color C_PANEL   = Color.FromArgb(10, 20, 35);
        private static readonly Color C_CARD    = Color.FromArgb(26, 41, 66);
        private static readonly Color C_TOOLBAR = Color.FromArgb(19, 32, 53);
        private static readonly Color C_ACCENT  = Color.FromArgb(79, 195, 247);
        private static readonly Color C_SUCCESS = Color.FromArgb(102, 187, 106);
        private static readonly Color C_DANGER  = Color.FromArgb(239, 83, 80);
        private static readonly Color C_TEXT    = Color.FromArgb(236, 239, 241);
        private static readonly Color C_SUB     = Color.FromArgb(144, 164, 174);

        private DataGridView dgv       = null!;
        private TextBox      txtArama  = null!;
        private ComboBox     cmbFiltre = null!;
        
        private Label        lblToplamTutar= null!;
        private Label        lblToplamSayi = null!;
        private Label        lblTeslimSayi = null!;
        private Label        lblOrtFiyat   = null!;
        private Panel        pnlBreakdown  = null!;

        public SiparisListesiForm(VeritabaniYonetici db)
        {
            _db = db;
            ArayuzOlustur();
            ListeyiYukle();
        }

        private void ArayuzOlustur()
        {
            Text          = "📋  Tüm Siparişler";
            Size          = new Size(1100, 660);
            StartPosition = FormStartPosition.CenterParent;
            BackColor     = C_BG;
            ForeColor     = C_TEXT;
            Font          = new Font("Segoe UI", 9.5f);

            // ── HEADER ──────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = C_PANEL };
            pnlHeader.Controls.Add(new Label
            {
                Text     = "📋  Tüm Siparişler",
                Font     = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor= C_ACCENT, AutoSize = true, Location = new Point(20, 16)
            });

            // ── TOOLBAR ─────────────────────────────────────────
            var pnlTool = new Panel
            {
                Dock = DockStyle.Top, Height = 55,
                BackColor = C_TOOLBAR, Padding = new Padding(15, 10, 15, 10)
            };

            // Arama
            pnlTool.Controls.Add(new Label { Text = "🔍 Ara:", Location = new Point(15,18), AutoSize=true, ForeColor=C_SUB });
            txtArama = new TextBox
            {
                Location = new Point(85, 14), Size = new Size(180, 30),
                BackColor= C_CARD, ForeColor = C_TEXT,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f), PlaceholderText = "Müşteri adı..."
            };
            txtArama.TextChanged += (s,e) => FiltreleListeyi();
            pnlTool.Controls.Add(txtArama);

            // Hayvan filtresi
            pnlTool.Controls.Add(new Label { Text = "🐾 Hayvan:", Location = new Point(285,18), AutoSize=true, ForeColor=C_SUB });
            cmbFiltre = new ComboBox
            {
                Location = new Point(375, 14), Size = new Size(150, 30),
                BackColor= C_CARD, ForeColor = C_TEXT,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f)
            };
            cmbFiltre.Items.Add("Tümü");
            cmbFiltre.Items.AddRange(new[] { "🐱 Kedi","🐶 Köpek","🐦 Kuş","🐟 Balık","🐹 Hamster","🐾 Diğer" });
            cmbFiltre.SelectedIndex = 0;
            cmbFiltre.SelectedIndexChanged += (s,e) => FiltreleListeyi();
            pnlTool.Controls.Add(cmbFiltre);

            // Butonlar
            var btnYeni = YapButon("➕ Yeni", new Point(740, 12), 100, C_ACCENT, C_PANEL);
            btnYeni.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnYeni.Click += (s,e) =>
            {
                if (new SiparisForm(_db).ShowDialog() == DialogResult.OK)
                {
                    txtArama.Text = "";
                    cmbFiltre.SelectedIndex = 0;
                    ListeyiYukle();
                }
            };

            var btnDuz = YapButon("✏️ Düzenle", new Point(855, 12), 110, C_CARD, C_TEXT);
            btnDuz.FlatAppearance.BorderColor = C_ACCENT;
            btnDuz.FlatAppearance.BorderSize  = 1;
            btnDuz.Click += BtnDuzenle_Click;

            var btnSil = YapButon("🗑️ Sil", new Point(975, 12), 95, Color.FromArgb(55,20,20), C_DANGER);
            btnSil.FlatAppearance.BorderColor = C_DANGER;
            btnSil.FlatAppearance.BorderSize  = 1;
            btnSil.Click += BtnSil_Click;

            pnlTool.Controls.Add(btnYeni);
            pnlTool.Controls.Add(btnDuz);
            pnlTool.Controls.Add(btnSil);

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

            var pnlCards = new Panel { Location=new Point(20,26), Size=new Size(1060,82), BackColor=Color.Transparent };
            pnlCards.Controls.Add(OzetKart("💰 Toplam Ciro",      "₺0,00", C_SUCCESS, 0,   out lblToplamTutar));
            pnlCards.Controls.Add(OzetKart("🧾 Sipariş Sayısı",   "0",     C_ACCENT,  190, out lblToplamSayi));
            pnlCards.Controls.Add(OzetKart("✅ Teslim Edildi",    "0",     C_SUCCESS, 380, out lblTeslimSayi));
            pnlCards.Controls.Add(OzetKart("📈 Ort. Sipariş",    "₺0,00", Color.FromArgb(255,167,38), 570, out lblOrtFiyat));

            // Hayvan bazlı özet
            pnlBreakdown = new Panel { Location=new Point(760,0), Size=new Size(320,82), BackColor=Color.Transparent };
            pnlCards.Controls.Add(pnlBreakdown);

            pnlSummary.Controls.Add(pnlCards);

            // ── GRID ────────────────────────────────────────────
            dgv = YapGrid();
            dgv.CellDoubleClick += (s,e) => { if (e.RowIndex >= 0) Duzenle(); };
            dgv.CellValueChanged += Dgv_CellValueChanged;
            dgv.CurrentCellDirtyStateChanged += Dgv_CurrentCellDirtyStateChanged;

            Controls.Add(dgv);         // Dock=Fill control should be added first or handled with Z-order
            Controls.Add(pnlHeader);   // Dock=Top
            Controls.Add(pnlTool);     // Dock=Top
            Controls.Add(pnlSummary);  // Dock=Bottom

            pnlHeader.SendToBack();
            pnlTool.SendToBack();
            pnlSummary.SendToBack();
            dgv.BringToFront();
        }

        private void ListeyiYukle()
        {
            _tumListe = _db.TumSiparisleriGetir();
            FiltreleListeyi();
        }

        private void FiltreleListeyi()
        {
            var q = _tumListe.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(txtArama.Text))
                q = q.Where(s => s.TumAd.Contains(txtArama.Text, StringComparison.OrdinalIgnoreCase)
                               || s.MusteriTelefon.Contains(txtArama.Text));

            if (cmbFiltre.SelectedIndex > 0)
                q = q.Where(s => (int)s.HayvanTuru == cmbFiltre.SelectedIndex - 1);

            GosterListeyi(q.ToList());
        }

        private void GosterListeyi(List<Siparis> liste)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            var tr = new System.Globalization.CultureInfo("tr-TR");
            dgv.Rows.Clear();

            foreach (var s in liste)
            {
                int ri = dgv.Rows.Add(
                    s.SiparisId, s.TumAd, s.MusteriTelefon,
                    s.HayvanTuruTR, s.MamaTuru, s.Miktar,
                    s.BirimFiyat.ToString("N2", tr),
                    s.ToplamFiyat.ToString("N2", tr),
                    s.SiparisTarihi.ToString("dd.MM.yyyy"),
                    s.TeslimTarihi.ToString("dd.MM.yyyy"),
                    s.TeslimEdildi
                );

                if (s.TeslimEdildi)
                {
                    for (int c = 0; c < dgv.Columns.Count - 1; c++)
                        dgv.Rows[ri].Cells[c].Style.ForeColor = Color.FromArgb(100, 130, 100);
                    dgv.Rows[ri].Cells["Musteri"].Style.ForeColor = C_SUCCESS;
                }
                else if (s.TeslimTarihi.Date < DateTime.Today)
                {
                    dgv.Rows[ri].Cells["TeslimTar"].Style.ForeColor = C_DANGER;
                }
            }

            _isUpdating = false;

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
                .Take(3)
                .ToList();

            int bx = 0;
            foreach (var g in gruplar)
            {
                var card = new Panel
                {
                    Location  = new Point(bx, 0), Size = new Size(100, 82),
                    BackColor = C_CARD
                };
                card.Controls.Add(new Label { Text=g.Hayvan, Dock=DockStyle.Top, Height=22, Font=new Font("Segoe UI",8.5f,FontStyle.Bold), ForeColor=C_SUB, TextAlign=ContentAlignment.MiddleCenter });
                card.Controls.Add(new Label { Text=$"{g.Sayi} adet", Dock=DockStyle.Top, Height=16, Font=new Font("Segoe UI",7.5f), ForeColor=C_ACCENT, TextAlign=ContentAlignment.MiddleCenter });
                card.Controls.Add(new Label { Text=g.Toplam.ToString("N0",tr)+"₺", Dock=DockStyle.Fill, Font=new Font("Segoe UI",10.5f,FontStyle.Bold), ForeColor=C_SUCCESS, TextAlign=ContentAlignment.MiddleCenter });
                pnlBreakdown.Controls.Add(card);
                bx += 105;
            }
        }

        private void Dgv_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dgv.IsCurrentCellDirty)
            {
                dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void Dgv_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_isUpdating) return;
            if (e.RowIndex < 0 || dgv.Columns[e.ColumnIndex].Name != "Teslim") return;

            int  siparisId   = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["Id"].Value);
            bool teslimEdildi= Convert.ToBoolean(dgv.Rows[e.RowIndex].Cells["Teslim"].Value);

            var siparis = _tumListe.FirstOrDefault(s => s.SiparisId == siparisId);
            if (siparis != null)
            {
                siparis.TeslimEdildi = teslimEdildi;
                _db.SiparisGuncelle(siparis);
                
                // Özet bilgilerini ve satır stilini güncellemek için listeyi yeniden filtrele/göster
                FiltreleListeyi();
            }
        }

        private void Duzenle()
        {
            if (dgv.CurrentRow == null) return;
            int id = Convert.ToInt32(dgv.CurrentRow.Cells["Id"].Value);
            var s  = _tumListe.FirstOrDefault(x => x.SiparisId == id);
            if (s == null) return;
            if (new SiparisForm(_db, s).ShowDialog() == DialogResult.OK) ListeyiYukle();
        }

        private void BtnDuzenle_Click(object? sender, EventArgs e) => Duzenle();

        private void BtnSil_Click(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            int    id      = Convert.ToInt32(dgv.CurrentRow.Cells["Id"].Value);
            string musteri = dgv.CurrentRow.Cells["Musteri"].Value?.ToString() ?? "";

            if (MessageBox.Show(
                    $"'{musteri}' müşterisine ait sipariş kalıcı olarak silinecek.\nEmin misiniz?",
                    "Sipariş Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _db.SiparisSil(id);
                ListeyiYukle();
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
                    SelectionBackColor = Color.FromArgb(45, 70, 100),
                    SelectionForeColor = Color.White,
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
                EnableHeadersVisualStyles = false,
                MultiSelect = false,
                RowTemplate = { Height = 42 }
            };

            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Id",          HeaderText="#",            AutoSizeMode=DataGridViewAutoSizeColumnMode.AllCells });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Musteri",      HeaderText="Müşteri",      FillWeight=50 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Telefon",      HeaderText="Telefon",      AutoSizeMode=DataGridViewAutoSizeColumnMode.AllCells });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Hayvan",       HeaderText="Hayvan Türü",  AutoSizeMode=DataGridViewAutoSizeColumnMode.AllCells });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Mama",         HeaderText="Mama / Yem",   FillWeight=50 });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Miktar",       HeaderText="Miktar",       AutoSizeMode=DataGridViewAutoSizeColumnMode.AllCells });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="BirimFiyat",   HeaderText="Birim (₺)",    AutoSizeMode=DataGridViewAutoSizeColumnMode.AllCells });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="Toplam",       HeaderText="Toplam (₺)",   AutoSizeMode=DataGridViewAutoSizeColumnMode.AllCells });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="SiparisTar",   HeaderText="Sipariş Tar.", AutoSizeMode=DataGridViewAutoSizeColumnMode.AllCells });
            g.Columns.Add(new DataGridViewTextBoxColumn { Name="TeslimTar",    HeaderText="Teslim Tar.",  AutoSizeMode=DataGridViewAutoSizeColumnMode.AllCells });
            g.Columns.Add(new DataGridViewCheckBoxColumn    { Name="Teslim",    HeaderText="Teslim Edildi",AutoSizeMode=DataGridViewAutoSizeColumnMode.AllCells, ReadOnly=false });
            
            // Grid genel olarak ReadOnly=true ama "Teslim" sütunu interaktif olmalı
            g.ReadOnly = false;
            foreach (DataGridViewColumn col in g.Columns)
            {
                if (col.Name != "Teslim") col.ReadOnly = true;
            }

            return g;
        }

        private static Button YapButon(string text, Point konum, int w, Color bg, Color fg)
        {
            var b = new Button
            {
                Text = text, Location = konum, Size = new Size(w, 32),
                FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = fg,
                Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
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
    }
}
