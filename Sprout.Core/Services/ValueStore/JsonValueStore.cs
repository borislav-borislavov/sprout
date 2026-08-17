using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

namespace Sprout.Core.Services.ValueStore
{
    /// <summary>
    /// A key/value store persisted as a JSON file. One file per category.
    /// </summary>
    public class JsonValueStore : IValueStore
    {
        private readonly object _lock = new();
        private readonly string _filePath;
        private JObject _values;

        public JsonValueStore(string filePath)
        {
            _filePath = filePath;
        }

        public void Save(string key, object value)
        {
            lock (_lock)
            {
                var values = LoadValues();
                values[key] = value == null ? JValue.CreateNull() : JToken.FromObject(value);
                Persist(values);
            }
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            lock (_lock)
            {
                var values = LoadValues();

                if (!values.TryGetValue(key, out var token) || token.Type == JTokenType.Null)
                    return defaultValue;

                try
                {
                    return token.ToObject<T>();
                }
                catch
                {
                    //TODO: logging
                    return defaultValue;
                }
            }
        }

        public bool Contains(string key)
        {
            lock (_lock)
            {
                return LoadValues().ContainsKey(key);
            }
        }

        public bool Remove(string key)
        {
            lock (_lock)
            {
                var values = LoadValues();

                if (!values.Remove(key)) return false;

                Persist(values);
                return true;
            }
        }

        private JObject LoadValues()
        {
            if (_values != null) return _values;

            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath, Encoding.UTF8);
                    _values = string.IsNullOrWhiteSpace(json) ? new JObject() : JObject.Parse(json);
                }
                else
                {
                    _values = new JObject();
                }
            }
            catch
            {
                //TODO: logging
                _values = new JObject();
            }

            return _values;
        }

        private void Persist(JObject values)
        {
            _values = values;
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            File.WriteAllText(_filePath, values.ToString(Formatting.Indented), Encoding.UTF8);
        }
    }
}
