using Coravel.Scheduling.Schedule.Interfaces;
using Discord;
using Discord.WebSocket;
using Magus.Common.Enums;
using Magus.Common.Utilities;
using Magus.Data;
using Magus.Data.Models;
using Microsoft.Extensions.Options;
using System.ServiceModel.Syndication;
using System.Xml;

namespace Magus.Bot.Services
{
    public class AnnouncementService
    {
        public const int DotaAppId = 570;
        public const string DotaFeedUrl = "https://store.steampowered.com/feeds/news/app/570/";

        private readonly IAsyncDataService _db;
        private readonly IDiscordClient _discord;
        private readonly ILogger<AnnouncementService> _logger;
        private readonly IScheduler _scheduler;
        private readonly BotSettings _botSettings;


        public AnnouncementService(IAsyncDataService db, DiscordSocketClient discord, HttpClient httpClient, ILogger<AnnouncementService> logger, IScheduler scheduler, IOptions<BotSettings> botSettings)
        {
            _db = db;
            _discord = discord.Rest;
            _logger = logger;
            _scheduler = scheduler;
            _botSettings = botSettings.Value;
        }

        public async Task Initialise()
        {
            ScheduleGetDotaNews();

            _logger.LogInformation("AnnouncementService Initalised");
        }

        private void ScheduleGetDotaNews() => _scheduler.ScheduleAsync(GetDotaNews)
                                                        .EveryMinute()
                                                        .RunOnceAtStart();

        private async Task GetDotaNews()
        {
            var newAnnouncements = await GetNewDotaAnnouncements();
            foreach (var announcement in newAnnouncements)
            {
                _logger.LogInformation("Processing new Dota news: {id} {title}", announcement.Id, announcement.Title);
                await Task.Delay(1000);
                await SendDotaAnnouncement(announcement);
            }
            await _db.InsertRecords(newAnnouncements);
        }

        private async Task<IList<Announcement>> GetNewDotaAnnouncements()
        {

            XmlReader reader     = XmlReader.Create(DotaFeedUrl);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();

            var latestPublished  = await _db.GetLatestPublishedAnnouncement(Topic.Dota);
            var newAnnouncements = new List<Announcement>();

            foreach (var item in feed.Items.OrderBy(i => i.PublishDate))
            {
                var id          = ulong.Parse(new Uri(item.Id).Segments.Last());
                var publishDate = item.PublishDate.ToUnixTimeSeconds();

                if (publishDate <= (latestPublished?.Date ?? 0))
                    continue;

                var imageUrl = item.Links.Where(l => l.MediaType?.StartsWith("image") ?? false).Select(l => l.Uri.ToString()).FirstOrDefault();

                var announcement = new Announcement()
                {
                    Id       = id,
                    Topic    = Topic.Dota,
                    Url      = item.Id,
                    ImageUrl = imageUrl,
                    Title    = item.Title.Text,
                    Content  = DiscordMessageFormatter.HtmlToDiscordEmbedMarkdown(item.Summary.Text),
                    Date     = publishDate,
                    Locale   = "en",
                };
                newAnnouncements.Add(announcement);
            }
            return newAnnouncements;
        }

        private async Task SendDotaAnnouncement(Announcement announcement)
        {
            var sourceId      = _botSettings.Announcements.DotaSource;
            var sourceChannel = await _discord.GetChannelAsync(sourceId) as INewsChannel;
            
            var content = announcement.Content.Length < 512
                ? announcement.Content
                : announcement.Content[..(512+announcement.Content[512..].IndexOf(" "))] + " ***...***";

            var description = new StringBuilder()
                .AppendLine(content)
                .AppendLine()
                .AppendFormat("<t:{0}:R> - [Original Announcement]({1})", announcement.Date, announcement.Url)
                .ToString();
            var embed = new EmbedBuilder()
                .WithUrl(announcement.Url)
                .WithTitle(announcement.Title)
                .WithDescription(description)
                .WithImageUrl(announcement.ImageUrl)
                .WithColor(Color.DarkRed)
                .Build();

            var sentMessage = await sourceChannel!.SendMessageAsync(embed: embed);
            try
            {
                await sentMessage.CrosspostAsync();
                announcement.IsPublished = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish message for Dota announcements {id}", announcement.Id);
                if (!announcement.IsPublished && sentMessage != null)
                    await sentMessage.DeleteAsync();
            }
        }
    }
}
