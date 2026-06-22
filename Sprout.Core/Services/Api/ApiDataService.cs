using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sprout.Core.Common;
using Sprout.Core.Factories;
using Sprout.Core.Models;
using Sprout.Core.Models.Configurations.Api;
using Sprout.Core.Models.DataAdapters;
using Sprout.Core.Models.DataAdapters.DataProviders;
using Sprout.Core.Models.DataAdapters.Filters;
using Sprout.Core.Models.Queries;
using Sprout.Core.Services.DataProviders;
using Sprout.Core.Services.Logging;
using Sprout.Core.UIStates;
using System.Data;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Data;
#nullable disable

namespace Sprout.Core.Services.Api
{
    public class ApiDataService : IDataService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiDataAdapter _dataAdapter;
        private readonly ApiDataProvider _dataProvider;
        private readonly ISqlQueryLogger _sqlQueryLogger;

        private static readonly string[] _tokenFieldNames = ["token", "access_token", "accessToken", "bearer", "jwt"];

        public UiStateRegistry UiStateRegistry { get; }

        public ApiDataService(
            ApiDataAdapter dataAdapter,
            UiStateRegistry uiStateRegistry,
            IHttpClientFactory httpClientFactory,
            ISqlQueryLogger sqlQueryLogger = null)
        {
            _dataAdapter = dataAdapter;
            _dataProvider = dataAdapter.DataProvider as ApiDataProvider;
            UiStateRegistry = uiStateRegistry;
            _httpClientFactory = httpClientFactory;
            _sqlQueryLogger = sqlQueryLogger;
        }

        // ──────────────────────────────────────────────────────────────
        // IDataService – read
        // ──────────────────────────────────────────────────────────────

        public async Task ProvideData()
        {
            SetBusy(true);
            try
            {
                await ProvideDataInternal();
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ProvideDataInternal()
        {
            var client = _httpClientFactory.CreateClient();

            var token = await TryAcquireTokenAsync(client);
            if (token != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = BuildUrl(_dataProvider.Text, _dataProvider);

            var sw = Stopwatch.StartNew();
            var response = await client.GetAsync(url);
            sw.Stop();

            _sqlQueryLogger?.Log(nameof(ApiDataService), url, null, sw.Elapsed);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var dt = DataTableFactory.Create();
            LoadJsonIntoDataTable(json, _dataProvider.DataPath, dt);
            DataTableFactory.PostLoadLogic(dt);

            _dataProvider.Data = dt;
        }

        // ──────────────────────────────────────────────────────────────
        // IDataService – write
        // ──────────────────────────────────────────────────────────────

        public async Task<ChangeResult> Insert(DataRow dataRow)
        {
            if (_dataAdapter.InsertCommand is not ApiEditCommand cmd)
                throw new NotImplementedException();

            if (string.IsNullOrWhiteSpace(cmd.Text))
                throw new Exception($"{nameof(_dataAdapter.InsertCommand)} URL not set");

            return await Change(cmd, dataRow);
        }

        public async Task<ChangeResult> Update(DataRow dataRow)
        {
            if (_dataAdapter.UpdateCommand is not ApiEditCommand cmd)
                throw new NotImplementedException();

            if (string.IsNullOrWhiteSpace(cmd.Text))
                throw new Exception($"{nameof(_dataAdapter.UpdateCommand)} URL not set");

            return await Change(cmd, dataRow);
        }

        public async Task<ChangeResult> Delete(DataRow dataRow)
        {
            if (_dataAdapter.DeleteCommand is not ApiEditCommand cmd)
                throw new NotImplementedException();

            if (string.IsNullOrWhiteSpace(cmd.Text))
                throw new Exception($"{nameof(_dataAdapter.DeleteCommand)} URL not set");

            return await Change(cmd, dataRow);
        }

        public async Task<ChangeResult> Change(IEditCommand editCmd, DataRow dataRow)
        {
            SetBusy(true);
            try
            {
                return await ChangeInternal(editCmd, dataRow);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task<ChangeResult> ChangeInternal(IEditCommand editCmd, DataRow dataRow)
        {
            if (editCmd is not ApiEditCommand cmd)
                throw new NotImplementedException();

            var client = _httpClientFactory.CreateClient();

            var token = await TryAcquireTokenAsync(client);
            if (token != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var url = SubstituteDependencies(cmd.Text, _dataProvider, dataRow);

            var body = BuildBody(cmd, dataRow);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var sw = Stopwatch.StartNew();

            HttpResponseMessage response = cmd.Verb switch
            {
                HttpVerb.POST => await client.PostAsync(url, content),
                HttpVerb.PUT => await client.PutAsync(url, content),
                HttpVerb.PATCH => await client.PatchAsync(url, content),
                HttpVerb.DELETE => await client.DeleteAsync(url),
                _ => throw new NotSupportedException($"HTTP verb {cmd.Verb} is not supported.")
            };

            sw.Stop();
            _sqlQueryLogger?.Log(nameof(ApiDataService), url, null, sw.Elapsed);

            var changeResult = new ChangeResult();
            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode == false)
            {
                var indentedJson = JToken.Parse(responseJson).ToString(Formatting.Indented);
                changeResult.Messages.Add(new ActionMessage("Error", indentedJson));
                return changeResult;
            }

            if (cmd.ShowResponseAsMessage)
            {
                var message = ExtractResponseMessage(responseJson, cmd.ResponsePath);
                if (message != null)
                    changeResult.Messages.Add(new ActionMessage("Info", message));
            }

            return changeResult;
        }

        // ──────────────────────────────────────────────────────────────
        // Auth
        // ──────────────────────────────────────────────────────────────

        private async Task<string> TryAcquireTokenAsync(HttpClient client)
        {
            if (string.IsNullOrWhiteSpace(_dataProvider.AuthUrl))
                return null;

            var body = _dataProvider.AuthBody ?? "{}";

            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_dataProvider.AuthUrl, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var jObj = JObject.Parse(json);

            foreach (var fieldName in _tokenFieldNames)
            {
                var prop = jObj[fieldName];
                if (prop != null)
                    return prop.Value<string>();
            }

            throw new Exception(
                $"Could not find a token field in the auth response. Expected one of: {string.Join(", ", _tokenFieldNames)}");
        }

        // ──────────────────────────────────────────────────────────────
        // URL building
        // ──────────────────────────────────────────────────────────────

        private string BuildUrl(string urlTemplate, ApiDataProvider dataProvider)
        {
            var url = SubstituteDependencies(urlTemplate, dataProvider, null);

            // Append active filters as query parameters
            var queryParams = new List<string>();

            foreach (var filter in dataProvider.Filters.Values)
            {
                if (filter is not ApiFilter apiFilter) continue;

                var value = $"{filter.StartValue}";
                if (string.IsNullOrEmpty(value)) continue;

                queryParams.Add($"{HttpUtility.UrlEncode(apiFilter.QueryParamName)}={HttpUtility.UrlEncode(value)}");

                if (filter.IsRange && filter.EndValue != null)
                {
                    var endParamName = apiFilter.QueryParamName + "_end";
                    queryParams.Add($"{HttpUtility.UrlEncode(endParamName)}={HttpUtility.UrlEncode($"{filter.EndValue}")}");
                }
            }

            if (queryParams.Count > 0)
            {
                var separator = url.Contains('?') ? "&" : "?";
                url = url + separator + string.Join("&", queryParams);
            }

            return url;
        }

        private string SubstituteDependencies(string urlTemplate, ApiDataProvider dataProvider, DataRow dataRow)
        {
            if (string.IsNullOrWhiteSpace(urlTemplate)) return urlTemplate;

            var url = urlTemplate;

            // Replace dependency placeholders ({@Control.Property})
            foreach (var dep in dataProvider.Dependencies)
            {
                var rawValue = dep.Value != null ? HttpUtility.UrlEncode($"{dep.Value}") : string.Empty;
                url = url.Replace($"{{{dep.RawDependency}}}", rawValue, StringComparison.OrdinalIgnoreCase);
            }

            // Replace row-data placeholders ({ColumnName}) when a DataRow is present
            if (dataRow != null)
            {
                var requestedParameters = DependencyParser.ParseDependencyMetas(urlTemplate);
                foreach (var param in requestedParameters)
                {
                    if (param.IsFromUIState) continue;

                    var version = dataRow.RowState == DataRowState.Deleted
                        ? DataRowVersion.Original
                        : DataRowVersion.Current;

                    try
                    {
                        var value = dataRow[param.Name, version];
                        url = url.Replace($"{{{param.RawPatameter}}}",
                            HttpUtility.UrlEncode($"{(value == DBNull.Value ? string.Empty : value)}"),
                            StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        // column might not exist in this row – skip
                    }
                }
            }

            return url;
        }

        // ──────────────────────────────────────────────────────────────
        // JSON helpers
        // ──────────────────────────────────────────────────────────────

        private static void LoadJsonIntoDataTable(string json, string dataPath, DataTable dt)
        {
            if (string.IsNullOrWhiteSpace(json)) return;

            var root = JToken.Parse(json);

            JToken target = root;

            if (!string.IsNullOrWhiteSpace(dataPath))
            {
                // Support both "a.b.c" dot-notation and "$.a.b.c" JSONPath
                var path = dataPath.StartsWith("$") ? dataPath : "$." + dataPath;
                target = root.SelectToken(path);

                if (target == null)
                    throw new Exception($"DataPath '{dataPath}' did not match any element in the API response.");
            }

            JArray array = target switch
            {
                JArray ja => ja,
                JObject jo => new JArray(jo),
                _ => throw new Exception($"The resolved DataPath value is not a JSON object or array (type: {target?.Type}).")
            };

            if (array.Count == 0) return;

            // Build columns from the first object
            var firstObj = array[0] as JObject;
            if (firstObj == null) return;

            foreach (var prop in firstObj.Properties())
            {
                if (!dt.Columns.Contains(prop.Name))
                    dt.Columns.Add(prop.Name, typeof(string));
            }

            // Populate rows
            foreach (var item in array)
            {
                if (item is not JObject jObj) continue;

                var row = dt.NewRow();
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName == Const.BuiltInDataTableColumns._IsDeleted
                        || col.ColumnName == Const.BuiltInDataTableColumns._RowBackColor)
                        continue;

                    var prop = jObj[col.ColumnName];
                    if (prop != null)
                    {
                        var strVal = prop.ToObject<object>()?.ToString();
                        row[col] = strVal != null ? (object)strVal : DBNull.Value;
                    }
                    else
                    {
                        row[col] = DBNull.Value;
                    }
                }
                dt.Rows.Add(row);
            }

            dt.AcceptChanges(); //Otherwise all rows are considered new
        }

        private string BuildBody(ApiEditCommand cmd, DataRow dataRow)
        {
            // No template — auto-serialise the entire DataRow as a JSON object
            if (string.IsNullOrWhiteSpace(cmd.Body))
                return BuildJsonBody(dataRow);

            var result = cmd.Body;
            var rootScopes = cmd.Body.GetScopes();
            var parsableBody = string.Empty;

            if (!string.IsNullOrWhiteSpace(cmd.Body) && rootScopes.Count() != 1)
            {
                throw new Exception("Malformed request body");
            }
            else
            {
                parsableBody = rootScopes.First();
            }

            var parameters = DependencyParser.ParseDependencyMetas(parsableBody);

            foreach (var param in parameters)
            {
                string rawValue;

                if (param.IsFromUIState)
                {
                    var dep = DependencyParser.ParseDependency(param.RawPatameter);
                    var uiState = UiStateRegistry[dep.ControlName];

                    if (uiState == null)
                        throw new Exception($"UI state '{dep.ControlName}' not found for body parameter '{param.RawPatameter}'.");

                    var resolved = ResolveBindingPath(uiState, dep.PropertyPath);
                    rawValue = resolved?.ToString() ?? string.Empty;
                }
                else
                {
                    if (dataRow == null)
                    {
                        rawValue = string.Empty;
                    }
                    else
                    {
                        var version = dataRow.RowState == DataRowState.Deleted
                            ? DataRowVersion.Original
                            : DataRowVersion.Current;

                        try
                        {
                            var value = dataRow[param.Name, version];
                            rawValue = value == DBNull.Value ? string.Empty : value?.ToString() ?? string.Empty;
                        }
                        catch
                        {
                            rawValue = string.Empty;
                        }
                    }
                }

                result = result.Replace($"{{{param.RawPatameter}}}", rawValue, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        private static string BuildJsonBody(DataRow dataRow)
        {
            if (dataRow == null) return "{}";

            var version = dataRow.RowState == DataRowState.Deleted
                ? DataRowVersion.Original
                : DataRowVersion.Current;

            var jObj = new JObject();
            foreach (DataColumn col in dataRow.Table.Columns)
            {
                if (col.ColumnName == Const.BuiltInDataTableColumns._IsDeleted
                    || col.ColumnName == Const.BuiltInDataTableColumns._RowBackColor)
                    continue;

                try
                {
                    var value = dataRow[col, version];
                    jObj[col.ColumnName] = value == DBNull.Value ? JValue.CreateNull() : JToken.FromObject(value);
                }
                catch
                {
                    // column not available in this version – skip
                }
            }
            return jObj.ToString(Formatting.None);
        }

        // ──────────────────────────────────────────────────────────────
        // Response message extraction
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Extracts a display message from <paramref name="responseJson"/> using the same
        /// dot-notation / JSONPath mechanic as DataPath.  Returns <c>null</c> when the
        /// response body is empty.  Falls back to the full body when <paramref name="responsePath"/>
        /// is empty or the path produces no match.
        /// </summary>
        private static string ExtractResponseMessage(string responseJson, string responsePath)
        {
            if (string.IsNullOrWhiteSpace(responseJson))
                return null;

            if (string.IsNullOrWhiteSpace(responsePath))
                return responseJson;

            try
            {
                var root = JToken.Parse(responseJson);
                var path = responsePath.StartsWith("$") ? responsePath : "$." + responsePath;
                var token = root.SelectToken(path);

                return token?.ToString() ?? responseJson;
            }
            catch
            {
                return responseJson;
            }
        }

        // ──────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────

        public object ResolveBindingPath(object source, string path)
        {
            if (source == null) return null;

            var dummy = new FrameworkElement { DataContext = source };
            var binding = new Binding(path) { Source = source };

            try
            {
                BindingOperations.SetBinding(dummy, FrameworkElement.TagProperty, binding);
                return dummy.Tag;
            }
            finally
            {
                BindingOperations.ClearBinding(dummy, FrameworkElement.TagProperty);
                dummy.DataContext = null;
            }
        }

        private void SetBusy(bool isBusy)
        {
            if (UiStateRegistry.Get(_dataAdapter.Name) is not BusyUIState busyState)
                return;

            busyState.IsBusy = isBusy;
        }

        public void Dispose() { }
    }
}
