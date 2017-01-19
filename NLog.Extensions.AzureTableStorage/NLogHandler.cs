using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NLog.Extensions.AzureTableStorage
{
    public class NLogHandler : MessageHandler
    {
        private readonly Logger logger;

        public NLogHandler() : this(null)
        {
        }

        public NLogHandler(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = "instrumentation";
            }

            logger = LogManager.GetLogger(name);
        }

        protected override Task IncomingMessageAsync(string correlationId, HttpRequestMessage request, byte[] message)
        {
            LogEventInfo eventInfo = new LogEventInfo();
            eventInfo.Level = LogLevel.Trace;
            eventInfo.LoggerName = logger.Name;
            eventInfo.Properties.Add("ActiveId", correlationId);
            eventInfo.Properties.Add("Method", request.Method);
            eventInfo.Properties.Add("Url", request.RequestUri);
            StringBuilder sb = new StringBuilder();
            sb.Append(request.Method).Append(" ").Append(request.RequestUri);

            try
            {
                string body = Encoding.UTF8.GetString(message);
                sb.Append(" Body: ").Append(body);
            }
            catch (Exception)
            {
                // ignore
                sb.Append(" Can't parse body ");
            }

            eventInfo.Message = sb.ToString();
            logger.Log(eventInfo);

            return Task.FromResult(0);
        }


        protected override Task OutgoingMessageAsync(string correlationId, HttpResponseMessage response, byte[] message, long elapsedMilliseconds)
        {
            LogEventInfo eventInfo = new LogEventInfo();
            eventInfo.Level = LogLevel.Trace;
            eventInfo.LoggerName = logger.Name;
            eventInfo.Properties.Add("ActiveId", correlationId);
            eventInfo.Properties.Add("Method", response.RequestMessage.Method);
            eventInfo.Properties.Add("Url", response.RequestMessage.RequestUri);
            eventInfo.Properties.Add("StatusCode", (int)response.StatusCode);
            StringBuilder sb = new StringBuilder();
            sb.Append(response.RequestMessage.Method).Append(" ").Append(response.RequestMessage.RequestUri).Append(" Code: ").Append((int)response.StatusCode);

            try
            {
                string body = Encoding.UTF8.GetString(message);
                sb.Append(" Body: ").Append(body);
            }
            catch (Exception)
            {
                // ignore
                sb.Append(" Can't parse body ");
            }

            eventInfo.Message = sb.ToString();
            logger.Log(eventInfo);

            return Task.FromResult(0);
        }
    }
}