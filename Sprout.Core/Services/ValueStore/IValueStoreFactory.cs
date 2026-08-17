namespace Sprout.Core.Services.ValueStore
{
    /// <summary>
    /// Provides <see cref="IValueStore"/> instances per category.
    /// Each category is saved to its own JSON file.
    /// </summary>
    public interface IValueStoreFactory
    {
        IValueStore Get(string category);
    }
}
