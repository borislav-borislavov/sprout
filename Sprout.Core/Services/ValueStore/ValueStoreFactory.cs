using System.Collections.Concurrent;
using System.IO;

namespace Sprout.Core.Services.ValueStore
{
    public class ValueStoreFactory : IValueStoreFactory
    {
        private readonly string _rootPath;
        private readonly ConcurrentDictionary<string, IValueStore> _stores = new(StringComparer.OrdinalIgnoreCase);

        public ValueStoreFactory(string rootPath = null)
        {
            _rootPath = string.IsNullOrEmpty(rootPath)
                ? Path.Combine(Environment.CurrentDirectory, "ValueStore")
                : rootPath;
        }

        public IValueStore Get(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be null or empty.", nameof(category));

            var safeName = SanitizeFileName(category);

            return _stores.GetOrAdd(safeName, name => new JsonValueStore(Path.Combine(_rootPath, $"{name}.json")));
        }

        private static string SanitizeFileName(string category)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                category = category.Replace(invalidChar, '_');

            return category;
        }
    }
}
