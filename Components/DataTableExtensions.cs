using System.Data;

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public static class DataTableExtensions
    {
      static public DataTable Top(this DataTable dataTable, int topCount)
        {
            var topFilter = topCount;
            if (topFilter > 0)
            {
                for (var index = dataTable.Rows.Count - 1; index >= topFilter; index--)
                {
                    dataTable.Rows.RemoveAt(index);
                }
            }
            return dataTable;
        }

        static public DataTable FilterAndSort(this DataTable dataTable, string filter, string sortField, string sortOrder)
        {
            var sort = string.Empty;
            if (sortField != string.Empty && sortOrder != string.Empty)
            {
                sort = string.Format("{0} {1}", sortField, sortOrder);
            }
            var dv = new DataView(dataTable) {RowFilter = filter, Sort = sort};

            var filteredtable = dv.ToTable();
            dataTable.Clear();
            dataTable.Merge(filteredtable);
            return dataTable;
        }

        static public DataTable Page(this DataTable dataTable, int pageIndex, int pageSize)
        {
            if (pageSize > 0)
            {
                var indexOfFirstItem = pageIndex * pageSize;
                var rows = dataTable.Rows;
                if (indexOfFirstItem < rows.Count)
                {
                    for (var index = indexOfFirstItem - 1; index >= 0; index--)
                    {
                        rows.RemoveAt(index);
                    }
                }

                for (var index = rows.Count - 1; index >= pageSize; index--)
                {
                    rows.RemoveAt(index);
                }
            }
            return dataTable;
        }
    }
}