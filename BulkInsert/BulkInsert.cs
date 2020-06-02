using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

using System.Data.Entity.Core.EntityClient;

namespace BulkInserts
{
    public class BulkInsert : IDisposable
    {
        private SqlConnection connection = new SqlConnection();
        private bool isConnectionOpen = false;
        public BulkInsert()
        {
            CreateConnection();
        }

        private void CreateConnection()
        {
            EntityConnectionStringBuilder stringBuilder = new EntityConnectionStringBuilder(ConfigurationManager.ConnectionStrings["AppEntities"].ConnectionString);
            string connectionStringasdfasdf = stringBuilder.ProviderConnectionString;
            connection = new SqlConnection(connectionStringasdfasdf);
            connection.Open();
            isConnectionOpen = true;
        }


        //Number of inserts per commit
        private int CommitBatchSize = 2000;

        public void Commit<T>(IList<T> data)
        {
            if (data.Count > 0)
            {
                DataTable dataTable;
                int numberOfPages = (data.Count / CommitBatchSize) + (data.Count % CommitBatchSize == 0 ? 0 : 1);
                for (int pageIndex = 0; pageIndex < numberOfPages; pageIndex++)
                {
                    // ToDataTable() & ToType comes from the helper class in this same folder
                    dataTable = data.Skip(pageIndex * CommitBatchSize).Take(CommitBatchSize).ToDataTable();
                    BulkInsertData(dataTable, data.ToType());
                }
            }
        }

        private void BulkInsertData(DataTable dataTable, Type type)
        {
            // Make sure to enable triggers
            SqlBulkCopy bulkCopy = new SqlBulkCopy
            (
                connection,
                SqlBulkCopyOptions.TableLock |
                SqlBulkCopyOptions.FireTriggers |
                SqlBulkCopyOptions.UseInternalTransaction |
                SqlBulkCopyOptions.KeepIdentity,
                null
            );

            // Create mapping between columns in the domain & SQL.
            // This is assumes that the classes in Domain have the same name as the SQL tables
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(type);
            foreach (PropertyDescriptor prop in properties)
            {
                //Exclude Virtual Types
                // This allows paralel usage with Entity Framework
                if (!prop.ComponentType.GetProperty(prop.Name).GetGetMethod().IsVirtual)
                {
                    var t = prop.Attributes.OfType<ColumnAttribute>().FirstOrDefault();

                    ColumnAttribute column = prop.Attributes.OfType<ColumnAttribute>().FirstOrDefault();
                    string columnName = column == null ? prop.Name : column.Name;
                    bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(prop.Name, columnName));
                }
            }

            // Set the destination table name
            bulkCopy.DestinationTableName = dataTable.TableName;
            if (!isConnectionOpen) { CreateConnection(); }

            // Write the data in the "dataTable"
            bulkCopy.WriteToServer(dataTable);

        }

        public void Dispose()
        {
            if (isConnectionOpen) { connection.Close(); }
        }
    }
}
