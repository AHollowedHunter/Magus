using Prometheus;

namespace Magus.Bot;

public static class MagusMetrics
{
    public static readonly Counter SlashCommandsExecuted = Metrics.CreateCounter("slash_commands_executed_total"
        , "Total Slash Commands Ran"
        , new CounterConfiguration() { LabelNames = new[] { "command", "subcommand_group", "subcommand", "is_success", "error" }});

    public static readonly Histogram HandleInteractionsDuration = Metrics.CreateHistogram("handle_interactions_duration"
        , "Time taken to handle interaction"
        , new HistogramConfiguration() { LabelNames = new[] { "type" }});


    public static readonly Gauge Guilds = Metrics.CreateGauge("guilds_total", "Total Guilds");
}
