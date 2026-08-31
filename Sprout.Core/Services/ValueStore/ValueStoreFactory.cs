using Sprout.Core.Common;
using Sprout.Core.Services.Configurations;
using System.Collections.Concurrent;
using System.IO;

namespace Sprout.Core.Services.ValueStore
{
    public class ValueStoreFactory : IValueStoreFactory
    {
        private readonly string _rootPath;
        private readonly ConcurrentDictionary<string, IValueStore> _stores = new(StringComparer.OrdinalIgnoreCase);

        public ValueStoreFactory(string? rootPath = null)
        {
            if (!string.IsNullOrEmpty(rootPath))
            {
                _rootPath = rootPath;
                return;
            }

            if (string.IsNullOrEmpty(AppArgs.SeedPath))
            {
                _rootPath = Path.Combine(Environment.CurrentDirectory, "ValueStore");
                return;
            }

            var seedDir = Path.GetDirectoryName(AppArgs.SeedPath);
            var seedName = Path.GetFileNameWithoutExtension(AppArgs.SeedPath);
            var seedFolder = Path.Combine(seedDir, $"{seedName}.store");
            Directory.CreateDirectory(seedFolder);
            _rootPath = seedFolder;
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
