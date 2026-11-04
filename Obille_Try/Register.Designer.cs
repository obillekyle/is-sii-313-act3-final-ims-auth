namespace Obille_Try
{
    partial class Register
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
            theme = new Kit.ThemeProvider(components);
            brand = new Obille_Try.UI.BrandPanel();
            stage = new Kit.Flex();
            column = new Kit.Flex();
            heading = new Kit.PageHeader();
            problem = new Kit.Alert();
            fullNameField = new Kit.Field();
            txtFullName = new Kit.TextInput();
            usernameField = new Kit.Field();
            txtUsername = new Kit.TextInput();
            passwordField = new Kit.Field();
            txtPassword = new Kit.TextInput();
            confirmField = new Kit.Field();
            txtConfirmPassword = new Kit.TextInput();
            btnRegister = new Kit.KitButton();
            signInRow = new Kit.Flex();
            signInPrompt = new Kit.KitLabel();
            btnBack = new Kit.KitButton();
            ((System.ComponentModel.ISupportInitialize)theme).BeginInit();
            stage.SuspendLayout();
            column.SuspendLayout();
            fullNameField.SuspendLayout();
            usernameField.SuspendLayout();
            passwordField.SuspendLayout();
            confirmField.SuspendLayout();
            signInRow.SuspendLayout();
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
            column.Controls.Add(fullNameField);
            column.Controls.Add(usernameField);
            column.Controls.Add(passwordField);
            column.Controls.Add(confirmField);
            column.Controls.Add(btnRegister);
            column.Controls.Add(signInRow);
            column.Direction = Kit.FlexDirection.Column;
            column.Gap = 4;
            column.Location = new Point(150, 50);
            column.Name = "column";
            column.Size = new Size(340, 600);
            column.TabIndex = 0;
            //
            // heading
            //
            heading.Description = "Register to use the inventory system.";
            heading.Location = new Point(0, 0);
            heading.Name = "heading";
            heading.Size = new Size(340, 66);
            heading.TabIndex = 0;
            heading.Title = "Create an account";
            //
            // problem
            //
            problem.Location = new Point(0, 82);
            problem.Name = "problem";
            problem.Size = new Size(340, 44);
            problem.Tone = Kit.Tone.Danger;
            problem.Visible = false;
            //
            // fullNameField
            //
            fullNameField.Controls.Add(txtFullName);
            fullNameField.Label = "Full name";
            fullNameField.Location = new Point(0, 82);
            fullNameField.Name = "fullNameField";
            fullNameField.Size = new Size(340, 64);
            fullNameField.TabIndex = 1;
            //
            // txtFullName
            //
            txtFullName.ControlSize = Kit.ControlSize.Large;
            txtFullName.Location = new Point(0, 24);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(340, 40);
            txtFullName.TabIndex = 0;
            txtFullName.TextChanged += txtFullName_TextChanged;
            //
            // usernameField
            //
            usernameField.Controls.Add(txtUsername);
            usernameField.Label = "Username";
            usernameField.Location = new Point(0, 162);
            usernameField.Name = "usernameField";
            usernameField.Size = new Size(340, 64);
            usernameField.TabIndex = 2;
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
            passwordField.Help = "Use at least 8 characters.";
            passwordField.Label = "Password";
            passwordField.Location = new Point(0, 242);
            passwordField.Name = "passwordField";
            passwordField.Size = new Size(340, 64);
            passwordField.TabIndex = 3;
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
            // confirmField
            //
            confirmField.Controls.Add(txtConfirmPassword);
            confirmField.Label = "Confirm password";
            confirmField.Location = new Point(0, 322);
            confirmField.Name = "confirmField";
            confirmField.Size = new Size(340, 64);
            confirmField.TabIndex = 4;
            //
            // txtConfirmPassword
            //
            txtConfirmPassword.ControlSize = Kit.ControlSize.Large;
            txtConfirmPassword.Location = new Point(0, 24);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new Size(340, 40);
            txtConfirmPassword.TabIndex = 0;
            txtConfirmPassword.Type = Kit.InputType.Password;
            txtConfirmPassword.TextChanged += txtConfirmPassword_TextChanged;
            //
            // btnRegister
            //
            btnRegister.ControlSize = Kit.ControlSize.Large;
            btnRegister.Location = new Point(0, 402);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(340, 40);
            btnRegister.TabIndex = 5;
            btnRegister.Text = "Create account";
            btnRegister.Variant = Kit.ButtonVariant.Primary;
            btnRegister.Click += btnRegister_Click;
            //
            // signInRow
            //
            signInRow.Align = Kit.FlexAlign.Center;
            signInRow.Controls.Add(signInPrompt);
            signInRow.Controls.Add(btnBack);
            signInRow.Location = new Point(0, 458);
            signInRow.Name = "signInRow";
            signInRow.Size = new Size(340, 32);
            signInRow.TabIndex = 6;
            //
            // signInPrompt
            //
            signInPrompt.Location = new Point(0, 6);
            signInPrompt.Name = "signInPrompt";
            signInPrompt.Quiet = true;
            signInPrompt.Size = new Size(160, 20);
            signInPrompt.Text = "Already have an account?";
            signInPrompt.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnBack
            //
            btnBack.AutoSize = true;
            btnBack.Location = new Point(168, 0);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(70, 32);
            btnBack.TabIndex = 0;
            btnBack.Text = "Sign in";
            btnBack.Variant = Kit.ButtonVariant.Secondary;
            btnBack.Click += btnBack_Click;
            //
            // Register
            //
            AcceptButton = btnRegister;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1000, 700);
            Controls.Add(stage);
            Controls.Add(brand);
            DoubleBuffered = true;
            MinimumSize = new Size(880, 740);
            Name = "Register";
            Text = "Create an account";
            ((System.ComponentModel.ISupportInitialize)theme).EndInit();
            stage.ResumeLayout(false);
            column.ResumeLayout(false);
            fullNameField.ResumeLayout(false);
            usernameField.ResumeLayout(false);
            passwordField.ResumeLayout(false);
            confirmField.ResumeLayout(false);
            signInRow.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Kit.ThemeProvider theme;
        private UI.BrandPanel brand;
        private Kit.Flex stage;
        private Kit.Flex column;
        private Kit.PageHeader heading;
        private Kit.Alert problem;
        private Kit.Field fullNameField;
        private Kit.TextInput txtFullName;
        private Kit.Field usernameField;
        private Kit.TextInput txtUsername;
        private Kit.Field passwordField;
        private Kit.TextInput txtPassword;
        private Kit.Field confirmField;
        private Kit.TextInput txtConfirmPassword;
        private Kit.KitButton btnRegister;
        private Kit.Flex signInRow;
        private Kit.KitLabel signInPrompt;
        private Kit.KitButton btnBack;
    }
}
