namespace Obille_Try
{
    partial class Login
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            theme = new Kit.ThemeProvider(components);
            brand = new Obille_Try.UI.BrandPanel();
            stage = new Kit.Flex();
            column = new Kit.Flex();
            heading = new Kit.PageHeader();
            problem = new Kit.Alert();
            usernameField = new Kit.Field();
            txtUsername = new Kit.TextInput();
            passwordField = new Kit.Field();
            txtPassword = new Kit.TextInput();
            btnSignIn = new Kit.KitButton();
            registerRow = new Kit.Flex();
            registerPrompt = new Kit.KitLabel();
            btnRegister = new Kit.KitButton();
            forgotNote = new Kit.KitLabel();
            ((System.ComponentModel.ISupportInitialize)theme).BeginInit();
            stage.SuspendLayout();
            column.SuspendLayout();
            usernameField.SuspendLayout();
            passwordField.SuspendLayout();
            registerRow.SuspendLayout();
            SuspendLayout();
            //
            // theme
            //
            theme.ContainerControl = this;
            theme.Mode = Kit.ThemeMode.Light;
            //
            // brand
            //
            brand.Dock = DockStyle.Left;
            brand.Location = new Point(0, 0);
            brand.Name = "brand";
            brand.Size = new Size(360, 700);
            //
            // stage
            //
            stage.Align = Kit.FlexAlign.Center;
            stage.Controls.Add(column);
            stage.Direction = Kit.FlexDirection.Column;
            stage.Dock = DockStyle.Fill;
            stage.Justify = Kit.FlexJustify.Center;
            stage.Location = new Point(360, 0);
            stage.Name = "stage";
            stage.Size = new Size(640, 700);
            stage.TabIndex = 0;
            //
            // column
            //
            column.Controls.Add(heading);
            column.Controls.Add(problem);
            column.Controls.Add(usernameField);
            column.Controls.Add(passwordField);
            column.Controls.Add(btnSignIn);
            column.Controls.Add(registerRow);
            column.Controls.Add(forgotNote);
            column.Direction = Kit.FlexDirection.Column;
            column.Gap = 4;
            column.Location = new Point(150, 115);
            column.Name = "column";
            column.Size = new Size(340, 470);
            column.TabIndex = 0;
            //
            // heading
            //
            heading.Description = "Sign in with your username and password.";
            heading.Location = new Point(0, 0);
            heading.Name = "heading";
            heading.Size = new Size(340, 66);
            heading.TabIndex = 0;
            heading.Title = "Welcome back";
            //
            // problem
            //
            problem.Location = new Point(0, 82);
            problem.Name = "problem";
            problem.Size = new Size(340, 44);
            problem.Tone = Kit.Tone.Danger;
            problem.Visible = false;
            //
            // usernameField
            //
            usernameField.Controls.Add(txtUsername);
            usernameField.Label = "Username";
            usernameField.Location = new Point(0, 82);
            usernameField.Name = "usernameField";
            usernameField.Size = new Size(340, 64);
            usernameField.TabIndex = 1;
            //
            // txtUsername
            //
            txtUsername.ControlSize = Kit.ControlSize.Large;
            txtUsername.Location = new Point(0, 24);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(340, 40);
            txtUsername.TabIndex = 0;
            txtUsername.TextChanged += txtUsername_TextChanged;
            //
            // passwordField
            //
            passwordField.Controls.Add(txtPassword);
            passwordField.Label = "Password";
            passwordField.Location = new Point(0, 162);
            passwordField.Name = "passwordField";
            passwordField.Size = new Size(340, 64);
            passwordField.TabIndex = 2;
            //
            // txtPassword
            //
            txtPassword.ControlSize = Kit.ControlSize.Large;
            txtPassword.Location = new Point(0, 24);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(340, 40);
            txtPassword.TabIndex = 0;
            txtPassword.Type = Kit.InputType.Password;
            txtPassword.TextChanged += txtPassword_TextChanged;
            //
            // btnSignIn
            //
            btnSignIn.ControlSize = Kit.ControlSize.Large;
            btnSignIn.Location = new Point(0, 242);
            btnSignIn.Name = "btnSignIn";
            btnSignIn.Size = new Size(340, 40);
            btnSignIn.TabIndex = 3;
            btnSignIn.Text = "Sign in";
            btnSignIn.Variant = Kit.ButtonVariant.Primary;
            btnSignIn.Click += btnSignIn_Click;
            //
            // registerRow
            //
            registerRow.Align = Kit.FlexAlign.Center;
            registerRow.Controls.Add(registerPrompt);
            registerRow.Controls.Add(btnRegister);
            registerRow.Location = new Point(0, 298);
            registerRow.Name = "registerRow";
            registerRow.Size = new Size(340, 32);
            registerRow.TabIndex = 4;
            //
            // registerPrompt
            //
            registerPrompt.Location = new Point(0, 6);
            registerPrompt.Name = "registerPrompt";
            registerPrompt.Quiet = true;
            registerPrompt.Size = new Size(130, 20);
            registerPrompt.Text = "No account yet?";
            registerPrompt.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnRegister
            //
            btnRegister.AutoSize = true;
            btnRegister.Location = new Point(138, 0);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(140, 32);
            btnRegister.TabIndex = 0;
            btnRegister.Text = "Create account";
            btnRegister.Variant = Kit.ButtonVariant.Secondary;
            btnRegister.Click += btnRegister_Click;
            //
            // forgotNote
            //
            forgotNote.Location = new Point(0, 346);
            forgotNote.Name = "forgotNote";
            forgotNote.Quiet = true;
            forgotNote.Size = new Size(340, 20);
            forgotNote.Text = "Forgot your password? Ask a system admin to reset it.";
            forgotNote.TextSize = 13;
            forgotNote.Wrap = true;
            //
            // Login
            //
            AcceptButton = btnSignIn;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1000, 700);
            Controls.Add(stage);
            Controls.Add(brand);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(880, 740);
            Name = "Login";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Sign in to SS Construction";
            ((System.ComponentModel.ISupportInitialize)theme).EndInit();
            stage.ResumeLayout(false);
            column.ResumeLayout(false);
            usernameField.ResumeLayout(false);
            passwordField.ResumeLayout(false);
            registerRow.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Kit.ThemeProvider theme;
        private UI.BrandPanel brand;
        private Kit.Flex stage;
        private Kit.Flex column;
        private Kit.PageHeader heading;
        private Kit.Alert problem;
        private Kit.Field usernameField;
        private Kit.TextInput txtUsername;
        private Kit.Field passwordField;
        private Kit.TextInput txtPassword;
        private Kit.KitButton btnSignIn;
        private Kit.Flex registerRow;
        private Kit.KitLabel registerPrompt;
        private Kit.KitButton btnRegister;
        private Kit.KitLabel forgotNote;
    }
}
