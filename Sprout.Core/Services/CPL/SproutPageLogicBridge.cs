using Sprout.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sprout.Core.Services.CPL
{
    /// <summary>
    /// I Use this to communicate with the custom user logic
    /// </summary>
    public sealed class SproutPageLogicBridge : IDisposable
    {
        private readonly string _pageId;
        private PageLogicLoadContext? _loadContext;
        private CustomPageLogicBase? _instance;
        private readonly Dictionary<string, Action> _exposedMethods = new();

        public bool IsActive => _instance is not null;

        public SproutPageLogicBridge(string pageId) => _pageId = pageId;

        // ── Load / Hot-Reload ─────────────────────────────────────────

        public async Task<string?> LoadAsync(byte[] assemblyBytes, SproutPageVM pageContext)
        {
            await UnloadAsync(); // dispose old one first (hot reload)

            try
            {
                _loadContext = new PageLogicLoadContext(_pageId);
                var assembly = _loadContext.LoadFromStream(new MemoryStream(assemblyBytes));

                var type = assembly.GetType($"DynamicPageLogic._{_pageId}.CustomPageLogic")!;
                _instance = (CustomPageLogicBase)Activator.CreateInstance(type)!;
                _instance.Page = pageContext;

                await _instance.OnLoadAsync();
                return null; // null = success
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task UnloadAsync()
        {
            if (_instance is not null)
            {
                await _instance.OnUnloadAsync();
                _instance = null;
            }
            _loadContext?.Unload();
            _loadContext = null;
        }

        // ── Event forwarding ──────────────────────────────────────────

        public Task RaiseValueChangedAsync(string componentId, object value)
            => _instance?.OnComponentValueChangedAsync(componentId, value) ?? Task.CompletedTask;

        public Task<bool> RaiseBeforeSaveAsync()
            => _instance?.OnBeforeSaveAsync() ?? Task.FromResult(true);

        public void Dispose() => UnloadAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Expose a method that can be called by the framework (e.g. from a button click)
        /// </summary>
        internal void Expose(string methodName, Action method)
        {
            _exposedMethods[methodName] = method;
        }

        internal void CallExposed(string methodName)
        {
            if (_exposedMethods.TryGetValue(methodName, out var method))
                method();
            else
                throw new Exception($"No method exposed with name {methodName}");
        }
    }
}
