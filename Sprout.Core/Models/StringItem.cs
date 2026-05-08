using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Sprout.Core.Models
{
    public class StringItem : INotifyPropertyChanged, IDataErrorInfo
    {
        public static implicit operator StringItem(string s) => new()
        {
            Value = s
        };

        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public string Error { get; set; }

        public string this[string columnName]
        {
            get
            {
                if (columnName != nameof(Value))
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(Value))
                    return "Value cannot be empty.";

                //if (Value.Length < 3)
                //    return "Minimum length is 3.";

                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
