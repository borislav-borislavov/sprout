using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Sprout.Core.Services
{
    /// <summary>
    /// Converts a List of Dictionaries to a format bindable by WPF DataGrid.
    /// </summary>
    public class DictionaryDataSource
    {
        /// <summary>
        /// Wraps a list of dictionaries into a DataView-compatible format.
        /// </summary>
        public static ListCollectionView CreateBindableSource(List<Dictionary<string, object>> data)
        {
            // Convert dictionaries to dynamic objects for binding
            var dynamicList = data
                .Select(dict => new DynamicDictionary(dict))
                .ToList();

            return new ListCollectionView(dynamicList);
        }
    }

    /// <summary>
    /// A wrapper that allows Dictionary<string, object> to work with WPF data binding.
    /// </summary>
    public class DynamicDictionary : System.Dynamic.DynamicObject
    {
        private readonly Dictionary<string, object> _dictionary;

        public DynamicDictionary(Dictionary<string, object> dictionary)
        {
            _dictionary = dictionary ?? new Dictionary<string, object>();
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            result = null;
            if (_dictionary.ContainsKey(binder.Name))
            {
                result = _dictionary[binder.Name];
                return true;
            }
            return false;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _dictionary.Keys;
        }
    }
}
