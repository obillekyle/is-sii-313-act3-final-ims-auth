namespace Obille_Try.Pages
{
    partial class PlaceholderPage
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            headerRow = new Panel();
            pageHeader = new Kit.PageHeader();
            emptyState = new Kit.EmptyState();
            headerRow.SuspendLayout();
            SuspendLayout();
            //
            // headerRow
            //
            headerRow.Controls.Add(pageHeader);
            headerRow.Dock = DockStyle.Top;
            headerRow.Location = new Point(18, 8);
            headerRow.Name = "headerRow";
            headerRow.Padding = new Padding(6, 0, 6, 8);
            headerRow.Size = new Size(958, 52);
            headerRow.TabIndex = 0;
            //
            // pageHeader
            //
            pageHeader.Dock = DockStyle.Top;
            pageHeader.Location = new Point(6, 0);
            pageHeader.Name = "pageHeader";
            pageHeader.Size = new Size(946, 44);
            pageHeader.TabIndex = 0;
            pageHeader.Title = "Projects";
            //
            // emptyState
            //
            emptyState.ActionText = "Go to the dashboard";
            emptyState.Dock = DockStyle.Fill;
            emptyState.Icon = "hard-hat";
            emptyState.Location = new Point(18, 60);
            emptyState.Name = "emptyState";
            emptyState.Size = new Size(958, 570);
            emptyState.TabIndex = 1;
            emptyState.Text = "The sketches draw the dashboard and the materials list.";
            emptyState.Title = "Projects is not in this prototype";
            //
            // PlaceholderPage
            //
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(emptyState);
            Controls.Add(headerRow);
            Name = "PlaceholderPage";
            Padding = new Padding(18, 8, 18, 14);
            Size = new Size(994, 644);
            headerRow.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel headerRow;
        private Kit.PageHeader pageHeader;
        private Kit.EmptyState emptyState;
    }
}
