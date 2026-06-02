using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.Models.Configurations
{
	public class SproutComboConfig : SproutControlConfig, IDataAdapterConfigHost
	{
		public string DisplayColumn { get; set; }
		public string ValueColumn { get; set; }
		public string SelectedValue { get; set; }

		public double? Height { get; set; }
		public double? Width { get; set; }
		public string Margin { get; set; }
		public string HorizontalAlignment { get; set; }
		public string VerticalAlignment { get; set; }
		public string ToolTip { get; set; }

		public IDataAdapterConfig DataAdapter { get; set; }
	}
}
