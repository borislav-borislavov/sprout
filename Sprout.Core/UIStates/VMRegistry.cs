using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.UIStates
{
    public sealed class VMRegistry
    {
        public BaseSproutControlVM this[string controlName]
        {
            get
            {
                if (_states.TryGetValue(controlName, out var state))
                {
                    return state;
                }

                throw new KeyNotFoundException($"No UI state registered with the name '{controlName}'.");
            }
        }

        private readonly Dictionary<string, BaseSproutControlVM> _states = new(StringComparer.InvariantCultureIgnoreCase);

        public Dictionary<string, BaseSproutControlVM> States => _states;

        public void Register(BaseSproutControlVM state)
        {
            _states[state.Name] = state;
            state.PropertyChanged += OnStateChanged;
        }

        private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
        {
            UiStateChanged?.Invoke(this, new UiStateChangedEventArgs(sender!, e.PropertyName!));
        }

        public event EventHandler<UiStateChangedEventArgs>? UiStateChanged;

        public T? Get<T>(string key) where T : BaseSproutControlVM
        {
            if (_states.TryGetValue(key, out var v) && v.GetType() == typeof(T))
            {
                return (T)v;
            }

            return default;

        }

        public object? Get(string key)
        {
            if (_states.TryGetValue(key, out var v))
                return v;

            return null;
        }
    }
}
