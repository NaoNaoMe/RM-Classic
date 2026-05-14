using System.Windows.Forms;

namespace rmApplication
{
	partial class HexBoxControl
	{
		private System.ComponentModel.IContainer components = null;
		private VScrollBar vScrollBar;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.vScrollBar = new VScrollBar();

			// 
			// vScrollBar
			// 
			this.vScrollBar.Name = "vScrollBar";
			this.vScrollBar.TabIndex = 0;
			this.vScrollBar.Visible = false;

			// 
			// HexBox
			// 
			this.Controls.Add(this.vScrollBar);
			this.Name = "HexBox";
			this.TabStop = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
				components.Dispose();
			base.Dispose(disposing);
		}
	}
}