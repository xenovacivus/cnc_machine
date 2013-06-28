namespace GUI
{
    partial class RouterUI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RouterUI));
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addRout = new System.Windows.Forms.ToolStripMenuItem();
            this.saveButton = new System.Windows.Forms.Button();
            this.openButton = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tool = new System.Windows.Forms.Button();
            this.routMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.completeRout = new System.Windows.Forms.ToolStripMenuItem();
            this.insertMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.insertPoint = new System.Windows.Forms.ToolStripMenuItem();
            this.movePoint = new System.Windows.Forms.ToolStripMenuItem();
            this.deletePoint = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteLine = new System.Windows.Forms.ToolStripMenuItem();
            this.routNow = new System.Windows.Forms.ToolStripMenuItem();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.robot_button = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.routBackwards = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.toolSpeedUpDown = new System.Windows.Forms.NumericUpDown();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.up_down_z = new System.Windows.Forms.NumericUpDown();
            this.up_down_y = new System.Windows.Forms.NumericUpDown();
            this.up_down_x = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.userControl11 = new GUI.RouterDrawing();
            this.contextMenuStrip1.SuspendLayout();
            this.routMenu.SuspendLayout();
            this.insertMenu.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.toolSpeedUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.up_down_z)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.up_down_y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.up_down_x)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addRout});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(125, 26);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // addRout
            // 
            this.addRout.Name = "addRout";
            this.addRout.Size = new System.Drawing.Size(124, 22);
            this.addRout.Text = "Add Rout";
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(6, 48);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(54, 23);
            this.saveButton.TabIndex = 39;
            this.saveButton.Text = "save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(66, 48);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(54, 23);
            this.openButton.TabIndex = 40;
            this.openButton.Text = "open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 19);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(152, 23);
            this.button2.TabIndex = 46;
            this.button2.Text = "Run";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.RoutAllClick);
            // 
            // tool
            // 
            this.tool.Location = new System.Drawing.Point(6, 77);
            this.tool.Name = "tool";
            this.tool.Size = new System.Drawing.Size(152, 23);
            this.tool.TabIndex = 53;
            this.tool.Text = "Tool Down";
            this.tool.UseVisualStyleBackColor = true;
            this.tool.Click += new System.EventHandler(this.tool_Click);
            // 
            // routMenu
            // 
            this.routMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.completeRout});
            this.routMenu.Name = "routMenu";
            this.routMenu.Size = new System.Drawing.Size(150, 26);
            // 
            // completeRout
            // 
            this.completeRout.Name = "completeRout";
            this.completeRout.Size = new System.Drawing.Size(149, 22);
            this.completeRout.Text = "completeRout";
            // 
            // insertMenu
            // 
            this.insertMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertPoint,
            this.movePoint,
            this.deletePoint,
            this.deleteLine,
            this.routNow});
            this.insertMenu.Name = "routMenu";
            this.insertMenu.Size = new System.Drawing.Size(139, 114);
            // 
            // insertPoint
            // 
            this.insertPoint.Name = "insertPoint";
            this.insertPoint.Size = new System.Drawing.Size(138, 22);
            this.insertPoint.Text = "Insert Point";
            // 
            // movePoint
            // 
            this.movePoint.Name = "movePoint";
            this.movePoint.Size = new System.Drawing.Size(138, 22);
            this.movePoint.Text = "Move Point";
            // 
            // deletePoint
            // 
            this.deletePoint.Name = "deletePoint";
            this.deletePoint.Size = new System.Drawing.Size(138, 22);
            this.deletePoint.Text = "Delete Point";
            // 
            // deleteLine
            // 
            this.deleteLine.Name = "deleteLine";
            this.deleteLine.Size = new System.Drawing.Size(138, 22);
            this.deleteLine.Text = "Delete Line";
            // 
            // routNow
            // 
            this.routNow.Name = "routNow";
            this.routNow.Size = new System.Drawing.Size(138, 22);
            this.routNow.Text = "Rout Now!";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(6, 48);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(152, 23);
            this.button3.TabIndex = 56;
            this.button3.Text = "Complete";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.CompleteClick);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(6, 19);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(171, 23);
            this.button4.TabIndex = 57;
            this.button4.Text = "Com Port";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // robot_button
            // 
            this.robot_button.Location = new System.Drawing.Point(84, 75);
            this.robot_button.Name = "robot_button";
            this.robot_button.Size = new System.Drawing.Size(93, 23);
            this.robot_button.TabIndex = 61;
            this.robot_button.Text = "robot";
            this.robot_button.UseVisualStyleBackColor = true;
            this.robot_button.Click += new System.EventHandler(this.robot_button_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 48);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(171, 23);
            this.button1.TabIndex = 65;
            this.button1.Text = "Reset Robot";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_2);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(6, 19);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(152, 23);
            this.button5.TabIndex = 66;
            this.button5.Text = "open gcode";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(6, 77);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(95, 23);
            this.button6.TabIndex = 67;
            this.button6.Text = "run gcode";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // propertyGrid
            // 
            this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid.Location = new System.Drawing.Point(993, 12);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(244, 456);
            this.propertyGrid.TabIndex = 70;
            // 
            // routBackwards
            // 
            this.routBackwards.AutoSize = true;
            this.routBackwards.Location = new System.Drawing.Point(6, 106);
            this.routBackwards.Name = "routBackwards";
            this.routBackwards.Size = new System.Drawing.Size(105, 17);
            this.routBackwards.TabIndex = 55;
            this.routBackwards.Text = "Rout Backwards";
            this.routBackwards.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.toolSpeedUpDown);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.up_down_z);
            this.groupBox1.Controls.Add(this.up_down_y);
            this.groupBox1.Controls.Add(this.up_down_x);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.robot_button);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(183, 161);
            this.groupBox1.TabIndex = 71;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Hardware Setup";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(93, 132);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 76;
            this.label2.Text = "IPM";
            // 
            // toolSpeedUpDown
            // 
            this.toolSpeedUpDown.DecimalPlaces = 1;
            this.toolSpeedUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.toolSpeedUpDown.Location = new System.Drawing.Point(125, 130);
            this.toolSpeedUpDown.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.toolSpeedUpDown.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.toolSpeedUpDown.Name = "toolSpeedUpDown";
            this.toolSpeedUpDown.Size = new System.Drawing.Size(52, 20);
            this.toolSpeedUpDown.TabIndex = 75;
            this.toolSpeedUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(130, 104);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(47, 17);
            this.checkBox1.TabIndex = 74;
            this.checkBox1.Text = "auto";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // up_down_z
            // 
            this.up_down_z.DecimalPlaces = 3;
            this.up_down_z.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.up_down_z.Location = new System.Drawing.Point(26, 130);
            this.up_down_z.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.up_down_z.Name = "up_down_z";
            this.up_down_z.Size = new System.Drawing.Size(52, 20);
            this.up_down_z.TabIndex = 72;
            // 
            // up_down_y
            // 
            this.up_down_y.DecimalPlaces = 3;
            this.up_down_y.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.up_down_y.Location = new System.Drawing.Point(26, 103);
            this.up_down_y.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.up_down_y.Name = "up_down_y";
            this.up_down_y.Size = new System.Drawing.Size(52, 20);
            this.up_down_y.TabIndex = 71;
            // 
            // up_down_x
            // 
            this.up_down_x.DecimalPlaces = 3;
            this.up_down_x.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.up_down_x.Location = new System.Drawing.Point(26, 77);
            this.up_down_x.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.up_down_x.Name = "up_down_x";
            this.up_down_x.Size = new System.Drawing.Size(52, 20);
            this.up_down_x.TabIndex = 70;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 132);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.TabIndex = 68;
            this.label5.Text = "Z";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 106);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 13);
            this.label4.TabIndex = 67;
            this.label4.Text = "Y";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 66;
            this.label1.Text = "X";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.routBackwards);
            this.groupBox2.Controls.Add(this.tool);
            this.groupBox2.Location = new System.Drawing.Point(12, 179);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(164, 131);
            this.groupBox2.TabIndex = 72;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Router Commands";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button5);
            this.groupBox3.Controls.Add(this.saveButton);
            this.groupBox3.Controls.Add(this.openButton);
            this.groupBox3.Controls.Add(this.button6);
            this.groupBox3.Location = new System.Drawing.Point(12, 316);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(164, 134);
            this.groupBox3.TabIndex = 73;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Inputs";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.radioButton3);
            this.groupBox4.Controls.Add(this.radioButton2);
            this.groupBox4.Controls.Add(this.radioButton1);
            this.groupBox4.Location = new System.Drawing.Point(201, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(107, 161);
            this.groupBox4.TabIndex = 74;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "groupBox4";
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(6, 71);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(49, 17);
            this.radioButton3.TabIndex = 2;
            this.radioButton3.Text = "robot";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(6, 48);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(52, 17);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.Text = "router";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 25);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(84, 17);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "drawing item";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // userControl11
            // 
            this.userControl11.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.userControl11.BackColor = System.Drawing.Color.Black;
            this.userControl11.ClearColor = System.Drawing.Color.Empty;
            this.userControl11.Location = new System.Drawing.Point(314, 12);
            this.userControl11.MinimumSize = new System.Drawing.Size(10, 10);
            this.userControl11.Name = "userControl11";
            this.userControl11.Size = new System.Drawing.Size(673, 456);
            this.userControl11.TabIndex = 68;
            this.userControl11.VSync = false;
            // 
            // RouterUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1249, 480);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.userControl11);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.propertyGrid);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.Name = "RouterUI";
            this.Text = "CNC Router";
            this.contextMenuStrip1.ResumeLayout(false);
            this.routMenu.ResumeLayout(false);
            this.insertMenu.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.toolSpeedUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.up_down_z)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.up_down_y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.up_down_x)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem addRout;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button tool;
        private System.Windows.Forms.ContextMenuStrip routMenu;
        private System.Windows.Forms.ToolStripMenuItem completeRout;
        private System.Windows.Forms.ContextMenuStrip insertMenu;
        private System.Windows.Forms.ToolStripMenuItem insertPoint;
        private System.Windows.Forms.ToolStripMenuItem movePoint;
        private System.Windows.Forms.ToolStripMenuItem deletePoint;
        private System.Windows.Forms.ToolStripMenuItem deleteLine;
        private System.Windows.Forms.ToolStripMenuItem routNow;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button robot_button;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private RouterDrawing userControl11;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.CheckBox routBackwards;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.NumericUpDown up_down_x;
        private System.Windows.Forms.NumericUpDown up_down_z;
        private System.Windows.Forms.NumericUpDown up_down_y;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown toolSpeedUpDown;

    }
}

