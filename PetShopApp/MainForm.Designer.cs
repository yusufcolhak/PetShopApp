// ============================================================
// PetShop Sipariş Yönetim Sistemi
// MainForm.Designer.cs  —  Otomatik oluşturulan kısmi sınıf
// (Tüm UI programatik olarak MainForm.cs içinde tanımlandı)
// ============================================================

namespace PetShopApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Tüm arayüz kontrollerı MainForm.cs içindeki ArayuzOlustur() metodunda oluşturulur.
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize    = new Size(1280, 760);
            Name          = "MainForm";
            Text          = "🐾 PetShop Sipariş Yönetim Sistemi";
            ResumeLayout(false);
        }
    }
}
