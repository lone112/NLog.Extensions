using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace NLog.Extensions.AzureTableStorage
{
    public class TableStorageManager
    {
        private readonly string _connectionString;
        private readonly CloudTable _cloudTable;

        public TableStorageManager(string connectionString, string connectionKey, string tableName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                this._connectionString = CloudConfigurationManager.GetSetting(connectionKey);
            }
            else
            {
                this._connectionString = connectionString;
            }

            if (string.IsNullOrWhiteSpace(this._connectionString))
            {
                throw new Exception("Please config connectionstring  or key");
            }

            var storageAccount = GetStorageAccount();
            // Create the table client.
            var tableClient = storageAccount.CreateCloudTableClient();
            //create charts table if not exists.
            _cloudTable = tableClient.GetTableReference(tableName);
            _cloudTable.CreateIfNotExists();
        }

        public void Add(ITableEntity entity)
        {
            var insertOperation = TableOperation.Insert(entity);
            _cloudTable.Execute(insertOperation);
        }

        public void AddRange(IEnumerable<ITableEntity> items)
        {
            var query = items.GroupBy(it => it.PartitionKey);
            foreach (IGrouping<string, ITableEntity> gdata in query)
            {
                AddRangeItems(gdata.ToArray());
            }
        }

        public void AddRangeItems(IEnumerable<ITableEntity> items)
        {
            TableBatchOperation batchOperation = new TableBatchOperation();
            int count = 0;
            foreach (ITableEntity entity in items)
            {
                entity.RowKey = entity.RowKey + count;
                batchOperation.Add(TableOperation.Insert(entity));
                count++;
                if (count >= 99)
                {
                    _cloudTable.ExecuteBatch(batchOperation);
                    batchOperation = new TableBatchOperation();
                }
            }

            if (batchOperation.Count > 0)
            {
                _cloudTable.ExecuteBatch(batchOperation);
            }
        }

        private CloudStorageAccount GetStorageAccount()
        {
            return CloudStorageAccount.Parse(this._connectionString);
        }
    }
}
