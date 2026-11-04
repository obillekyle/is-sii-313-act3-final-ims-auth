namespace Obille_Try.Dialogs
{
    partial class MaterialDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            footer = new Kit.DialogFooter();
            addButton = new Kit.KitButton();
            cancelButton = new Kit.KitButton();
            fields = new TableLayoutPanel();
            nameField = new Kit.Field();
            nameBox = new Kit.TextInput();
            codeField = new Kit.Field();
            codeBox = new Kit.TextInput();
            categoryField = new Kit.Field();
            categoryBox = new Kit.Select();
            unitField = new Kit.Field();
            unitBox = new Kit.Select();
            costField = new Kit.Field();
            costBox = new Kit.TextInput();
            reorderPointField = new Kit.Field();
            reorderPointBox = new Kit.NumberInput();
            reorderQuantityField = new Kit.Field();
            reorderQuantityBox = new Kit.NumberInput();
            supplierField = new Kit.Field();
            supplierBox = new Kit.Select();
            binField = new Kit.Field();
            binBox = new Kit.TextInput();
            footer.SuspendLayout();
            fields.SuspendLayout();
            nameField.SuspendLayout();
            codeField.SuspendLayout();
            categoryField.SuspendLayout();
            unitField.SuspendLayout();
            costField.SuspendLayout();
            reorderPointField.SuspendLayout();
            reorderQuantityField.SuspendLayout();
            supplierField.SuspendLayout();
            binField.SuspendLayout();
            SuspendLayout();
            //
            // footer
            //
            footer.Controls.Add(addButton);
            footer.Controls.Add(cancelButton);
            footer.Location = new Point(20, 436);
            footer.Name = "footer";
            footer.Size = new Size(528, 56);
            footer.TabIndex = 1;
            //
            // addButton
            //
            addButton.AutoSize = true;
            addButton.DialogResult = DialogResult.OK;
            addButton.Location = new Point(408, 12);
            addButton.Name = "addButton";
            addButton.Size = new Size(120, 32);
            addButton.TabIndex = 0;
            addButton.Text = "Add material";
            addButton.Variant = Kit.ButtonVariant.Primary;
            //
            // cancelButton
            //
            cancelButton.AutoSize = true;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(318, 12);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(82, 32);
            cancelButton.TabIndex = 1;
            cancelButton.Text = "Cancel";
            //
            // fields
            //
            fields.AutoSize = true;
            fields.ColumnCount = 2;
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            fields.Controls.Add(nameField, 0, 0);
            fields.Controls.Add(codeField, 0, 1);
            fields.Controls.Add(categoryField, 1, 1);
            fields.Controls.Add(unitField, 0, 2);
            fields.Controls.Add(costField, 1, 2);
            fields.Controls.Add(reorderPointField, 0, 3);
            fields.Controls.Add(reorderQuantityField, 1, 3);
            fields.Controls.Add(supplierField, 0, 4);
            fields.Controls.Add(binField, 1, 4);
            fields.Dock = DockStyle.Top;
            fields.Location = new Point(20, 56);
            fields.Name = "fields";
            fields.RowCount = 5;
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.Size = new Size(528, 380);
            fields.TabIndex = 0;
            //
            // nameField
            //
            fields.SetColumnSpan(nameField, 2);
            nameField.Controls.Add(nameBox);
            nameField.Dock = DockStyle.Fill;
            nameField.Label = "Name";
            nameField.Location = new Point(0, 0);
            nameField.Margin = new Padding(0, 0, 0, 12);
            nameField.Name = "nameField";
            nameField.Size = new Size(528, 64);
            nameField.TabIndex = 0;
            //
            // nameBox
            //
            nameBox.Location = new Point(0, 26);
            nameBox.Name = "nameBox";
            nameBox.Placeholder = "Galvanized steel bar, 12 mm";
            nameBox.Size = new Size(528, 32);
            nameBox.TabIndex = 0;
            //
            // codeField
            //
            codeField.Controls.Add(codeBox);
            codeField.Dock = DockStyle.Fill;
            codeField.Label = "Code";
            codeField.Location = new Point(0, 76);
            codeField.Margin = new Padding(0, 0, 8, 12);
            codeField.Name = "codeField";
            codeField.Size = new Size(256, 64);
            codeField.TabIndex = 1;
            //
            // codeBox
            //
            codeBox.Location = new Point(0, 26);
            codeBox.Mono = true;
            codeBox.Name = "codeBox";
            codeBox.Size = new Size(256, 32);
            codeBox.TabIndex = 0;
            codeBox.Text = "STL-0302";
            //
            // categoryField
            //
            categoryField.Controls.Add(categoryBox);
            categoryField.Dock = DockStyle.Fill;
            categoryField.Label = "Category";
            categoryField.Location = new Point(272, 76);
            categoryField.Margin = new Padding(8, 0, 0, 12);
            categoryField.Name = "categoryField";
            categoryField.Size = new Size(256, 64);
            categoryField.TabIndex = 2;
            //
            // categoryBox
            //
            categoryBox.Location = new Point(0, 26);
            categoryBox.Name = "categoryBox";
            categoryBox.Placeholder = "Choose a category";
            categoryBox.Size = new Size(256, 32);
            categoryBox.TabIndex = 0;
            //
            // unitField
            //
            unitField.Controls.Add(unitBox);
            unitField.Dock = DockStyle.Fill;
            unitField.Label = "Counted in";
            unitField.Location = new Point(0, 152);
            unitField.Margin = new Padding(0, 0, 8, 12);
            unitField.Name = "unitField";
            unitField.Size = new Size(256, 64);
            unitField.TabIndex = 3;
            //
            // unitBox
            //
            unitBox.Location = new Point(0, 26);
            unitBox.Name = "unitBox";
            unitBox.Placeholder = "Choose a unit";
            unitBox.Size = new Size(256, 32);
            unitBox.TabIndex = 0;
            //
            // costField
            //
            costField.Controls.Add(costBox);
            costField.Dock = DockStyle.Fill;
            costField.Label = "Standard cost per piece, in pesos";
            costField.Location = new Point(272, 152);
            costField.Margin = new Padding(8, 0, 0, 12);
            costField.Name = "costField";
            costField.Size = new Size(256, 64);
            costField.TabIndex = 4;
            //
            // costBox
            //
            costBox.Location = new Point(0, 26);
            costBox.Name = "costBox";
            costBox.Placeholder = "0.00";
            costBox.Size = new Size(256, 32);
            costBox.TabIndex = 0;
            //
            // reorderPointField
            //
            reorderPointField.Controls.Add(reorderPointBox);
            reorderPointField.Dock = DockStyle.Fill;
            reorderPointField.Hint = "The stock at which the material shows as low.";
            reorderPointField.Label = "Reorder point";
            reorderPointField.Location = new Point(0, 228);
            reorderPointField.Margin = new Padding(0, 0, 8, 12);
            reorderPointField.Name = "reorderPointField";
            reorderPointField.Size = new Size(256, 64);
            reorderPointField.TabIndex = 5;
            //
            // reorderPointBox
            //
            reorderPointBox.Location = new Point(0, 26);
            reorderPointBox.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            reorderPointBox.Name = "reorderPointBox";
            reorderPointBox.Size = new Size(256, 32);
            reorderPointBox.TabIndex = 0;
            //
            // reorderQuantityField
            //
            reorderQuantityField.Controls.Add(reorderQuantityBox);
            reorderQuantityField.Dock = DockStyle.Fill;
            reorderQuantityField.Hint = "How many to order when stock reaches the reorder point.";
            reorderQuantityField.Label = "Reorder quantity";
            reorderQuantityField.Location = new Point(272, 228);
            reorderQuantityField.Margin = new Padding(8, 0, 0, 12);
            reorderQuantityField.Name = "reorderQuantityField";
            reorderQuantityField.Size = new Size(256, 64);
            reorderQuantityField.TabIndex = 6;
            //
            // reorderQuantityBox
            //
            reorderQuantityBox.Location = new Point(0, 26);
            reorderQuantityBox.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            reorderQuantityBox.Name = "reorderQuantityBox";
            reorderQuantityBox.Size = new Size(256, 32);
            reorderQuantityBox.TabIndex = 0;
            //
            // supplierField
            //
            supplierField.Controls.Add(supplierBox);
            supplierField.Dock = DockStyle.Fill;
            supplierField.Label = "Supplier";
            supplierField.Location = new Point(0, 304);
            supplierField.Margin = new Padding(0, 0, 8, 0);
            supplierField.Name = "supplierField";
            supplierField.Optional = true;
            supplierField.Size = new Size(256, 64);
            supplierField.TabIndex = 7;
            //
            // supplierBox
            //
            supplierBox.Location = new Point(0, 26);
            supplierBox.Name = "supplierBox";
            supplierBox.Placeholder = "Choose a supplier";
            supplierBox.Size = new Size(256, 32);
            supplierBox.TabIndex = 0;
            //
            // binField
            //
            binField.Controls.Add(binBox);
            binField.Dock = DockStyle.Fill;
            binField.Label = "Storage bin";
            binField.Location = new Point(272, 304);
            binField.Margin = new Padding(8, 0, 0, 0);
            binField.Name = "binField";
            binField.Optional = true;
            binField.Size = new Size(256, 64);
            binField.TabIndex = 8;
            //
            // binBox
            //
            binBox.Location = new Point(0, 26);
            binBox.Name = "binBox";
            binBox.Placeholder = "Steel rack 4";
            binBox.Size = new Size(256, 32);
            binBox.TabIndex = 0;
            //
            // MaterialDialog
            //
            AcceptButton = addButton;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = cancelButton;
            ClientSize = new Size(560, 520);
            Controls.Add(fields);
            Controls.Add(footer);
            Name = "MaterialDialog";
            Text = "New material";
            Title = "New material";
            footer.ResumeLayout(false);
            footer.PerformLayout();
            fields.ResumeLayout(false);
            nameField.ResumeLayout(false);
            codeField.ResumeLayout(false);
            categoryField.ResumeLayout(false);
            unitField.ResumeLayout(false);
            costField.ResumeLayout(false);
            reorderPointField.ResumeLayout(false);
            reorderQuantityField.ResumeLayout(false);
            supplierField.ResumeLayout(false);
            binField.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Kit.DialogFooter footer;
        private Kit.KitButton addButton;
        private Kit.KitButton cancelButton;
        private TableLayoutPanel fields;
        private Kit.Field nameField;
        private Kit.TextInput nameBox;
        private Kit.Field codeField;
        private Kit.TextInput codeBox;
        private Kit.Field categoryField;
        private Kit.Select categoryBox;
        private Kit.Field unitField;
        private Kit.Select unitBox;
        private Kit.Field costField;
        private Kit.TextInput costBox;
        private Kit.Field reorderPointField;
        private Kit.NumberInput reorderPointBox;
        private Kit.Field reorderQuantityField;
        private Kit.NumberInput reorderQuantityBox;
        private Kit.Field supplierField;
        private Kit.Select supplierBox;
        private Kit.Field binField;
        private Kit.TextInput binBox;
    }
}
