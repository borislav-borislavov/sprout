
using CommunityToolkit.Mvvm.ComponentModel;
using Sprout.Core.Services.Queries;
using System.ComponentModel;
using System.Data;

namespace Sprout.Core.Models.Queries
{
    public partial class Query : ObservableObject, IDataProvider
    {
        public string Name { get; set; }

        public QueryCommand InsertCommand { get; set; }
        public QueryCommand UpdateCommand { get; set; }
        public QueryCommand DeleteCommand { get; set; }

        /// <summary>
        /// The Root table name of the query. Used for automatic updates.
        /// </summary>
        public string TableName { get; set; }
        public List<string> PrimaryKeys { get; set; } = [];
        public string Text { get; set; }


        [ObservableProperty]
        private DataTable _data = new();

        public Query()
        {
            InsertCommand = new QueryCommand(this, QueryCommandTypes.Insert);
            UpdateCommand = new QueryCommand(this, QueryCommandTypes.Update);
            DeleteCommand = new QueryCommand(this, QueryCommandTypes.Delete);
        }

        partial void OnDataChanged(DataTable value)
        {
            if (value == null) return;

            value.ColumnChanged += Data_ColumnChanged;
        }

        private void Data_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            var row = e.Row;
            var column = e.Column!.ColumnName;
            var newValue = e.ProposedValue;
            var oldValue = row[column, DataRowVersion.Original];

            // Guard: ignore unchanged values
            if (Equals(oldValue, newValue))
                return;

#warning left off here
            //avoid the granular updates, use bulk changes instead

            //UpdateDatabase(
            //    id: row["Id"],
            //    column: column,
            //    value: newValue
            //);
        }

        public string ConnectionString { get; internal set; }
        public IEnumerable<QueryDependency> Dependencies { get; internal set; } = [];
    }

    public class QueryDependency
    {
        public string RawDependency { get; set; }
        public string ControlName { get; set; }
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public string[] Extra { get; internal set; }
    }

#warning QueryCommandTypes might be redundant
    public enum QueryCommandTypes
    {
        Insert,
        Update,
        Delete,
    }

    public class QueryCommand
    {
        public QueryCommandTypes Type { get; set; }
        public string Text { get; internal set; }
        public Dictionary<string, string> DefaultValues { get; internal set; }

        public Query Parent { get; }

        public QueryCommand(Query parent, QueryCommandTypes type)
        {
            Type = type;
            Parent = parent;
        }
    }

    public class QueryColumn
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsKey { get; set; }
    }

    public interface IDataProvider
    {
        DataTable Data { get; set; }
    }
}
