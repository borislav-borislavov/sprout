using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Sprout.Core.Features.Dependency
{
    public static class BindingEvaluator
    {
        /// <summary>
        /// Used to extract a value in a binding like way from an object.
        /// It is used maintly to evaluate bindings when executing insert/update/delete commands.
        /// In such scenarios we don't want the bindings to fire every time a value changes, we just want to evaluate the binding once before the operation is executed.
        /// </summary>
        [Obsolete("This method is deprecated. Use FastPropertyPathEvaluator.GetValue instead.")]
        public static object Evaluate(object source, string path)
        {
            if (source == null) return null;

            // 1. Create the dummy
            var dummy = new FrameworkElement { DataContext = source };
            var binding = new Binding(path) { Source = source };

            try
            {
                // 2. Attach the binding
                BindingOperations.SetBinding(dummy, FrameworkElement.TagProperty, binding);

                // 3. Capture the value
                return dummy.Tag;
            }
            finally
            {
                // 4. CLEAN UP: Explicitly break the link between the source and the dummy
                BindingOperations.ClearBinding(dummy, FrameworkElement.TagProperty);
                dummy.DataContext = null;
            }
        }
    }
}
