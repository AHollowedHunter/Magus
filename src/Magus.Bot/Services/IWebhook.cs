namespace Magus.Bot.Services
{
    public interface IWebhook : IDisposable
    {
        public Task<bool> SendMessage(string message, string webhook);
    }
}
