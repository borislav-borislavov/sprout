namespace Sprout.Core.Features.AppStateFeature;

using System.Collections.Concurrent;

/// <summary>
/// Holds application-wide runtime values in memory for the lifetime of the process.
/// Data is not persisted and is lost on application exit.
/// </summary>
public class AppState : IAppState
{
    private readonly ConcurrentDictionary<string, object?> _values = new();

    public event EventHandler<AppStateChangedEventArgs> AppStateChanged;

    public void Set<T>(string key, T value)
    {
        _values[key] = value;
        AppStateChanged?.Invoke(this, new AppStateChangedEventArgs(key, value));
    }

    public T? Get<T>(string key, T? defaultValue = default)
    {
        return _values.TryGetValue(key, out var value) ? (T?)value : defaultValue;
    }

    public bool TryGet<T>(string key, out T? value)
    {
        if (_values.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public bool Remove(string key)
    {
        return _values.TryRemove(key, out _);
    }

    public bool Contains(string key)
    {
        return _values.ContainsKey(key);
    }

    public void Clear()
    {
        _values.Clear();
    }
}
