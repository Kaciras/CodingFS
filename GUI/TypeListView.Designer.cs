namespace CodingFS.GUI
{
	partial class TypeListView
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
			ColumnHeader columnHeader1;
			ColumnHeader columnHeader2;
			combo = new ComboBox();
			listView = new ListView();
			addFolderButton = new Button();
			label4 = new Label();
			addFilesButton = new Button();
			columnHeader1 = new ColumnHeader();
			columnHeader2 = new ColumnHeader();
			SuspendLayout();
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Path";
			columnHeader1.Width = 510;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Type";
			columnHeader2.Width = 120;
			// 
			// combo
			// 
			combo.DropDownStyle = ComboBoxStyle.DropDownList;
			combo.FormattingEnabled = true;
			combo.Items.AddRange(new object[] { "Dependency", "Generated" });
			combo.Location = new Point(259, 101);
			combo.Name = "combo";
			combo.Size = new Size(151, 28);
			combo.TabIndex = 0;
			combo.Visible = false;
			combo.SelectedValueChanged += Combo_SelectedValueChanged;
			combo.Leave += Combo_SelectedValueChanged;
			// 
			// listView
			// 
			listView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			listView.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
			listView.FullRowSelect = true;
			listView.Location = new Point(0, 42);
			listView.Name = "listView";
			listView.Size = new Size(658, 173);
			listView.TabIndex = 1;
			listView.UseCompatibleStateImageBehavior = false;
			listView.View = View.Details;
			listView.KeyUp += ListView_KeyUp;
			listView.MouseUp += ListView_MouseUp;
			// 
			// addFolderButton
			// 
			addFolderButton.Location = new Point(558, 5);
			addFolderButton.Name = "addFolderButton";
			addFolderButton.Size = new Size(100, 29);
			addFolderButton.TabIndex = 22;
			addFolderButton.Text = "Add Folder";
			addFolderButton.UseVisualStyleBackColor = true;
			addFolderButton.Click += AddFolderButton_Click;
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new Point(0, 9);
			label4.Name = "label4";
			label4.Size = new Size(303, 20);
			label4.TabIndex = 21;
			label4.Text = "Custom file types (use delete key to remove)";
			// 
			// addFilesButton
			// 
			addFilesButton.Location = new Point(450, 5);
			addFilesButton.Margin = new Padding(5);
			addFilesButton.Name = "addFilesButton";
			addFilesButton.Size = new Size(100, 29);
			addFilesButton.TabIndex = 20;
			addFilesButton.Text = "Add Files";
			addFilesButton.UseVisualStyleBackColor = true;
			addFilesButton.Click += AddFilesButton_Click;
			// 
			// TypeListView
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(addFolderButton);
			Controls.Add(label4);
			Controls.Add(addFilesButton);
			Controls.Add(combo);
			Controls.Add(listView);
			Name = "TypeListView";
			Size = new Size(658, 215);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private ComboBox combo;
		private ListView listView;
		private Button addFolderButton;
		private Label label4;
		private Button addFilesButton;
	}
}
