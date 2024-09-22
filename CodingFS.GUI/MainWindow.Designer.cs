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
			driveSelect = new ComboBox();
			optionsGroup = new GroupBox();
			depthInput = new NumericUpDown();
			sourceCheck = new CheckBox();
			generatedCheck = new CheckBox();
			dependencyCheck = new CheckBox();
			label1 = new Label();
			label2 = new Label();
			optionsGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)depthInput).BeginInit();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(8, 99);
			label1.Name = "label1";
			label1.Size = new Size(70, 20);
			label1.TabIndex = 9;
			label1.Text = "Mount to";
			// 
			// selectRootButton
			// 
			selectRootButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			selectRootButton.Location = new Point(488, 28);
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
			rootBox.Location = new Point(8, 28);
			rootBox.Name = "rootBox";
			rootBox.Size = new Size(474, 27);
			rootBox.TabIndex = 1;
			// 
			// readonlyCheck
			// 
			readonlyCheck.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			readonlyCheck.AutoSize = true;
			readonlyCheck.Location = new Point(389, 98);
			readonlyCheck.Name = "readonlyCheck";
			readonlyCheck.Size = new Size(93, 24);
			readonlyCheck.TabIndex = 2;
			readonlyCheck.Text = "Readonly";
			readonlyCheck.UseVisualStyleBackColor = true;
			// 
			// mountButton
			// 
			mountButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			mountButton.Location = new Point(508, 160);
			mountButton.Margin = new Padding(5);
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
			unmountButton.Location = new Point(404, 160);
			unmountButton.Margin = new Padding(5);
			unmountButton.Name = "unmountButton";
			unmountButton.Size = new Size(94, 29);
			unmountButton.TabIndex = 4;
			unmountButton.Text = "Unmount";
			unmountButton.UseVisualStyleBackColor = true;
			unmountButton.Click += UnmountButton_Click;
			// 
			// driveSelect
			// 
			driveSelect.DropDownStyle = ComboBoxStyle.DropDownList;
			driveSelect.FormattingEnabled = true;
			driveSelect.Location = new Point(84, 96);
			driveSelect.Name = "driveSelect";
			driveSelect.Size = new Size(82, 28);
			driveSelect.TabIndex = 8;
			// 
			// optionsGroup
			// 
			optionsGroup.Controls.Add(dependencyCheck);
			optionsGroup.Controls.Add(generatedCheck);
			optionsGroup.Controls.Add(sourceCheck);
			optionsGroup.Controls.Add(depthInput);
			optionsGroup.Controls.Add(rootBox);
			optionsGroup.Controls.Add(label2);
			optionsGroup.Controls.Add(selectRootButton);
			optionsGroup.Controls.Add(readonlyCheck);
			optionsGroup.Controls.Add(label1);
			optionsGroup.Controls.Add(driveSelect);
			optionsGroup.Location = new Point(12, 12);
			optionsGroup.Name = "optionsGroup";
			optionsGroup.Padding = new Padding(5);
			optionsGroup.Size = new Size(590, 138);
			optionsGroup.TabIndex = 11;
			optionsGroup.TabStop = false;
			optionsGroup.Text = "Options";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(172, 99);
			label2.Name = "label2";
			label2.Size = new Size(83, 20);
			label2.TabIndex = 10;
			label2.Text = "Scan depth";
			// 
			// depthInput
			// 
			depthInput.Location = new Point(261, 95);
			depthInput.Name = "depthInput";
			depthInput.Size = new Size(107, 27);
			depthInput.TabIndex = 11;
			// 
			// sourceCheck
			// 
			sourceCheck.AutoSize = true;
			sourceCheck.Location = new Point(8, 63);
			sourceCheck.Margin = new Padding(5);
			sourceCheck.Name = "sourceCheck";
			sourceCheck.Size = new Size(109, 24);
			sourceCheck.TabIndex = 12;
			sourceCheck.Text = "Source Files";
			sourceCheck.UseVisualStyleBackColor = true;
			// 
			// generatedCheck
			// 
			generatedCheck.AutoSize = true;
			generatedCheck.Location = new Point(255, 63);
			generatedCheck.Margin = new Padding(5);
			generatedCheck.Name = "generatedCheck";
			generatedCheck.Size = new Size(133, 24);
			generatedCheck.TabIndex = 13;
			generatedCheck.Text = "Generated Files";
			generatedCheck.UseVisualStyleBackColor = true;
			// 
			// dependencyCheck
			// 
			dependencyCheck.AutoSize = true;
			dependencyCheck.Location = new Point(123, 63);
			dependencyCheck.Margin = new Padding(5);
			dependencyCheck.Name = "dependencyCheck";
			dependencyCheck.Size = new Size(126, 24);
			dependencyCheck.TabIndex = 14;
			dependencyCheck.Text = "Depdndencies";
			dependencyCheck.UseVisualStyleBackColor = true;
			// 
			// MainWindow
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(614, 199);
			Controls.Add(optionsGroup);
			Controls.Add(unmountButton);
			Controls.Add(mountButton);
			Name = "MainWindow";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "CodingFS";
			FormClosing += MainWindow_FormClosing;
			optionsGroup.ResumeLayout(false);
			optionsGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)depthInput).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Button selectRootButton;
		private TextBox rootBox;
		private CheckBox readonlyCheck;
		private Button mountButton;
		private Button unmountButton;
		private ComboBox driveSelect;
		private Label label1;
		private GroupBox optionsGroup;
		private NumericUpDown depthInput;
		private CheckBox dependencyCheck;
		private CheckBox generatedCheck;
		private CheckBox sourceCheck;
	}
}
