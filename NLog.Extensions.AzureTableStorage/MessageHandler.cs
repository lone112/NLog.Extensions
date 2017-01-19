using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Extensions.AzureTableStorage
{
    public abstract class MessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var corrId = Guid.NewGuid().ToString();

            var requestMessage = await request.Content.ReadAsByteArrayAsync();

            await this.IncomingMessageAsync(corrId, request, requestMessage);

            var response = await base.SendAsync(request, cancellationToken);

            byte[] responseMessage;

            if (response.IsSuccessStatusCode)
            {
                responseMessage = await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);
            }

            stopwatch.Stop();
            await this.OutgoingMessageAsync(corrId, response, responseMessage, stopwatch.ElapsedMilliseconds);

            return response;
        }

        protected abstract Task IncomingMessageAsync(string correlationId, HttpRequestMessage request, byte[] message);

        protected abstract Task OutgoingMessageAsync(string correlationId, HttpResponseMessage response, byte[] message, long elapsedMilliseconds);
    }
}
