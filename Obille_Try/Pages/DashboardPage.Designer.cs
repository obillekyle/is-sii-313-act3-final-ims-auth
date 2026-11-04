namespace Obille_Try.Pages
{
    partial class DashboardPage
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
            layout = new Kit.Flex();
            pageHeader = new Kit.PageHeader();
            statsGrid = new Kit.Grid();
            stockValueTile = new Kit.StatTile();
            lowStockTile = new Kit.StatTile();
            toolsTile = new Kit.StatTile();
            cardsGrid = new Kit.Grid();
            flowsCard = new Obille_Try.UI.FlowsCard();
            workCard = new Obille_Try.UI.OpenWorkCard();
            layout.SuspendLayout();
            statsGrid.SuspendLayout();
            cardsGrid.SuspendLayout();
            SuspendLayout();
            //
            // layout
            //
            layout.Controls.Add(pageHeader);
            layout.Controls.Add(statsGrid);
            layout.Controls.Add(cardsGrid);
            layout.Direction = Kit.FlexDirection.Column;
            layout.Dock = DockStyle.Fill;
            layout.Gap = 4;
            layout.SetGrow(cardsGrid, true);
            layout.Location = new Point(24, 8);
            layout.Name = "layout";
            layout.Size = new Size(946, 616);
            layout.TabIndex = 0;
            //
            // pageHeader
            //
            pageHeader.Description = "Malolos yard, this week";
            pageHeader.Location = new Point(6, 4);
            pageHeader.Name = "pageHeader";
            pageHeader.Size = new Size(934, 66);
            pageHeader.TabIndex = 0;
            pageHeader.Title = "Dashboard";
            //
            // statsGrid
            //
            statsGrid.Columns = 3;
            statsGrid.Controls.Add(stockValueTile);
            statsGrid.Controls.Add(lowStockTile);
            statsGrid.Controls.Add(toolsTile);
            statsGrid.Location = new Point(0, 86);
            statsGrid.Name = "statsGrid";
            statsGrid.Size = new Size(946, 112);
            statsGrid.TabIndex = 1;
            //
            // stockValueTile
            //
            stockValueTile.Label = "Stock value";
            stockValueTile.Location = new Point(0, 0);
            stockValueTile.Name = "stockValueTile";
            stockValueTile.Percent = true;
            stockValueTile.Period = "from last week";
            stockValueTile.Size = new Size(312, 112);
            stockValueTile.TabIndex = 0;
            stockValueTile.Value = "₱2.70M";
            //
            // lowStockTile
            //
            lowStockTile.Clickable = true;
            lowStockTile.Label = "Low stock";
            lowStockTile.Location = new Point(317, 0);
            lowStockTile.Name = "lowStockTile";
            lowStockTile.Period = "since last week";
            lowStockTile.Size = new Size(312, 112);
            lowStockTile.TabIndex = 1;
            lowStockTile.Unit = "materials";
            lowStockTile.UnitOne = "material";
            lowStockTile.UpIsGood = false;
            lowStockTile.Value = "6";
            //
            // toolsTile
            //
            toolsTile.Label = "Tools overdue";
            toolsTile.Location = new Point(634, 0);
            toolsTile.Name = "toolsTile";
            toolsTile.Period = "since last week";
            toolsTile.Size = new Size(312, 112);
            toolsTile.TabIndex = 2;
            toolsTile.Unit = "tools";
            toolsTile.UnitOne = "tool";
            toolsTile.UpIsGood = false;
            toolsTile.Value = "3";
            //
            // cardsGrid
            //
            cardsGrid.Columns = 8;
            cardsGrid.Controls.Add(flowsCard);
            cardsGrid.Controls.Add(workCard);
            cardsGrid.FillHeight = true;
            cardsGrid.Location = new Point(0, 214);
            cardsGrid.Name = "cardsGrid";
            cardsGrid.SetSpan(flowsCard, 5);
            cardsGrid.SetSpan(workCard, 3);
            cardsGrid.Size = new Size(946, 402);
            cardsGrid.TabIndex = 2;
            //
            // flowsCard
            //
            flowsCard.Location = new Point(0, 0);
            flowsCard.Name = "flowsCard";
            flowsCard.Size = new Size(581, 402);
            flowsCard.TabIndex = 0;
            flowsCard.Title = "Received and issued, weekly";
            //
            // workCard
            //
            workCard.Items.AddRange(new Obille_Try.UI.WorkItem[] { new Obille_Try.UI.WorkItem("Approved requests", "clipboard-check", "5"), new Obille_Try.UI.WorkItem("Deliveries", "truck", "3"), new Obille_Try.UI.WorkItem("Orders due", "package", "2"), new Obille_Try.UI.WorkItem("Maintenance", "wrench", "2"), new Obille_Try.UI.WorkItem("Purchases", "shopping-cart", "5"), new Obille_Try.UI.WorkItem("Stock recount", "list-checks", "4 of 7") });
            workCard.Location = new Point(597, 0);
            workCard.Name = "workCard";
            workCard.Size = new Size(349, 402);
            workCard.TabIndex = 1;
            workCard.Title = "Open work";
            //
            // DashboardPage
            //
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(layout);
            Name = "DashboardPage";
            Padding = new Padding(18, 4, 18, 12);
            Size = new Size(994, 644);
            layout.ResumeLayout(false);
            statsGrid.ResumeLayout(false);
            cardsGrid.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Kit.Flex layout;
        private Kit.PageHeader pageHeader;
        private Kit.Grid statsGrid;
        private Kit.StatTile stockValueTile;
        private Kit.StatTile lowStockTile;
        private Kit.StatTile toolsTile;
        private Kit.Grid cardsGrid;
        private Obille_Try.UI.FlowsCard flowsCard;
        private Obille_Try.UI.OpenWorkCard workCard;
    }
}
