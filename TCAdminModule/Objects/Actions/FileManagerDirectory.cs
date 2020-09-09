using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Nexus.Exceptions;
using TCAdmin.SDK.Objects;
using TCAdmin.SDK.Web.References.FileSystem;

namespace TCAdminModule.Objects.Actions
{
    public class FileManagerDirectory
    {
        public FileManagerDirectory(CommandContext commandContext, Server server,
            VirtualDirectorySecurity vds, DirectoryListing listing, string currentDirectory)
        {
            CommandContext = commandContext;
            FileSystem = server.FileSystemService;
            Listing = listing;
            CurrentDirectory = currentDirectory;
            VirtualDirectorySecurity = vds;
        }

        private CommandContext CommandContext { get; }

        private VirtualDirectorySecurity VirtualDirectorySecurity { get; }

        private TCAdmin.SDK.Web.References.FileSystem.FileSystem FileSystem { get; }

        private DirectoryListing Listing { get; }

        private string CurrentDirectory { get; }

        public async Task Compress()
        {
            var msg = await CommandContext.RespondAsync("**Compressing...** *Please Wait*");

            var directorySecurity = new VirtualDirectorySecurity(CurrentDirectory);

            var files = Listing.Files.Select(file => file.Name).ToList();
            files.AddRange(Listing.Directories.Select(dir => dir.Name));

            try
            {
                FileSystem.CompressFiles(CurrentDirectory, files.ToArray(),
                    directorySecurity.VirtualDirectorySecurityString, 5000000000);
                await CommandContext.RespondAsync("**Compress Completed.**");
            }
            catch (Exception e)
            {
                await CommandContext.RespondAsync(e.Message);
            }

            await CleanUp(CommandContext.Channel, msg);
        }

        public async Task CreateFile()
        {
            var interactivity = CommandContext.Client.GetInteractivity();
            var msg = await CommandContext.RespondAsync("What would you like the file to be called?");
            var waitForFileName = await interactivity.WaitForMessageAsync(x =>
                x.Author.Id == CommandContext.User.Id && x.Channel.Id == CommandContext.Channel.Id);
            var fileName = waitForFileName.Result.Content;

            if (FitsMask(fileName))
            {
                await CommandContext.RespondAsync("**This File Type is banned!**");
                return;
            }

            FileSystem.CreateTextFile(CurrentDirectory + fileName, Array.Empty<byte>());
            await CommandContext.RespondAsync($"File **{fileName}** created in **{CurrentDirectory}**");

            await CleanUp(CommandContext.Channel, msg);
        }

        public async Task DeleteDirectory()
        {
            var msg = await CommandContext.RespondAsync("Deleting " + CurrentDirectory);

            FileSystem.DeleteDirectory(CurrentDirectory);

            await CleanUp(CommandContext.Channel, msg);
        }

        public async Task UploadFile()
        {
            var interactivity = CommandContext.Client.GetInteractivity();
            var msg = await CommandContext.RespondAsync("Please upload a file to send to the server.");
            var waitForFile = await interactivity.WaitForMessageAsync(x =>
                x.Author.Id == CommandContext.User.Id && x.Channel.Id == CommandContext.Channel.Id);
            if (waitForFile.Result.Attachments.Count == 0)
                throw new CustomMessageException("There was no attachments sent in the message by " +
                                                 CommandContext.User);
            var fileUploaded = waitForFile.Result.Attachments[0];

            if (FitsMask(fileUploaded.FileName))
            {
                await CommandContext.RespondAsync("**This File Type is banned!**");
                return;
            }

            FileSystem.DownloadFile(CurrentDirectory + "/" + fileUploaded.FileName, fileUploaded.Url);

            await CommandContext.RespondAsync("Upload Complete.");

            await CleanUp(CommandContext.Channel, msg);
        }

        private async Task CleanUp(DiscordChannel channel, DiscordMessage afterMessage)
        {
            var messages = await channel.GetMessagesAfterAsync(afterMessage.Id);
            if (messages.Count > 1) await channel.DeleteMessagesAsync(messages);

            await channel.DeleteMessageAsync(afterMessage);
        }

        private bool FitsMask(string sFileName)
        {
            if (VirtualDirectorySecurity.UserType == UserType.Admin) return false;

            var bannedExtensions = VirtualDirectorySecurity.VirtualDirectorySecurityObject.AdditionalPermissions[0]
                .Filters.Split(';');
            return bannedExtensions
                .Select(bannedExt => new Regex(bannedExt.Replace(".", "[.]").Replace("*", ".*").Replace("?", ".")))
                .Any(mask => mask.IsMatch(sFileName));
        }
    }
}