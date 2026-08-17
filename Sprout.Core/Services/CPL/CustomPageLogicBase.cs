using Sprout.Core.Services.ValueStore;
using Sprout.Core.SproutControlVMs;
using Sprout.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Core.Services.CPL
{
    public abstract class CustomPageLogicBase
    {
        // Injected by the platform before OnLoad
        public SproutPageVM Page { get; internal set; }

        public IValueStoreFactory ValueStoreFactory { get; set; }

        // ── Lifecycle (user overrides these) ──────────────────────────
        public virtual Task OnLoadAsync() => Task.CompletedTask;
        public virtual Task OnUnloadAsync() => Task.CompletedTask;

        // ── Events ────────────────────────────────────────────────────
        public virtual Task OnComponentValueChangedAsync(string componentId, object newValue)
            => Task.CompletedTask;

        public virtual Task<bool> OnBeforeSaveAsync()
            => Task.FromResult(true);

        public virtual Task OnPropertyChanged(VMChangedEventArgs change) => Task.CompletedTask;

        //// ── Convenience helpers the user can call ─────────────────────
        //protected void SetValue(string id, object value) => Page.SetValue(id, value);
        //protected object GetValue(string id) => Page.GetValue(id);
        //protected void ShowMessage(string msg) => Page.ShowMessage(msg);

        protected void SaveValue(string key, object value)
        {
            var store = ValueStoreFactory.Get(Page.PageConfig.ID.ToString());
            store.Save(key, value);
        }

        protected T GetValue<T>(string key, T defaultValue = default)
        {
            var store = ValueStoreFactory.Get(Page.PageConfig.ID.ToString());
            return store.Get(key, defaultValue);
        }
    }
}
