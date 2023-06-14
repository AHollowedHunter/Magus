using Prometheus;

namespace Magus.Bot
{
    public static class MagusMetrics
    {
        public static readonly Counter SlashCommandsExecuted = Metrics.CreateCounter("magus_slash_commands_executed"
            , "Total Slash Commands Ran"
            , new CounterConfiguration() { LabelNames = new[] { "name" }});
    }
}
