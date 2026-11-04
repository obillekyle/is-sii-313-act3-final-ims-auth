namespace Obille_Try.Pages
{
    partial class MaterialsPage
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            toaster = new Kit.Toaster(components);
            headerRow = new Panel();
            pageHeader = new Kit.PageHeader();
            newButton = new Kit.KitButton();
            toolbar = new Panel();
            filterBar = new Panel();
            filtersFlow = new FlowLayoutPanel();
            categorySelect = new Kit.Select();
            stockSelect = new Kit.Select();
            supplierSelect = new Kit.Select();
            searchBox = new Kit.TextInput();
            bulkBar = new Panel();
            pickedLabel = new Kit.KitLabel();
            clearPickedButton = new Kit.KitButton();
            bulkGap = new Panel();
            deletePickedButton = new Kit.KitButton();
            tableRow = new Panel();
            materialGrid = new Obille_Try.UI.MaterialGrid();
            materialColumn = new DataGridViewTextBoxColumn();
            stockColumn = new DataGridViewTextBoxColumn();
            costColumn = new DataGridViewTextBoxColumn();
            actionsColumn = new DataGridViewTextBoxColumn();
            footerRow = new Panel();
            pagination = new Kit.Pagination();
            headerRow.SuspendLayout();
            pageHeader.SuspendLayout();
            toolbar.SuspendLayout();
            filterBar.SuspendLayout();
            filtersFlow.SuspendLayout();
            bulkBar.SuspendLayout();
            tableRow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)materialGrid).BeginInit();
            footerRow.SuspendLayout();
            SuspendLayout();
            // 
            // toaster
            // 
            toaster.ContainerControl = this;
            // 
            // headerRow
            // 
            headerRow.Controls.Add(pageHeader);
            headerRow.Dock = DockStyle.Top;
            headerRow.Location = new Point(18, 8);
            headerRow.Name = "headerRow";
            headerRow.Padding = new Padding(6, 0, 6, 8);
            headerRow.Size = new Size(958, 74);
            headerRow.TabIndex = 0;
            // 
            // pageHeader
            // 
            pageHeader.Controls.Add(newButton);
            pageHeader.Description = "59 materials in the Malolos yard";
            pageHeader.Dock = DockStyle.Top;
            pageHeader.Location = new Point(6, 0);
            pageHeader.Name = "pageHeader";
            pageHeader.Size = new Size(946, 66);
            pageHeader.TabIndex = 0;
            pageHeader.Title = "Materials";
            // 
            // newButton
            // 
            newButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            newButton.AutoSize = true;
            newButton.Icon = "plus";
            newButton.Location = new Point(816, 2);
            newButton.Name = "newButton";
            newButton.Shortcut = "Ctrl+N";
            newButton.Size = new Size(130, 32);
            newButton.TabIndex = 0;
            newButton.Text = "New material";
            newButton.UseVisualStyleBackColor = false;
            newButton.Variant = Kit.ButtonVariant.Primary;
            // 
            // toolbar
            // 
            toolbar.Controls.Add(filterBar);
            toolbar.Controls.Add(bulkBar);
            toolbar.Dock = DockStyle.Top;
            toolbar.Location = new Point(18, 82);
            toolbar.Name = "toolbar";
            toolbar.Padding = new Padding(6, 0, 6, 12);
            toolbar.Size = new Size(958, 44);
            toolbar.TabIndex = 1;
            // 
            // filterBar
            // 
            filterBar.Controls.Add(filtersFlow);
            filterBar.Controls.Add(searchBox);
            filterBar.Dock = DockStyle.Fill;
            filterBar.Location = new Point(6, 0);
            filterBar.Name = "filterBar";
            filterBar.Size = new Size(946, 32);
            filterBar.TabIndex = 0;
            // 
            // filtersFlow
            // 
            filtersFlow.Controls.Add(categorySelect);
            filtersFlow.Controls.Add(stockSelect);
            filtersFlow.Controls.Add(supplierSelect);
            filtersFlow.Dock = DockStyle.Fill;
            filtersFlow.Location = new Point(0, 0);
            filtersFlow.Margin = new Padding(0);
            filtersFlow.Name = "filtersFlow";
            filtersFlow.Size = new Size(666, 32);
            filtersFlow.TabIndex = 0;
            filtersFlow.WrapContents = false;
            // 
            // categorySelect
            // 
            categorySelect.AccessibleName = "Category";
            categorySelect.Location = new Point(0, 0);
            categorySelect.Margin = new Padding(0, 0, 8, 0);
            categorySelect.Name = "categorySelect";
            categorySelect.Size = new Size(180, 32);
            categorySelect.TabIndex = 0;
            // 
            // stockSelect
            // 
            stockSelect.AccessibleName = "Stock";
            stockSelect.Location = new Point(188, 0);
            stockSelect.Margin = new Padding(0, 0, 8, 0);
            stockSelect.Name = "stockSelect";
            stockSelect.Size = new Size(150, 32);
            stockSelect.TabIndex = 1;
            stockSelect.SelectedIndexChanged += stockSelect_SelectedIndexChanged;
            // 
            // supplierSelect
            // 
            supplierSelect.AccessibleName = "Supplier";
            supplierSelect.Location = new Point(346, 0);
            supplierSelect.Margin = new Padding(0);
            supplierSelect.Name = "supplierSelect";
            supplierSelect.Size = new Size(240, 32);
            supplierSelect.TabIndex = 2;
            // 
            // searchBox
            // 
            searchBox.AccessibleName = "Search materials";
            searchBox.Clearable = true;
            searchBox.Dock = DockStyle.Right;
            searchBox.Location = new Point(666, 0);
            searchBox.Name = "searchBox";
            searchBox.Placeholder = "Search by name, code or bin";
            searchBox.Size = new Size(280, 32);
            searchBox.TabIndex = 1;
            searchBox.Type = Kit.InputType.Search;
            // 
            // bulkBar
            // 
            bulkBar.Controls.Add(pickedLabel);
            bulkBar.Controls.Add(clearPickedButton);
            bulkBar.Controls.Add(bulkGap);
            bulkBar.Controls.Add(deletePickedButton);
            bulkBar.Dock = DockStyle.Fill;
            bulkBar.Location = new Point(6, 0);
            bulkBar.Name = "bulkBar";
            bulkBar.Size = new Size(946, 32);
            bulkBar.TabIndex = 1;
            bulkBar.Visible = false;
            // 
            // pickedLabel
            // 
            pickedLabel.AccessibleRole = AccessibleRole.StaticText;
            pickedLabel.Dock = DockStyle.Fill;
            pickedLabel.Location = new Point(0, 0);
            pickedLabel.Name = "pickedLabel";
            pickedLabel.Size = new Size(671, 32);
            pickedLabel.Strong = true;
            pickedLabel.TabIndex = 0;
            pickedLabel.TabStop = false;
            pickedLabel.Text = "1 selected";
            // 
            // clearPickedButton
            // 
            clearPickedButton.AutoSize = true;
            clearPickedButton.Dock = DockStyle.Right;
            clearPickedButton.Location = new Point(671, 0);
            clearPickedButton.Name = "clearPickedButton";
            clearPickedButton.Size = new Size(116, 32);
            clearPickedButton.TabIndex = 0;
            clearPickedButton.Text = "Clear selection";
            clearPickedButton.UseVisualStyleBackColor = false;
            clearPickedButton.Variant = Kit.ButtonVariant.Ghost;
            // 
            // bulkGap
            // 
            bulkGap.Dock = DockStyle.Right;
            bulkGap.Location = new Point(787, 0);
            bulkGap.Name = "bulkGap";
            bulkGap.Size = new Size(8, 32);
            bulkGap.TabIndex = 1;
            // 
            // deletePickedButton
            // 
            deletePickedButton.AutoSize = true;
            deletePickedButton.Dock = DockStyle.Right;
            deletePickedButton.Icon = "trash-2";
            deletePickedButton.Location = new Point(795, 0);
            deletePickedButton.Name = "deletePickedButton";
            deletePickedButton.Size = new Size(151, 32);
            deletePickedButton.TabIndex = 2;
            deletePickedButton.Text = "Delete 1 material";
            deletePickedButton.UseVisualStyleBackColor = false;
            deletePickedButton.Variant = Kit.ButtonVariant.Danger;
            // 
            // tableRow
            // 
            tableRow.Controls.Add(materialGrid);
            tableRow.Dock = DockStyle.Fill;
            tableRow.Location = new Point(18, 126);
            tableRow.Name = "tableRow";
            tableRow.Padding = new Padding(6, 0, 6, 0);
            tableRow.Size = new Size(958, 452);
            tableRow.TabIndex = 2;
            // 
            // materialGrid
            // 
            materialGrid.AccessibleName = "Materials";
            materialGrid.AllowUserToAddRows = false;
            materialGrid.AllowUserToDeleteRows = false;
            materialGrid.AllowUserToResizeColumns = false;
            materialGrid.AllowUserToResizeRows = false;
            materialGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            materialGrid.Columns.AddRange(new DataGridViewColumn[] { materialColumn, stockColumn, costColumn, actionsColumn });
            materialGrid.Dock = DockStyle.Fill;
            materialGrid.EmptyText = "No materials match these filters.";
            materialGrid.Location = new Point(6, 0);
            materialGrid.MultiSelect = false;
            materialGrid.Name = "materialGrid";
            materialGrid.ReadOnly = true;
            materialGrid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            materialGrid.Selectable = true;
            materialGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            materialGrid.Size = new Size(946, 452);
            materialGrid.StandardTab = true;
            materialGrid.TabIndex = 0;
            // 
            // materialColumn
            // 
            materialColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            materialColumn.DataPropertyName = "Material";
            materialColumn.HeaderText = "Material";
            materialColumn.Name = "materialColumn";
            materialColumn.ReadOnly = true;
            materialColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // stockColumn
            // 
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleRight;
            stockColumn.DataPropertyName = "Stock";
            stockColumn.DefaultCellStyle = dataGridViewCellStyle2;
            stockColumn.HeaderText = "Stock";
            stockColumn.Name = "stockColumn";
            stockColumn.ReadOnly = true;
            stockColumn.Resizable = DataGridViewTriState.False;
            stockColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            stockColumn.Width = 170;
            // 
            // costColumn
            // 
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleRight;
            costColumn.DataPropertyName = "Cost";
            costColumn.DefaultCellStyle = dataGridViewCellStyle3;
            costColumn.HeaderText = "Unit cost";
            costColumn.Name = "costColumn";
            costColumn.ReadOnly = true;
            costColumn.Resizable = DataGridViewTriState.False;
            costColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            costColumn.Width = 150;
            // 
            // actionsColumn
            // 
            actionsColumn.DataPropertyName = "Actions";
            actionsColumn.HeaderText = "";
            actionsColumn.Name = "actionsColumn";
            actionsColumn.ReadOnly = true;
            actionsColumn.Resizable = DataGridViewTriState.False;
            actionsColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            actionsColumn.ToolTipText = "Material actions";
            actionsColumn.Width = 64;
            // 
            // footerRow
            // 
            footerRow.Controls.Add(pagination);
            footerRow.Dock = DockStyle.Bottom;
            footerRow.Location = new Point(18, 578);
            footerRow.Name = "footerRow";
            footerRow.Padding = new Padding(6, 12, 6, 0);
            footerRow.Size = new Size(958, 52);
            footerRow.TabIndex = 3;
            // 
            // pagination
            // 
            pagination.Dock = DockStyle.Fill;
            pagination.Location = new Point(6, 12);
            pagination.Name = "pagination";
            pagination.Size = new Size(946, 36);
            pagination.TabIndex = 0;
            // 
            // MaterialsPage
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(tableRow);
            Controls.Add(footerRow);
            Controls.Add(toolbar);
            Controls.Add(headerRow);
            Name = "MaterialsPage";
            Padding = new Padding(18, 8, 18, 14);
            Size = new Size(994, 644);
            headerRow.ResumeLayout(false);
            pageHeader.ResumeLayout(false);
            pageHeader.PerformLayout();
            toolbar.ResumeLayout(false);
            filterBar.ResumeLayout(false);
            filtersFlow.ResumeLayout(false);
            bulkBar.ResumeLayout(false);
            bulkBar.PerformLayout();
            tableRow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)materialGrid).EndInit();
            footerRow.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Kit.Toaster toaster;
        private Panel headerRow;
        private Kit.PageHeader pageHeader;
        private Kit.KitButton newButton;
        private Panel toolbar;
        private Panel filterBar;
        private FlowLayoutPanel filtersFlow;
        private Kit.Select categorySelect;
        private Kit.Select stockSelect;
        private Kit.Select supplierSelect;
        private Kit.TextInput searchBox;
        private Panel bulkBar;
        private Kit.KitLabel pickedLabel;
        private Kit.KitButton clearPickedButton;
        private Panel bulkGap;
        private Kit.KitButton deletePickedButton;
        private Panel tableRow;
        private UI.MaterialGrid materialGrid;
        private DataGridViewTextBoxColumn materialColumn;
        private DataGridViewTextBoxColumn stockColumn;
        private DataGridViewTextBoxColumn costColumn;
        private DataGridViewTextBoxColumn actionsColumn;
        private Panel footerRow;
        private Kit.Pagination pagination;
    }
}
