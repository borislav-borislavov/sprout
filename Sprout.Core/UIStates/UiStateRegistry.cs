using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprout.Core.UIStates
{
    public sealed class UiStateRegistry
    {
        public BaseUIState this[string controlName]
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

        private readonly Dictionary<string, BaseUIState> _states = new();

        public void Register(string controlName, BaseUIState state)
        {
            _states[controlName] = state;
            state.PropertyChanged += OnStateChanged;
        }

        private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
        {
            UiStateChanged?.Invoke(this, new UiStateChangedEventArgs(sender!, e.PropertyName!));
        }

        public event EventHandler<UiStateChangedEventArgs>? UiStateChanged;

        public T? Get<T>(string key) where T : BaseUIState
        {
            return _states.TryGetValue(key, out var v) ? (T)v : default;
        }
    }
}
