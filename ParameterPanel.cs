using System;
using System.Drawing;
using System.Windows.Forms;
using Inventor;
using TextBox = System.Windows.Forms.TextBox;
using Color = System.Drawing.Color;

namespace InventorParameterDock
{
    public sealed class ParameterPanel : UserControl
    {
        private readonly Inventor.Application app;
        private readonly DataGridView grid = new DataGridView();
        private readonly Label header = new Label();
        private readonly TextBox name = new TextBox();
        private readonly TextBox expression = new TextBox();
        private readonly Button add = new Button();
        private bool loading;

        public ParameterPanel(Inventor.Application inventor)
        {
            app = inventor;
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(247, 248, 250);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1,
                RowCount = 4, Padding = new Padding(8) };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            header.Dock = DockStyle.Fill;
            header.Text = "Nincs aktív alkatrész";
            header.AutoEllipsis = true;
            header.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.RowHeadersVisible = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Param", HeaderText = "Név", ReadOnly = true, FillWeight = 28 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Expression", HeaderText = "Kifejezés", FillWeight = 50 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Units", HeaderText = "Egység", ReadOnly = true, FillWeight = 22 });
            grid.CellEndEdit += OnCellEndEdit;
            var entry = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            entry.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
            entry.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            entry.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
            name.Dock = DockStyle.Fill; name.PlaceholderFallback("Név (pl. a)");
            expression.Dock = DockStyle.Fill; expression.PlaceholderFallback("18 mm");
            add.Text = "+ Hozzáad"; add.Dock = DockStyle.Fill;
            add.Click += AddParameter;
            entry.Controls.Add(name, 0, 0);
            entry.Controls.Add(expression, 1, 0);
            entry.Controls.Add(add, 2, 0);
            var refresh = new Button { Text = "Frissítés", Dock = DockStyle.Right, Width = 100 };
            refresh.Click += (s, e) => RefreshParameters();
            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(grid, 0, 1);
            layout.Controls.Add(entry, 0, 2);
            layout.Controls.Add(refresh, 0, 3);
            Controls.Add(layout);
            MinimumSize = new Size(260, 160);
        }

        private PartDocument ActivePart()
        {
            // In-place component editing: prefer the edited part over the assembly.
            var edit = app.ActiveEditObject as PartDocument;
            if (edit != null) return edit;
            return app.ActiveDocument as PartDocument;
        }

        public void RefreshParameters()
        {
            if (IsDisposed) return;
            loading = true;
            try
            {
                grid.Rows.Clear();
                var part = ActivePart();
                header.Text = part == null ? "Nincs aktív IPT (alkatrész)" : part.DisplayName;
                grid.Enabled = add.Enabled = part != null;
                if (part == null) return;
                UserParameters parameters = part.ComponentDefinition.Parameters.UserParameters;
                foreach (UserParameter p in parameters)
                    grid.Rows.Add(p.Name, p.Expression, p.get_Units());
            }
            catch (Exception ex) { header.Text = "Olvasási hiba: " + ex.Message; }
            finally { loading = false; }
        }

        private void OnCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (loading || e.RowIndex < 0 || e.ColumnIndex != 1) return;
            var part = ActivePart();
            if (part == null) return;
            string key = Convert.ToString(grid.Rows[e.RowIndex].Cells[0].Value);
            string formula = Convert.ToString(grid.Rows[e.RowIndex].Cells[1].Value);
            try
            {
                part.ComponentDefinition.Parameters.UserParameters[key].Expression = formula;
                part.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Érvénytelen kifejezés vagy nem módosítható paraméter:\n" + ex.Message,
                    "Paraméterhiba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            RefreshParameters();
        }

        private void AddParameter(object sender, EventArgs e)
        {
            var part = ActivePart();
            if (part == null) return;
            string key = name.Text.Trim();
            string formula = expression.Text.Trim();
            try
            {
                if (key.Length == 0 || formula.Length == 0)
                    throw new ArgumentException("Adj meg nevet és kifejezést.");
                // First release: newly created parameters are lengths in millimetres.
                part.ComponentDefinition.Parameters.UserParameters.AddByExpression(key, formula, "mm");
                part.Update();
                name.Clear(); expression.Clear();
                RefreshParameters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nem sikerült létrehozni a paramétert:\n" + ex.Message,
                    "Paraméterhiba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
    internal static class TextBoxExtensions
    {
        // Compatible with .NET Framework 4.8 (no TextBox.PlaceholderText).
        internal static void PlaceholderFallback(this TextBox box, string hint)
        {
            box.AccessibleDescription = hint;
        }
    }
}
