using DSharpPlus.Entities;

namespace TCAdminModule.Configurations.EmbedTemplates
{
    public class ErrorEmbedSettings : EmbedTemplateSettings
    {
        public ErrorEmbedSettings()
        {
            this.EmbedBuilder = new DiscordEmbedBuilder
            {
                Title = "Error",
                Description = "The task errored",
                Color = new Optional<DiscordColor>(new DiscordColor(16718362)),
                ThumbnailUrl = "https://img.icons8.com/flat_round/64/000000/error--v1.png"
            };
        }
    }
}