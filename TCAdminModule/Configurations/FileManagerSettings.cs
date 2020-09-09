namespace TCAdminModule.Configurations
{
    public class FileManagerSettings
    {
        public string ThumbnailUrl { get; set; } = "https://img.icons8.com/plasticine/256/000000/folder-invoices.png";

        public string HexColor { get; set; } = "#ffb31a";

        public string[] ExitCommand { get; set; } = {"exit", "quit", "q"};

        public string[] GoBackCommand { get; set; } = {"..", "..."};
    }
}