using DSharpPlus.Entities;

namespace TCAdminModule.Configurations.EmbedTemplates
{
    public class SuccessEmbedSettings : EmbedTemplateSettings
    {
        public SuccessEmbedSettings()
        {
            this.EmbedBuilder = new DiscordEmbedBuilder
            {
                Title = "Success",
                Description = "The task was successful",
                Color = new Optional<DiscordColor>(new DiscordColor(9556540)),
                ThumbnailUrl = "https://img.icons8.com/color/48/000000/checked-2.png"
            };
        }
    }
}