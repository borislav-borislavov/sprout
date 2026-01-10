using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sprout.Core.Models.DataAdapters.DataProviders
{
	public class DataProviderDependency : DependencyObject
	{
		public string RawDependency { get; set; }
		public string ControlName { get; set; }
		public string PropertyPath { get; internal set; }

		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(
				name: nameof(Value),
				propertyType: typeof(string),
				ownerType: typeof(DependencyProperty),
				typeMetadata: null
			);

		public object Value
		{
			get => (string)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}
	}
}
