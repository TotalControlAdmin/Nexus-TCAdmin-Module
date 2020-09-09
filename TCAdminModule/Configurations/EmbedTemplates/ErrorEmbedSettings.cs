using DSharpPlus.Entities;

namespace TCAdminModule.Configurations.EmbedTemplates
{
    public class ErrorEmbedSettings : EmbedTemplateSettings
    {
        public ErrorEmbedSettings()
        {
            EmbedBuilder = new DiscordEmbedBuilder
            {
                Title = "Error",
                Description = "The task errored",
                Color = new Optional<DiscordColor>(new DiscordColor(16718362)),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = "https://img.icons8.com/flat_round/64/000000/error--v1.png"
                }
            };
        }
    }
}