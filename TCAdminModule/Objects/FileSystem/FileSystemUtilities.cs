using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TCAdmin.SDK;
using TCAdmin.SDK.Misc;
using TCAdmin.SDK.Objects;
using TCAdmin.SDK.VirtualFileSystem;
using TCAdmin.SDK.Web.FileManager;
using TCAdmin.SDK.Web.References.FileSystem;
using TCAdminModule.Helpers;
using FileManagerDirectory = TCAdminModule.Objects.Actions.FileManagerDirectory;

namespace TCAdminModule.Objects.FileSystem
{
    using DirectoryInfo = System.IO.DirectoryInfo;
    using FileInfo = TCAdmin.SDK.Web.References.FileSystem.FileInfo;
    using FileSystem = TCAdmin.SDK.Web.References.FileSystem.FileSystem;
    using Permission = TCAdmin.SDK.VirtualFileSystem.Permission;
    using Server = TCAdmin.SDK.Objects.Server;
    using Service = TCAdmin.GameHosting.SDK.Objects.Service;
    using VirtualDirectorySecurity = VirtualDirectorySecurity;
    using WebClient = System.Net.WebClient;

    public class FileSystemUtilities
    {
        public enum EDirectoryActions
        {
            CreateFile = 'N',

            Compress = 'C',

            UploadFile = 'U',

            DeleteFolder = 'D'
        }

        public enum EFileActions
        {
            Copy = 1,

            Delete,

            Download,

            Edit,

            Extract,

            Rename
        }

        public enum EListingType
        {
            Directory,

            File,

            Unknown
        }

        public FileSystemUtilities(VirtualDirectorySecurity vds, Server server, Service service, CommandContext ctx)
        {
            FileSystem = server.FileSystemService;
            VirtualDirectorySecurity = vds;
            Server = server;
            Service = service;
            CommandContext = ctx;
        }

        public FileSystemUtilities(VirtualDirectorySecurity vds, Server server, CommandContext ctx)
        {
            FileSystem = server.FileSystemService;
            VirtualDirectorySecurity = vds;
            Server = server;
            Service = new Service
            {
                WorkingDirectory = "/",
                RootDirectory = "/",
                Executable = "/"
            };
            CommandContext = ctx;
        }

        private FileSystem FileSystem { get; }

        private VirtualDirectorySecurity VirtualDirectorySecurity { get; }

        private Server Server { get; }

        private Service Service { get; }

        private CommandContext CommandContext { get; }

        public bool CanGoBack(string currentDirectory, int serviceId)
        {
            var directoryInfo = new DirectoryInfo(currentDirectory).Parent;
            if (directoryInfo != null)
            {
                var parentDirectory = directoryInfo.FullName;
                if (!parentDirectory.Contains(serviceId.ToString()))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public async Task<bool> DirectoryAction(EDirectoryActions option, DirectoryListing listing,
            string currentDirectory)
        {
            var directory = new FileManagerDirectory(CommandContext, Server, VirtualDirectorySecurity, listing,
                currentDirectory);
            switch (option)
            {
                case EDirectoryActions.CreateFile:
                    await directory.CreateFile();
                    return true;
                case EDirectoryActions.UploadFile:
                    await directory.UploadFile();
                    return true;
                case EDirectoryActions.DeleteFolder:
                    await directory.DeleteDirectory();
                    return true;
                case EDirectoryActions.Compress:
                    await directory.Compress();
                    return true;
            }

            return false;
        }

        public async Task DownloadFile(FileInfo file)
        {
            var server = new Server(Service.ServerId);
            var directorySecurity = new TCAdmin.SDK.VirtualFileSystem.VirtualDirectorySecurity
            {
                Permissions = Permission.Read | Permission.Write | Permission.Delete,
                PermissionType = PermissionType.Root,
                RelativeExecutable =
                    Strings.ReplaceCaseInsensitive(Service.Executable, Service.RootDirectory, string.Empty),
                VirtualDirectoryName = Service.ConnectionInfo,
                RootPhysicalPath = Service.RootDirectory,
                RealPhysicalPath =
                    TCAdmin.SDK.Misc.FileSystem.FixAbsoluteFilePath(Service.RootDirectory, server.OperatingSystem),
                ServerId = server.ServerId
            };
            var download = new RemoteDownload(server)
            {
                DirectorySecurity = directorySecurity,
                FileName = file.FullName
            };

            if (file.Length / 1024 > 8)
            {
                await CommandContext.RespondAsync(
                    embed: EmbedTemplates.CreateSuccessEmbed(
                        $"[<{download.GetDownloadUrl()}>](**Click here to download {file.Name}**)"));
            }
            else
            {
                using (var client = new WebClient())
                {
                    client.DownloadFileCompleted += delegate { CommandContext.RespondWithFileAsync(file.Name); };

                    client.DownloadFileAsync(new Uri(download.GetDownloadUrl()), file.Name);
                }
            }

            await Task.Delay(3000);
            File.Delete(file.Name);
        }

        public async Task<bool> FileAction(FileInfo file, EFileActions option)
        {
            switch (option)
            {
                case EFileActions.Delete:
                    await DeleteFile(file);
                    return true;
                case EFileActions.Copy:
                    await CopyFile(file);
                    return true;
                case EFileActions.Edit:
                    await EditFile(file);
                    return true;
                case EFileActions.Extract:
                    await ExtractFile(file);
                    return true;
                case EFileActions.Rename:
                    await RenameFile(file);
                    return true;
                case EFileActions.Download:
                    await DownloadFile(file);
                    return true;
            }

            return false;
        }

        public DirectoryListing GenerateListingDirectory(string directory,
            VirtualDirectorySecurity virtualDirectorySecurity)
        {
            var parsedDirectory = directory.Replace("..", string.Empty);

            var listing = FileSystem.GetDirectoryListing(parsedDirectory,
                virtualDirectorySecurity.VirtualDirectorySecurityString, true, true);
            return listing;
        }

        public FileInfo GetFile(string dir, string file)
        {
            try
            {
                var files = FileSystem.GetFiles(dir);
                return files.FirstOrDefault(x => x.Name == file);
            }
            catch (Exception e)
            {
                LogManager.WriteError(e);
                return null;
            }
        }

        public string GetFileContent(FileInfo file)
        {
            var fileBytes = FileSystem.ReadFile(file.FullName);
            var fileContent = Encoding.UTF8.GetString(fileBytes);
            return fileContent;
        }

        public FileInfo GetFileInfo(int fileId, DirectoryListing directoryListing)
        {
            var file = directoryListing.Files[fileId - 1];

            return file;
        }

        public EListingType GetListingType(int index, DirectoryListing directoryListing)
        {
            try
            {
                if (FileSystem.DirectoryExists(directoryListing.Directories[index - 1].FullName))
                    return EListingType.Directory;
            }
            catch
            {
                return EListingType.File;
            }

            return EListingType.Unknown;
        }

        public DirectoryListing NavigateBackFolder(string currentDirectory)
        {
            var directoryInfo = new DirectoryInfo(currentDirectory).Parent;
            if (directoryInfo != null)
            {
                var parentDirectory = directoryInfo.FullName;
                return GenerateListingDirectory(parentDirectory);
            }

            return GenerateListingDirectory(currentDirectory);
        }

        public DirectoryListing NavigateCurrentFolder(string currentDirectory)
        {
            return GenerateListingDirectory(currentDirectory);
        }

        public DirectoryListing NavigateNextFolder(int nextDirectory, DirectoryListing directoryListing)
        {
            return GenerateListingDirectory(directoryListing.Directories[nextDirectory - 1].FullName);
        }

        private async Task CopyFile(FileInfo file)
        {
            var interactivity = CommandContext.Client.GetInteractivity();
            var msg = await CommandContext.RespondAsync(
                $"Enter absolute path you would like to copy {file.Name} to.\nE.G. **Servers/server**");
            var copyTo = await interactivity.WaitForMessageAsync(x => x.Author.Id == CommandContext.User.Id);
            FileSystem.CopyFile(file.FullName, Service.WorkingDirectory + copyTo.Result.Content + "/" + file.Name);
            await CommandContext.RespondAsync(
                $"Copied {file.Name} to {Service.WorkingDirectory}/{copyTo.Result.Content}");
            await CleanUp(CommandContext.Channel, msg);
        }

        private async Task DeleteFile(FileInfo file)
        {
            FileSystem.DeleteFile(file.FullName);
            var msg = await CommandContext.RespondAsync($"Deleted **{file.FullName}**");
            await CleanUp(CommandContext.Channel, msg);
        }

        private async Task EditFile(FileInfo file)
        {
            var interactivity = CommandContext.Client.GetInteractivity();
            await DownloadFile(file);
            await CommandContext.RespondAsync("**Please send back the file as an attachment.**");
            var updatedFileAttachment = await interactivity.WaitForMessageAsync(
                x => x.Author.Id == CommandContext.User.Id && x.Channel.Id == CommandContext.Channel.Id
                                                           && x.Attachments.Any(
                                                               y => y.FileName.EndsWith(file.Extension)),
                TimeSpan.FromMinutes(5));

            if (FitsMask(updatedFileAttachment.Result.Attachments[0].FileName))
            {
                await CommandContext.RespondAsync("This File Type is banned.");
                return;
            }

            var updatedFileContent = DownloadString(updatedFileAttachment.Result.Attachments[0].Url);

            FileSystem.DeleteFile(file.FullName);
            FileSystem.AppendTextFile(file.FullName, Encoding.ASCII.GetBytes(updatedFileContent));
            await CommandContext.RespondAsync($"Edited **{file.Name}**");
        }

        private async Task ExtractFile(FileInfo file)
        {
            try
            {
                var msg = await CommandContext.RespondAsync("Extracting");
                FileSystem.Extract(file.FullName, file.Directory,
                    new VirtualDirectorySecurity(file.Directory + "\\").VirtualDirectorySecurityString);
                await CommandContext.RespondAsync("Extracted");
                await CleanUp(CommandContext.Channel, msg);
            }
            catch (Exception e)
            {
                await CommandContext.RespondAsync(e.Message);
            }
        }

        private async Task RenameFile(FileInfo file)
        {
            var interactivity = CommandContext.Client.GetInteractivity();
            var msg = await CommandContext.RespondAsync($"**What would you like to rename '{file.Name}' to?**");
            var renameName = await interactivity.WaitForMessageAsync(x =>
                x.Author.Id == CommandContext.User.Id && x.Channel.Id == CommandContext.Channel.Id);

            if (FitsMask(renameName.Result.Content))
            {
                await CommandContext.RespondAsync("This File Type is banned.");
                return;
            }

            FileSystem.RenameFile(file.FullName, file.Directory + renameName.Result.Content, "None", "Discord", "None");
            await CommandContext.RespondAsync($"Renamed **{file.Name}** to **{renameName.Result.Content}**");

            await CleanUp(CommandContext.Channel, msg);
        }

        private DirectoryListing GenerateListingDirectory(string directory)
        {
            var parsedDirectory = directory.Replace("..", string.Empty);

            var listing = FileSystem.GetDirectoryListing(parsedDirectory,
                new VirtualDirectorySecurity(directory).VirtualDirectorySecurityString, true, true);
            return listing;
        }

        private async Task CleanUp(DiscordChannel channel, DiscordMessage afterMessage)
        {
            var messages = await channel.GetMessagesAfterAsync(afterMessage.Id);
            if (messages.Count > 1)
            {
                await channel.DeleteMessagesAsync(messages);
            }

            await channel.DeleteMessageAsync(afterMessage);
        }

        private string DownloadString(string address)
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(address);
            }
        }

        private bool FitsMask(string sFileName)
        {
            if (VirtualDirectorySecurity.UserType == UserType.Admin)
            {
                return false;
            }

            var bannedExtensions = VirtualDirectorySecurity.VirtualDirectorySecurityObject.AdditionalPermissions[0]
                .Filters.Split(';');
            foreach (var bannedExt in bannedExtensions)
            {
                var mask = new Regex(bannedExt.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));
                if (mask.IsMatch(sFileName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}