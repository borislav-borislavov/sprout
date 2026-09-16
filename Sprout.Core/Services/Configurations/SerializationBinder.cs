using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sprout.Core.Features.SeedFileUpdateFeature;
using Sprout.Core.Models.Configurations;
using Sprout.Core.Models.Configurations.Api;
using Sprout.Core.Models.Configurations.DataGrid;
using Sprout.Core.Models.Configurations.Duck;

namespace Sprout.Core.Services.Configurations;

/// <summary>
/// Responsible for providing keys for classes in the Polymorphic serialization.
/// By default the fully qualified path of each type is used which makes it hard to refactor the application.
/// This SerializationBinder provides static keys for each type even if the class name gets refactored and it is backwards compatible.
/// </summary>
public class SerializationBinder : ISerializationBinder
{
    private static bool SaveLegacyPaths = false;

    private static readonly Dictionary<string, Type> NameToType = new()
    {
        ["adHocSeedUpdate"] = typeof(AdHocSeedUpdateConfig),
        ["apiDataAdapter"] = typeof(ApiDataAdapterConfig),
        ["apiDataProvider"] = typeof(ApiDataProviderConfig),
        ["apiEditCommand"] = typeof(ApiEditCommandConfig),
        ["closePageAction"] = typeof(ClosePageActionConfig),
        ["copyToClipboardAction"] = typeof(CopyToClipboardActionConfig),
        ["duckDataAdapter"] = typeof(DuckDataAdapterConfig),
        ["duckDataProvider"] = typeof(DuckDataProviderConfig),
        ["duckEditCommand"] = typeof(DuckEditCommandConfig),
        ["executeSelectAction"] = typeof(ExecuteSelectActionConfig),
        ["executeUpdateAction"] = typeof(ExecuteUpdateActionConfig),
        ["filter"] = typeof(FilterConfig),
        ["grid"] = typeof(GridConfig),
        ["login"] = typeof(LoginConfiguration),
        ["openPageAction"] = typeof(OpenPageActionConfig),
        ["refreshDataGridAction"] = typeof(RefreshDataGridActionConfig),
        ["sqlServerDataAdapter"] = typeof(SqlServerDataAdapterConfig),
        ["sqlServerDataProvider"] = typeof(SqlServerDataProviderConfig),
        ["sqlServerEditCommand"] = typeof(SqlServerEditCommandConfig),
        ["border"] = typeof(SproutBorderConfig),
        ["sproutButtonAction"] = typeof(SproutButtonActionConfig),
        ["button"] = typeof(SproutButtonConfig),
        ["checkBox"] = typeof(SproutCheckBoxConfig),
        ["combo"] = typeof(SproutComboConfig),
        ["config"] = typeof(SproutConfiguration),
        ["control"] = typeof(SproutControlConfig),
        ["datePicker"] = typeof(SproutDatePickerConfig),
        ["dataGridColumn"] = typeof(SproutDataGridColumnConfig),
        ["dataGrid"] = typeof(SproutDataGridConfig),
        ["dataGridRowAction"] = typeof(SproutDataGridRowActionConfig),
        ["job"] = typeof(SproutJobConfiguration),
        ["label"] = typeof(SproutLabelConfig),
        ["list"] = typeof(SproutListConfig),
        ["page"] = typeof(SproutPageConfiguration),
        ["tabControl"] = typeof(SproutTabControlConfig),
        ["tabItem"] = typeof(SproutTabItemConfig),
        ["textBox"] = typeof(SproutTextBoxConfig),
    };

    public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
    {
        if (SaveLegacyPaths == false)
        {
            var match = NameToType.FirstOrDefault(kv => kv.Value == serializedType);
            if (match.Key != null)
            {
                assemblyName = null;
                typeName = match.Key;
                return;
            } 
        }

        // not in the dictionary — fall back to legacy fully-qualified format
        assemblyName = serializedType.Assembly.GetName().Name;
        typeName = serializedType.FullName;
    }

    public Type BindToType(string? assemblyName, string typeName)
    {
        if (NameToType.TryGetValue(typeName, out var type))
            return type; // new short-name format

        // fall back: old fully-qualified name still floating around
        var legacyType = Type.GetType($"{typeName}, {assemblyName}");
        if (legacyType != null)
            return legacyType;

        throw new JsonSerializationException($"Unknown child type: '{typeName}'");
    }
}
