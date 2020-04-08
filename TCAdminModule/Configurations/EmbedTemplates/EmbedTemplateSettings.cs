using DSharpPlus.Entities;

namespace TCAdminModule.Configurations.EmbedTemplates
{
    public class EmbedTemplateSettings
    {
        public DiscordEmbed EmbedBuilder { get; set; } = new DiscordEmbedBuilder();
    }
}