namespace CodingFS.GUI
{
	partial class MainWindow
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
			Label label1;
			Label label2;
			selectRootButton = new Button();
			rootBox = new TextBox();
			readonlyCheck = new CheckBox();
			mountButton = new Button();
			unmountButton = new Button();
			typeSelect = new ComboBox();
			driveSelect = new ComboBox();
			optionsGroup = new GroupBox();
			label1 = new Label();
			label2 = new Label();
			optionsGroup.SuspendLayout();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(6, 62);
			label1.Name = "label1";
			label1.Size = new Size(70, 20);
			label1.TabIndex = 9;
			label1.Text = "Mount to";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(187, 62);
			label2.Name = "label2";
			label2.Size = new Size(73, 20);
			label2.TabIndex = 10;
			label2.Text = "With type";
			// 
			// selectRootButton
			// 
			selectRootButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			selectRootButton.Location = new Point(490, 24);
			selectRootButton.Name = "selectRootButton";
			selectRootButton.Size = new Size(94, 29);
			selectRootButton.TabIndex = 0;
			selectRootButton.Text = "Select";
			selectRootButton.UseVisualStyleBackColor = true;
			selectRootButton.Click += SelectRootButton_Click;
			// 
			// rootBox
			// 
			rootBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			rootBox.Location = new Point(6, 26);
			rootBox.Name = "rootBox";
			rootBox.Size = new Size(478, 27);
			rootBox.TabIndex = 1;
			// 
			// readonlyCheck
			// 
			readonlyCheck.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			readonlyCheck.AutoSize = true;
			readonlyCheck.Location = new Point(491, 62);
			readonlyCheck.Name = "readonlyCheck";
			readonlyCheck.Size = new Size(93, 24);
			readonlyCheck.TabIndex = 2;
			readonlyCheck.Text = "Readonly";
			readonlyCheck.UseVisualStyleBackColor = true;
			// 
			// mountButton
			// 
			mountButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			mountButton.Location = new Point(508, 149);
			mountButton.Name = "mountButton";
			mountButton.Size = new Size(94, 29);
			mountButton.TabIndex = 3;
			mountButton.Text = "Mount";
			mountButton.UseVisualStyleBackColor = true;
			mountButton.Click += MountButton_Click;
			// 
			// unmountButton
			// 
			unmountButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			unmountButton.Enabled = false;
			unmountButton.Location = new Point(408, 149);
			unmountButton.Name = "unmountButton";
			unmountButton.Size = new Size(94, 29);
			unmountButton.TabIndex = 4;
			unmountButton.Text = "Unmount";
			unmountButton.UseVisualStyleBackColor = true;
			unmountButton.Click += UnmountButton_Click;
			// 
			// typeSelect
			// 
			typeSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			typeSelect.FormattingEnabled = true;
			typeSelect.Location = new Point(266, 59);
			typeSelect.Name = "typeSelect";
			typeSelect.Size = new Size(156, 28);
			typeSelect.TabIndex = 5;
			// 
			// driveSelect
			// 
			driveSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			driveSelect.FormattingEnabled = true;
			driveSelect.Location = new Point(82, 59);
			driveSelect.Name = "driveSelect";
			driveSelect.Size = new Size(82, 28);
			driveSelect.TabIndex = 8;
			// 
			// optionsGroup
			// 
			optionsGroup.Controls.Add(rootBox);
			optionsGroup.Controls.Add(label2);
			optionsGroup.Controls.Add(selectRootButton);
			optionsGroup.Controls.Add(readonlyCheck);
			optionsGroup.Controls.Add(label1);
			optionsGroup.Controls.Add(typeSelect);
			optionsGroup.Controls.Add(driveSelect);
			optionsGroup.Location = new Point(12, 12);
			optionsGroup.Name = "optionsGroup";
			optionsGroup.Size = new Size(590, 125);
			optionsGroup.TabIndex = 11;
			optionsGroup.TabStop = false;
			optionsGroup.Text = "Options";
			// 
			// MainWindow
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(614, 190);
			Controls.Add(optionsGroup);
			Controls.Add(unmountButton);
			Controls.Add(mountButton);
			Name = "MainWindow";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "CodingFS";
			FormClosing += MainWindow_FormClosing;
			optionsGroup.ResumeLayout(false);
			optionsGroup.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private Button selectRootButton;
		private TextBox rootBox;
		private CheckBox readonlyCheck;
		private Button mountButton;
		private Button unmountButton;
		private ComboBox typeSelect;
		private ComboBox driveSelect;
		private Label label1;
		private GroupBox optionsGroup;
	}
}
