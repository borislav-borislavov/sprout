namespace Sprout.Core.Features.AppStateFeature
{
    public interface IAppState
    {
        event EventHandler<AppStateChangedEventArgs> AppStateChanged;
        void Clear();
        bool Contains(string key);
        T? Get<T>(string key, T? defaultValue = default);
        bool Remove(string key);
        void Set<T>(string key, T value);
        bool TryGet<T>(string key, out T? value);
    }
}