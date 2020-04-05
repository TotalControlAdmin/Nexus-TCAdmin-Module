namespace TCAdminModule.Configurations
{
    public class UpdateConfigurationCronSettings
    {
        public bool Enable { get; set; } = true;

        public int UpdateEverySeconds { get; set; } = 7_200_000;
    }
}