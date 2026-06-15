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

        // ── Lifecycle (user overrides these) ──────────────────────────
        public virtual Task OnLoadAsync() => Task.CompletedTask;
        public virtual Task OnUnloadAsync() => Task.CompletedTask;

        // ── Events ────────────────────────────────────────────────────
        public virtual Task OnComponentValueChangedAsync(string componentId, object newValue)
            => Task.CompletedTask;

        public virtual Task<bool> OnBeforeSaveAsync()
            => Task.FromResult(true);

        //// ── Convenience helpers the user can call ─────────────────────
        //protected void SetValue(string id, object value) => Page.SetValue(id, value);
        //protected object GetValue(string id) => Page.GetValue(id);
        //protected void ShowMessage(string msg) => Page.ShowMessage(msg);
    }
}
