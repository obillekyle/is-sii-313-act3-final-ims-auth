namespace Obille_Try
{
    partial class Dash
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
            components = new System.ComponentModel.Container();
            theme = new Kit.ThemeProvider(components);
            sidebar = new Obille_Try.UI.AppSidebar();
            main = new Panel();
            content = new Obille_Try.UI.BorderPanel();
            dashboardPage = new Obille_Try.Pages.DashboardPage();
            materialsPage = new Obille_Try.Pages.MaterialsPage();
            placeholderPage = new Obille_Try.Pages.PlaceholderPage();
            header = new Obille_Try.UI.BorderPanel();
            breadcrumbs = new Kit.Breadcrumbs();
            locationSelect = new Kit.Select();
            headerGap = new Panel();
            searchButton = new Kit.KitButton();
            bellButton = new Kit.MenuButton();
            ((System.ComponentModel.ISupportInitialize)theme).BeginInit();
            main.SuspendLayout();
            content.SuspendLayout();
            header.SuspendLayout();
            SuspendLayout();
            // 
            // theme
            // 
            theme.ContainerControl = this;
            theme.Mode = Kit.ThemeMode.Light;
            // 
            // sidebar
            // 
            sidebar.Dock = DockStyle.Left;
            sidebar.Items.Add(new Kit.NavItem("Dashboard", "house", null, "dashboard"));
            sidebar.Items.Add(new Kit.NavItem("Materials", "boxes", null, "materials"));
            sidebar.Items.Add(new Kit.NavItem("Projects", "hard-hat", null, "projects"));
            sidebar.Items.Add(new Kit.NavItem("Material request", "clipboard-list", null, "requests"));
            sidebar.Items.Add(new Kit.NavItem("Purchasing", "shopping-cart", null, "purchasing"));
            sidebar.Items.Add(new Kit.NavItem("Deliveries", "truck", null, "deliveries"));
            sidebar.Location = new Point(0, 0);
            sidebar.Name = "sidebar";
            sidebar.PersonDetail = "Warehouse custodian";
            sidebar.PersonName = "Cyrus Obille";
            sidebar.Size = new Size(248, 780);
            sidebar.TabIndex = 0;
            sidebar.TabStop = false;
            sidebar.SelectedIndexChanged += navList_SelectedIndexChanged;
            // 
            // main
            // 
            main.Controls.Add(content);
            main.Controls.Add(header);
            main.Dock = DockStyle.Fill;
            main.Location = new Point(248, 0);
            main.Name = "main";
            main.Size = new Size(1032, 780);
            main.TabIndex = 1;
            // 
            // content
            // 
            content.Controls.Add(dashboardPage);
            content.Controls.Add(materialsPage);
            content.Controls.Add(placeholderPage);
            content.Dock = DockStyle.Fill;
            content.Location = new Point(0, 60);
            content.Name = "content";
            content.Size = new Size(1032, 720);
            content.Surface = UI.Surface.Page;
            content.TabIndex = 1;
            // 
            // dashboardPage
            // 
            dashboardPage.Dock = DockStyle.Fill;
            dashboardPage.Location = new Point(0, 0);
            dashboardPage.Name = "dashboardPage";
            dashboardPage.Padding = new Padding(18, 4, 18, 12);
            dashboardPage.Size = new Size(1032, 720);
            dashboardPage.TabIndex = 0;
            dashboardPage.Load += dashboardPage_Load;
            // 
            // materialsPage
            // 
            materialsPage.Dock = DockStyle.Fill;
            materialsPage.Location = new Point(0, 0);
            materialsPage.Name = "materialsPage";
            materialsPage.Padding = new Padding(18, 8, 18, 14);
            materialsPage.Size = new Size(1032, 720);
            materialsPage.TabIndex = 1;
            materialsPage.Visible = false;
            // 
            // placeholderPage
            // 
            placeholderPage.Dock = DockStyle.Fill;
            placeholderPage.Location = new Point(0, 0);
            placeholderPage.Name = "placeholderPage";
            placeholderPage.Padding = new Padding(18, 8, 18, 14);
            placeholderPage.Size = new Size(1032, 720);
            placeholderPage.TabIndex = 2;
            placeholderPage.Visible = false;
            // 
            // header
            // 
            header.Controls.Add(breadcrumbs);
            header.Controls.Add(locationSelect);
            header.Controls.Add(headerGap);
            header.Controls.Add(searchButton);
            header.Controls.Add(bellButton);
            header.Dock = DockStyle.Top;
            header.Edges = AnchorStyles.Bottom;
            header.Location = new Point(0, 0);
            header.Name = "header";
            header.Padding = new Padding(24, 14, 16, 14);
            header.Size = new Size(1032, 60);
            header.TabIndex = 0;
            // 
            // breadcrumbs
            // 
            breadcrumbs.Dock = DockStyle.Fill;
            breadcrumbs.Location = new Point(24, 14);
            breadcrumbs.Name = "breadcrumbs";
            breadcrumbs.Size = new Size(744, 32);
            breadcrumbs.TabIndex = 0;
            breadcrumbs.TabStop = false;
            // 
            // locationSelect
            // 
            locationSelect.AccessibleName = "Location";
            locationSelect.Dock = DockStyle.Right;
            locationSelect.Location = new Point(768, 14);
            locationSelect.Name = "locationSelect";
            locationSelect.Options = new string[]
    {
    "Malolos"
    };
            locationSelect.Size = new Size(176, 32);
            locationSelect.TabIndex = 1;
            // 
            // headerGap
            // 
            headerGap.Dock = DockStyle.Right;
            headerGap.Location = new Point(944, 14);
            headerGap.Name = "headerGap";
            headerGap.Size = new Size(8, 32);
            headerGap.TabIndex = 2;
            // 
            // searchButton
            // 
            searchButton.AccessibleName = "Find a material";
            searchButton.Dock = DockStyle.Right;
            searchButton.Icon = "search";
            searchButton.IconOnly = true;
            searchButton.Location = new Point(952, 14);
            searchButton.Name = "searchButton";
            searchButton.Shortcut = "Ctrl+F";
            searchButton.Size = new Size(32, 32);
            searchButton.TabIndex = 3;
            searchButton.UseVisualStyleBackColor = false;
            searchButton.Variant = Kit.ButtonVariant.Ghost;
            // 
            // bellButton
            // 
            bellButton.AccessibleName = "Notifications";
            bellButton.Dock = DockStyle.Right;
            bellButton.Icon = "bell";
            bellButton.IconOnly = true;
            bellButton.IconRight = "";
            bellButton.Location = new Point(984, 14);
            bellButton.Name = "bellButton";
            bellButton.Size = new Size(32, 32);
            bellButton.TabIndex = 4;
            bellButton.UseVisualStyleBackColor = false;
            bellButton.Variant = Kit.ButtonVariant.Ghost;
            // 
            // Dash
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1280, 780);
            Controls.Add(main);
            Controls.Add(sidebar);
            ForeColor = Color.FromArgb(15, 15, 15);
            KeyPreview = true;
            MinimumSize = new Size(1040, 700);
            Name = "Dash";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SS Construction Inventory";
            ((System.ComponentModel.ISupportInitialize)theme).EndInit();
            main.ResumeLayout(false);
            content.ResumeLayout(false);
            header.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Kit.ThemeProvider theme;
        private UI.AppSidebar sidebar;
        private Panel main;
        private UI.BorderPanel content;
        private Pages.DashboardPage dashboardPage;
        private Pages.MaterialsPage materialsPage;
        private Pages.PlaceholderPage placeholderPage;
        private UI.BorderPanel header;
        private Kit.Breadcrumbs breadcrumbs;
        private Kit.Select locationSelect;
        private Panel headerGap;
        private Kit.KitButton searchButton;
        private Kit.MenuButton bellButton;
    }
}
