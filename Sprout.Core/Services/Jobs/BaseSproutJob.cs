using Sprout.Core.Models.Configurations;
using Sprout.Core.Services.ValueStore;

namespace Sprout.Core.Services.Jobs
{
    public abstract class BaseSproutJob
    {
        public abstract Task<string> ExecuteAsync(CancellationToken cancellationToken);

        #region ValueStore
        private IValueStore _valueStore;
        public SproutJobConfiguration JobConfig { get; internal set; }
        public IValueStoreFactory ValueStoreFactory { get; internal set; }

        protected T GetValue<T>(string key, T defaultValue = default)
        {
            return GetDefaultValueStore().Get(key, defaultValue);
        }

        protected void SaveValue(string key, object value)
        {
            GetDefaultValueStore().Save(key, value);
        }

        protected bool ContainsValue(string key)
        {
            return GetDefaultValueStore().Contains(key);
        }

        protected bool RemoveValue(string key)
        {
            return GetDefaultValueStore().Remove(key);
        }

        protected IValueStore GetDefaultValueStore(string? key = default)
        {
            if (_valueStore == null)
            {
                _valueStore = ValueStoreFactory.Get(key ?? GetValueStoreKey());
            }

            return _valueStore;
        }

        private string GetValueStoreKey() => $"JB-{JobConfig.ID}"; 
        #endregion
    }
}
