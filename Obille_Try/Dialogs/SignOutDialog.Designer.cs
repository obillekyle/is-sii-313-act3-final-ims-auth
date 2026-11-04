namespace Obille_Try.Dialogs
{
    partial class SignOutDialog
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
            signOutButton = new Kit.KitButton();
            cancelButton = new Kit.KitButton();
            footer.SuspendLayout();
            SuspendLayout();
            //
            // footer
            //
            footer.Controls.Add(signOutButton);
            footer.Controls.Add(cancelButton);
            footer.Location = new Point(20, 90);
            footer.Name = "footer";
            footer.Size = new Size(368, 56);
            footer.TabIndex = 0;
            //
            // signOutButton
            //
            signOutButton.AutoSize = true;
            signOutButton.DialogResult = DialogResult.OK;
            signOutButton.Location = new Point(262, 12);
            signOutButton.Name = "signOutButton";
            signOutButton.Size = new Size(106, 32);
            signOutButton.TabIndex = 1;
            signOutButton.Text = "Sign out";
            signOutButton.Variant = Kit.ButtonVariant.Primary;
            //
            // cancelButton
            //
            cancelButton.AutoSize = true;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(172, 12);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(82, 32);
            cancelButton.TabIndex = 0;
            cancelButton.Text = "Cancel";
            //
            // SignOutDialog
            //
            AcceptButton = signOutButton;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = cancelButton;
            ClientSize = new Size(400, 146);
            Controls.Add(footer);
            Description = "You will need your password to sign in again.";
            DialogSize = Kit.DialogSize.Small;
            Name = "SignOutDialog";
            Text = "Sign out?";
            Title = "Sign out?";
            footer.ResumeLayout(false);
            footer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Kit.DialogFooter footer;
        private Kit.KitButton signOutButton;
        private Kit.KitButton cancelButton;
    }
}
