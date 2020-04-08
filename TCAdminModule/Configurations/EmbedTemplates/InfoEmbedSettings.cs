using DSharpPlus.Entities;

namespace TCAdminModule.Configurations.EmbedTemplates
{
    public class InfoEmbedSettings : EmbedTemplateSettings
    {
        public InfoEmbedSettings()
        {
            this.EmbedBuilder = new DiscordEmbedBuilder
            {
                Title = "Information",
                Description = "Information",
                Color = new Optional<DiscordColor>(new DiscordColor(15132390)),
                ThumbnailUrl = "https://img.icons8.com/office/40/000000/information.png"
            };
        }
    }
}