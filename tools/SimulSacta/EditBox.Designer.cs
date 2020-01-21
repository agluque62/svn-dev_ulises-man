namespace SimulSACTA
{
   partial class EditBox
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
         System.Windows.Forms.Button button1;
         this.ValueTB = new System.Windows.Forms.TextBox();
         button1 = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // ValueTB
         // 
         this.ValueTB.Location = new System.Drawing.Point(12, 12);
         this.ValueTB.Name = "ValueTB";
         this.ValueTB.Size = new System.Drawing.Size(100, 20);
         this.ValueTB.TabIndex = 0;
         // 
         // button1
         // 
         button1.DialogResult = System.Windows.Forms.DialogResult.OK;
         button1.Location = new System.Drawing.Point(131, 10);
         button1.Name = "button1";
         button1.Size = new System.Drawing.Size(43, 23);
         button1.TabIndex = 1;
         button1.Text = "OK";
         button1.UseVisualStyleBackColor = true;
         // 
         // EditBox
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(192, 44);
         this.Controls.Add(button1);
         this.Controls.Add(this.ValueTB);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "EditBox";
         this.Text = "Value";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      public System.Windows.Forms.TextBox ValueTB;

   }
}