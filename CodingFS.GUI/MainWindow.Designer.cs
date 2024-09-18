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
			listButton = new Button();
			commandBox = new TextBox();
			driveSelect = new ComboBox();
			label1 = new Label();
			label2 = new Label();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(12, 50);
			label1.Name = "label1";
			label1.Size = new Size(70, 20);
			label1.TabIndex = 9;
			label1.Text = "Mount to";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(215, 50);
			label2.Name = "label2";
			label2.Size = new Size(73, 20);
			label2.TabIndex = 10;
			label2.Text = "With type";
			// 
			// selectRootButton
			// 
			selectRootButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			selectRootButton.Location = new Point(456, 10);
			selectRootButton.Name = "selectRootButton";
			selectRootButton.Size = new Size(94, 29);
			selectRootButton.TabIndex = 0;
			selectRootButton.Text = "Select";
			selectRootButton.UseVisualStyleBackColor = true;
			selectRootButton.Click += selectRootButton_Click;
			// 
			// rootBox
			// 
			rootBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			rootBox.Location = new Point(12, 12);
			rootBox.Name = "rootBox";
			rootBox.Size = new Size(438, 27);
			rootBox.TabIndex = 1;
			// 
			// readonlyCheck
			// 
			readonlyCheck.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			readonlyCheck.AutoSize = true;
			readonlyCheck.Location = new Point(457, 49);
			readonlyCheck.Name = "readonlyCheck";
			readonlyCheck.Size = new Size(93, 24);
			readonlyCheck.TabIndex = 2;
			readonlyCheck.Text = "Readonly";
			readonlyCheck.UseVisualStyleBackColor = true;
			// 
			// mountButton
			// 
			mountButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			mountButton.Location = new Point(456, 119);
			mountButton.Name = "mountButton";
			mountButton.Size = new Size(94, 29);
			mountButton.TabIndex = 3;
			mountButton.Text = "Mount";
			mountButton.UseVisualStyleBackColor = true;
			mountButton.Click += mountButton_Click;
			// 
			// unmountButton
			// 
			unmountButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			unmountButton.Enabled = false;
			unmountButton.Location = new Point(356, 119);
			unmountButton.Name = "unmountButton";
			unmountButton.Size = new Size(94, 29);
			unmountButton.TabIndex = 4;
			unmountButton.Text = "Unmount";
			unmountButton.UseVisualStyleBackColor = true;
			unmountButton.Click += unmountButton_Click;
			// 
			// typeSelect
			// 
			typeSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			typeSelect.FormattingEnabled = true;
			typeSelect.Location = new Point(294, 45);
			typeSelect.Name = "typeSelect";
			typeSelect.Size = new Size(156, 28);
			typeSelect.TabIndex = 5;
			// 
			// listButton
			// 
			listButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			listButton.Location = new Point(256, 119);
			listButton.Name = "listButton";
			listButton.Size = new Size(94, 29);
			listButton.TabIndex = 6;
			listButton.Text = "List Files";
			listButton.UseVisualStyleBackColor = true;
			listButton.Click += listButton_Click;
			// 
			// commandBox
			// 
			commandBox.Location = new Point(12, 81);
			commandBox.Margin = new Padding(5);
			commandBox.Name = "commandBox";
			commandBox.ReadOnly = true;
			commandBox.Size = new Size(538, 27);
			commandBox.TabIndex = 7;
			// 
			// driveSelect
			// 
			driveSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			driveSelect.FormattingEnabled = true;
			driveSelect.Location = new Point(88, 45);
			driveSelect.Name = "driveSelect";
			driveSelect.Size = new Size(82, 28);
			driveSelect.TabIndex = 8;
			// 
			// MainWindow
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(562, 160);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(driveSelect);
			Controls.Add(commandBox);
			Controls.Add(listButton);
			Controls.Add(typeSelect);
			Controls.Add(unmountButton);
			Controls.Add(mountButton);
			Controls.Add(readonlyCheck);
			Controls.Add(rootBox);
			Controls.Add(selectRootButton);
			Name = "MainWindow";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "CodingFS";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button selectRootButton;
		private TextBox rootBox;
		private CheckBox readonlyCheck;
		private Button mountButton;
		private Button unmountButton;
		private ComboBox typeSelect;
		private Button listButton;
		private TextBox commandBox;
		private ComboBox driveSelect;
		private Label label1;
	}
}
