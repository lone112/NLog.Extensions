using System;
using System.Linq;
using Microsoft.Azure;
using NLog.Layouts;
using NLog.LogReceiverService;

namespace NLog.Extensions.AzureTableStorage
{
    public class ReceiverToTable : ILogReceiverServer
    {
        private TableStorageManager _tableManager;
        private Layout _layout;
        private readonly string _storageConnection;
        private readonly string _tableName;

        public ReceiverToTable()
        {
            this._storageConnection = CloudConfigurationManager.GetSetting("StorageConnectionString");
            this._tableName = CloudConfigurationManager.GetSetting("TableName") ?? "b32";
            Init();
        }

        public ReceiverToTable(string storageConnection, string tableName)
        {
            _storageConnection = storageConnection;
            _tableName = tableName;
            Init();
        }

        private void Init()
        {
            this._layout = "${longdate}|${level:uppercase=true}|${logger}|${message}";
            this._tableManager = new TableStorageManager(this._storageConnection, string.Empty, this._tableName);
        }

        public void ProcessLogMessages(NLogEvents nevents)
        {
            var events = nevents.ToEventInfo("Client.");
            Console.WriteLine("in: {0} {1}", nevents.Events.Length, events.Count);
            var items = events.Select(it => new LogEntity(it));
            _tableManager.AddRange(items);
        }
    }
}