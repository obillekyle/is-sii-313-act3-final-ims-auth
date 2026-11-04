namespace Obille_Try.Dialogs
{
    partial class DeleteMaterialDialog
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
            body = new Panel();
            subjectLabel = new Kit.KitLabel();
            art = new Obille_Try.UI.DeleteArt();
            footer = new Kit.DialogFooter();
            deleteButton = new Kit.KitButton();
            cancelButton = new Kit.KitButton();
            body.SuspendLayout();
            footer.SuspendLayout();
            SuspendLayout();
            //
            // body
            //
            body.Controls.Add(subjectLabel);
            body.Controls.Add(art);
            body.Dock = DockStyle.Fill;
            body.Location = new Point(20, 90);
            body.Name = "body";
            body.Padding = new Padding(0, 4, 0, 16);
            body.Size = new Size(368, 100);
            body.TabIndex = 0;
            //
            // subjectLabel
            //
            subjectLabel.Dock = DockStyle.Fill;
            subjectLabel.Location = new Point(96, 4);
            subjectLabel.Name = "subjectLabel";
            subjectLabel.Size = new Size(272, 80);
            subjectLabel.Text = "Galvanized steel bar (GSB-0352)";
            subjectLabel.TextAlign = ContentAlignment.MiddleLeft;
            subjectLabel.Wrap = true;
            //
            // art
            //
            art.Dock = DockStyle.Left;
            art.Location = new Point(0, 4);
            art.Name = "art";
            art.Size = new Size(96, 80);
            //
            // footer
            //
            footer.Controls.Add(deleteButton);
            footer.Controls.Add(cancelButton);
            footer.Location = new Point(20, 190);
            footer.Name = "footer";
            footer.Size = new Size(368, 56);
            footer.TabIndex = 1;
            //
            // deleteButton
            //
            deleteButton.AutoSize = true;
            deleteButton.DialogResult = DialogResult.OK;
            deleteButton.Icon = "trash-2";
            deleteButton.Location = new Point(276, 12);
            deleteButton.Name = "deleteButton";
            deleteButton.Size = new Size(92, 32);
            deleteButton.TabIndex = 1;
            deleteButton.Text = "Delete";
            deleteButton.Variant = Kit.ButtonVariant.Danger;
            //
            // cancelButton
            //
            cancelButton.AutoSize = true;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(186, 12);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(82, 32);
            cancelButton.TabIndex = 0;
            cancelButton.Text = "Cancel";
            //
            // DeleteMaterialDialog
            //
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = cancelButton;
            ClientSize = new Size(400, 246);
            Controls.Add(body);
            Controls.Add(footer);
            Description = "Its stock comes off the count. This can't be undone.";
            DialogSize = Kit.DialogSize.Small;
            Name = "DeleteMaterialDialog";
            Text = "Delete material?";
            Title = "Delete material?";
            body.ResumeLayout(false);
            footer.ResumeLayout(false);
            footer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel body;
        private Kit.KitLabel subjectLabel;
        private UI.DeleteArt art;
        private Kit.DialogFooter footer;
        private Kit.KitButton deleteButton;
        private Kit.KitButton cancelButton;
    }
}
