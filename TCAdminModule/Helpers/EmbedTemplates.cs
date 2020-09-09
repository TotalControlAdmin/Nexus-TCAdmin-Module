using System;
using DSharpPlus.Entities;
using Nexus.SDK.Modules;
using TCAdminModule.Configurations.EmbedTemplates;

namespace TCAdminModule.Helpers
{
    public static class EmbedTemplates
    {
        public static DiscordEmbedBuilder CreateSuccessEmbed(string title = "Success",
            string description = "The task executed successfully", bool showTimestamp = true)
        {
            var config = new NexusModuleConfiguration<SuccessEmbedSettings>("SuccessEmbedConfig",
                "./Config/TCAdminModule/EmbedTemplates/").GetConfiguration();

            var embed = new DiscordEmbedBuilder(config.EmbedBuilder) {Title = title, Description = description};

            if (showTimestamp) embed.WithTimestamp(DateTime.Now);

            return embed;
        }

        public static DiscordEmbedBuilder CreateErrorEmbed(string title = "Error",
            string description = "The task errored", bool showTimestamp = true)
        {
            var config = new NexusModuleConfiguration<ErrorEmbedSettings>("ErrorEmbedConfig",
                "./Config/TCAdminModule/EmbedTemplates/").GetConfiguration();

            var embed = new DiscordEmbedBuilder(config.EmbedBuilder) {Title = title, Description = description};

            if (showTimestamp) embed.WithTimestamp(DateTime.Now);

            return embed;
        }

        public static DiscordEmbedBuilder CreateInfoEmbed(string title,
            string description, bool showTimestamp = true)
        {
            var config = new NexusModuleConfiguration<InfoEmbedSettings>("InfoEmbedConfig",
                "./Config/TCAdminModule/EmbedTemplates/").GetConfiguration();

            var embed = new DiscordEmbedBuilder(config.EmbedBuilder) {Title = title, Description = description};

            if (showTimestamp) embed.WithTimestamp(DateTime.Now);

            return embed;
        }
    }
}