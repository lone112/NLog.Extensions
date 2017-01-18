using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace NLog.Extensions.AzureTableStorage
{
    [Target("AzureTableStorage")]
    public class AzureTableStorageTarget : TargetWithLayout
    {
        private TableStorageManager _tableStorageManager;

        [RequiredParameter]
        public string ConnectionStringKey { get; set; }

        [RequiredParameter]
        public string TableName { get; set; }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            ValidateParameters();
            _tableStorageManager = new TableStorageManager(string.Empty, ConnectionStringKey, TableName);
        }

        private void ValidateParameters()
        {
            IsNameValidForTableStorage(TableName);
        }

        private void IsNameValidForTableStorage(string tableName)
        {
            var validator = new AzureStorageTableNameValidator(tableName);
            if (!validator.IsValid())
            {
                throw new NotSupportedException(tableName + " is not a valid name for Azure storage table name.")
                {
                    HelpLink = "http://msdn.microsoft.com/en-us/library/windowsazure/dd179338.aspx"
                };
            }
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (_tableStorageManager != null)
            {
                var item = ConvertToLogEntity(logEvent);
                _tableStorageManager.Add(item);
            }
        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            if (_tableStorageManager != null)
            {
                var items = logEvents.Select(it => ConvertToLogEntity(it.LogEvent));

                _tableStorageManager.AddRange(items);
            }
        }

        private ITableEntity ConvertToLogEntity(LogEventInfo logEvent)
        {
            try
            {
                if (logEvent.Properties.Count > 0)
                {
                    dynamic it = Create(logEvent);
                    foreach (KeyValuePair<object, object> pair in logEvent.Properties)
                    {
                        string key = pair.Key.ToString();
                        if (string.IsNullOrEmpty(key))
                        {
                            continue;
                        }

                        it[key] = pair.Value.ToString();
                    }

                    return it;
                }
            }
            catch (Exception)
            {
                // ignore
            }

            return new LogEntity(logEvent);
        }

        private ElasticTableEntity Create(LogEventInfo logEvent)
        {
            dynamic it = new ElasticTableEntity();
            it.LoggerName = logEvent.LoggerName;
            it.LogTimeStamp = logEvent.TimeStamp.ToString("o");
            it.Level = logEvent.Level.Name;
            it.Message = logEvent.FormattedMessage;
            if (logEvent.Exception != null)
            {
                it.Exception = logEvent.Exception.ToString();
                if (logEvent.Exception.Data.Count > 0)
                {
                    it.ExceptionData = LogEntity.GetExceptionDataAsString(logEvent.Exception);
                }

                if (logEvent.Exception.InnerException != null)
                {
                    it.InnerException = logEvent.Exception.InnerException.ToString();
                }
            }

            if (logEvent.StackTrace != null)
            {
                it.StackTrace = logEvent.StackTrace.ToString();
            }

            //RowKey = String.Format("{0}__{1}", (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19"), Guid.NewGuid());
            it.RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19") + System.Threading.Thread.CurrentThread.ManagedThreadId;
            it.PartitionKey = logEvent.LoggerName;
            it.MachineName = Environment.MachineName;
            return it;
        }
    }
}
