using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZwcadPlugin
{
    /// <summary>
    /// US-697 — Formulario WinForms que muestra el resumen de detección
    /// (esquinas L y muros rectos) y solicita la altura del muro en metros
    /// (default 2.70 m) antes de extruir las polilíneas en ZWCAD.
    /// </summary>
    public class FormularioConfigMuros : Form
    {
        private readonly NumericUpDown _numAltura;
        private readonly Button _btnAceptar;
        private readonly Button _btnCancelar;

        /// <summary>Altura del muro en METROS (con 2 decimales) confirmada por el usuario.</summary>
        public decimal AlturaMuroMetros
        {
            get { return _numAltura.Value; }
        }

        public FormularioConfigMuros(int totalEsquinas, int totalMuros, decimal alturaInicialM = 2.70m)
        {
            // -------------------- Form --------------------
            Text            = "Configuración de muros — Tandem 2026";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterScreen;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ClientSize      = new Size(380, 230);
            Font            = new Font("Segoe UI", 9F);

            // -------------------- Cabecera --------------------
            var lblTitulo = new Label
            {
                Text      = "Resumen de detección",
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(20, 15),
                ForeColor = Color.FromArgb(0, 90, 158)
            };

            // -------------------- Resumen (read-only) --------------------
            var lblEsquinas = new Label
            {
                Text     = string.Format("• Esquinas L detectadas:  {0}", totalEsquinas),
                AutoSize = true,
                Location = new Point(35, 50)
            };
            var lblMuros = new Label
            {
                Text     = string.Format("• Muros rectos detectados: {0}", totalMuros),
                AutoSize = true,
                Location = new Point(35, 75)
            };

            // -------------------- Input altura --------------------
            var lblAltura = new Label
            {
                Text     = "Altura del muro (metros):",
                AutoSize = true,
                Location = new Point(20, 120)
            };
            _numAltura = new NumericUpDown
            {
                Location      = new Point(200, 117),
                Size          = new Size(100, 25),
                Minimum       = 0.10m,
                Maximum       = 10.00m,
                DecimalPlaces = 2,
                Increment     = 0.10m,
                Value         = alturaInicialM,
                TextAlign     = HorizontalAlignment.Right
            };

            // -------------------- Botones --------------------
            _btnAceptar = new Button
            {
                Text         = "Aceptar",
                DialogResult = DialogResult.OK,
                Size         = new Size(100, 30),
                Location     = new Point(170, 175),
                BackColor    = Color.FromArgb(0, 120, 215),
                ForeColor    = Color.White,
                FlatStyle    = FlatStyle.Flat
            };
            _btnAceptar.FlatAppearance.BorderSize = 0;

            _btnCancelar = new Button
            {
                Text         = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Size         = new Size(100, 30),
                Location     = new Point(280, 175)
            };

            AcceptButton = _btnAceptar;
            CancelButton = _btnCancelar;

            Controls.Add(lblTitulo);
            Controls.Add(lblEsquinas);
            Controls.Add(lblMuros);
            Controls.Add(lblAltura);
            Controls.Add(_numAltura);
            Controls.Add(_btnAceptar);
            Controls.Add(_btnCancelar);
        }
    }
}
