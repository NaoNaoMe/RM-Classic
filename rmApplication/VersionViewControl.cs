using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace rmApplication
{
	[Docking(DockingBehavior.Ask)]
	public partial class VersionViewControl : UserControl
	{
		public VersionViewControl()
		{
			InitializeComponent();
		}

		[Browsable(true)]
		[Description("label")]
		public string Label
		{
			get
			{
				return label.Text;
			}
			set
			{
				label.Text = value;
			}
		}

		[Browsable(true)]
		[Description("textbox")]
		public string TextBox
		{
			get
			{
				return textBox.Text;
			}
			set
			{
				textBox.Text = value;
			}
		}


	}
}
