using System.Text;

namespace Magus.Bot.Services
{
    public class DiscordWebhook : IWebhook
    {
        private bool disposedValue;
        private readonly HttpClient httpClient;

        public DiscordWebhook()
        {
            this.httpClient = new HttpClient();
        }

        public async Task<bool> SendMessage(string message, string webhook)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, webhook);
            requestMessage.Content = new StringContent(message, encoding: Encoding.UTF8, "application/json");
            var response = await httpClient.SendAsync(requestMessage);
            return response.IsSuccessStatusCode;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    httpClient.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
