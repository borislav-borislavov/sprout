using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.SproutControlVMs
{
    public sealed class VMRegistry
    {
        private readonly Dictionary<string, BaseSproutControlVM> _viewModels = new(StringComparer.InvariantCultureIgnoreCase);

        public Dictionary<string, BaseSproutControlVM> ViewModels => _viewModels;

        public BaseSproutControlVM this[string controlName]
        {
            get
            {
                if (_viewModels.TryGetValue(controlName, out var vm))
                {
                    return vm;
                }

                throw new KeyNotFoundException($"No VM registered with the name '{controlName}'.");
            }
        }

        public void Register(BaseSproutControlVM vm)
        {
            _viewModels[vm.Name] = vm;
            vm.PropertyChanged += OnVMChanged;
        }

        private void OnVMChanged(object? sender, PropertyChangedEventArgs e)
        {
            VMChanged?.Invoke(this, new VMChangedEventArgs(sender!, e.PropertyName!));
        }

        public event EventHandler<VMChangedEventArgs>? VMChanged;

        public T? Get<T>(string key) where T : BaseSproutControlVM
        {
            if (_viewModels.TryGetValue(key, out var v) && v.GetType() == typeof(T))
            {
                return (T)v;
            }

            return default;

        }

        public object? Get(string key)
        {
            if (_viewModels.TryGetValue(key, out var v))
                return v;

            return null;
        }
    }
}
