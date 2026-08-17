namespace Sprout.Core.Services.ValueStore
{
    /// <summary>
    /// A simple key/value store persisted as JSON.
    /// </summary>
    public interface IValueStore
    {
        /// <summary>
        /// Saves a value under the given key and persists it to disk.
        /// </summary>
        void Save(string key, object value);

        /// <summary>
        /// Gets a value by key. Returns <paramref name="defaultValue"/> when the key does not exist.
        /// </summary>
        T Get<T>(string key, T defaultValue = default);

        /// <summary>
        /// Returns true when the store contains the given key.
        /// </summary>
        bool Contains(string key);

        /// <summary>
        /// Removes a key from the store. Returns true if the key existed.
        /// </summary>
        bool Remove(string key);
    }
}
