 using System;
 using System.IO;
 using System.Linq;
 using System.Threading.Tasks;
 using DSharpPlus.CommandsNext;
 using DSharpPlus.Entities;
 using DSharpPlus.Interactivity;
 using Nexus.SDK.Modules;
 using TCAdmin.GameHosting.SDK.Objects;
 using TCAdmin.SDK.Web.References.FileSystem;
 using TCAdminModule.Attributes;
 using TCAdminModule.Configurations;
 using TCAdminModule.Objects.FileSystem;
 using DirectoryInfo = System.IO.DirectoryInfo;
 using FileInfo = TCAdmin.SDK.Web.References.FileSystem.FileInfo;
 
 namespace TCAdminModule.Objects.Emulators
{
    public class TcaFileManager : IDisposable
    {
        public TcaFileManager(CommandContext ctx, CommandAttributes.RequireAuthentication authenticationService, string rootDir, bool lockDirectory = false)
        {
            AuthenticationService = authenticationService;

            var service = authenticationService.Service;
            var user = authenticationService.User;

            Server = new Server(service.ServerId);
            FileSystem = Server.FileSystemService;
            CurrentDirectory = rootDir + "\\";
            LockDirectory = lockDirectory;

            VirtualDirectorySecurity = new VirtualDirectorySecurity(FileSystem, CurrentDirectory, user.UserType, service.GameId);
            FileSystemUtilities = new FileSystemUtilities(VirtualDirectorySecurity, Server, service, ctx);
            CurrentListing = FileSystemUtilities.GenerateListingDirectory(CurrentDirectory, VirtualDirectorySecurity);
            CommandContext = ctx;

            IsServer = false;
        }

        public TcaFileManager(CommandContext ctx, Server server, string rootDir, bool lockDirectory = false)
        {
            Server = server;
            FileSystem = Server.FileSystemService;
            CurrentDirectory = rootDir + "\\";
            LockDirectory = lockDirectory;

            VirtualDirectorySecurity = new VirtualDirectorySecurity(CurrentDirectory);
            FileSystemUtilities = new FileSystemUtilities(VirtualDirectorySecurity, Server, ctx);
            CurrentListing = FileSystemUtilities.GenerateListingDirectory(CurrentDirectory, VirtualDirectorySecurity);
            CommandContext = ctx;

            IsServer = true;
        }

        private TCAdmin.SDK.Web.References.FileSystem.FileSystem FileSystem { get; }

        private VirtualDirectorySecurity VirtualDirectorySecurity { get; }

        public string CurrentDirectory { get; set; }

        private DiscordMessage ListingMessage { get; set; }

        private bool LockDirectory { get; set; }
        
        private DirectoryListing CurrentListing { get; set; }
        
        private Server Server { get; }

        private CommandContext CommandContext { get; }

        private CommandAttributes.RequireAuthentication AuthenticationService { get; }

        private FileSystemUtilities FileSystemUtilities { get; }
        
        private FileManagerSettings _settings = new NexusModuleConfiguration<FileManagerSettings>("FileManagerSettings", "./Config/TCAdminModule/").GetConfiguration();
        
        private bool IsServer { get; }

        public void Dispose()
        {
            FileSystem?.Dispose();
            ((IDisposable) Server)?.Dispose();
        }

        public async Task InitializeFileManagerAsync()
        {
            var interactivity = CommandContext.Client.GetInteractivity();
            var waitMsg = await CommandContext.RespondAsync("Please wait");

            if (!FileSystem.DirectoryExists(CurrentDirectory))
            {
                throw new DirectoryNotFoundException("No directory exists on the remote server.");
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = "File Manager", Color = DiscordColor.None, Description = $"**Navigating {CurrentDirectory}**\n\n"
            };
            embed = UpdateEmbedListing(embed);

            await waitMsg.DeleteAsync();

            ListingMessage = await CommandContext.RespondAsync(embed: embed);

            var navigating = true;
            var currentlyInFile = false;
            FileInfo currentFileInfo = null;
            while (navigating)
            {
                var choice = await interactivity.WaitForMessageAsync(x => x.Author.Id == CommandContext.User.Id && x.Channel.Id == CommandContext.Channel.Id);
                if (choice.TimedOut)
                {
                    await CommandContext.RespondAsync("Ended action");
                    navigating = false;
                    continue;
                }

                var message = choice.Result.Content.ToLower();
                await choice.Result.DeleteAsync();

                if (_settings.ExitCommand.Contains(message))
                {
                    await ListingMessage.DeleteAsync();

                    return;
                }

                if (_settings.GoBackCommand.Contains(message))
                {
                    if (currentlyInFile)
                    {
                        CurrentListing = FileSystemUtilities.NavigateCurrentFolder(currentFileInfo.Directory);
                        currentFileInfo = null;
                        currentlyInFile = false;
                        var updatedEmbed = UpdateEmbedListing(embed);
                        await ListingMessage.ModifyAsync(embed: updatedEmbed.Build());
                    }
                    else
                    {
                        if (IsServer)
                        {
                            CurrentListing = FileSystemUtilities.NavigateBackFolder(CurrentDirectory + "\\");
                            CurrentDirectory = new DirectoryInfo(CurrentDirectory).Parent?.FullName + "\\";
                            var updatedEmbed = UpdateEmbedListing(embed);
                            await ListingMessage.ModifyAsync(embed: updatedEmbed.Build());
                            continue;
                        }

                        if (FileSystemUtilities.CanGoBack(CurrentDirectory + "\\", AuthenticationService.Service.ServiceId) && !LockDirectory)
                        {
                            CurrentListing = FileSystemUtilities.NavigateBackFolder(CurrentDirectory + "\\");
                            CurrentDirectory = new DirectoryInfo(CurrentDirectory).Parent?.FullName + "\\";
                            var updatedEmbed = UpdateEmbedListing(embed);
                            await ListingMessage.ModifyAsync(embed: updatedEmbed.Build());
                        }
                    }

                    continue;
                }

                if (int.TryParse(message, out var index))
                {
                    if (currentlyInFile)
                    {
                        var fileAction = (FileSystemUtilities.EFileActions) index;
                        if (await FileSystemUtilities.FileAction(currentFileInfo, fileAction))
                        {
                            VirtualDirectorySecurity.AddAction(
                                $"{CommandContext.Member}\n{CommandContext.Channel}",
                                $"Action: {fileAction.ToString()}\nAffected file: {currentFileInfo.FullName}",
                                DateTime.UtcNow);

                            if (fileAction == FileSystemUtilities.EFileActions.Extract || fileAction == FileSystemUtilities.EFileActions.Delete)
                            {
                                CurrentListing = FileSystemUtilities.NavigateCurrentFolder(CurrentDirectory);
                                var updatedEmbed = UpdateEmbedListing(embed);
                                await ListingMessage.ModifyAsync(embed: updatedEmbed.Build());
                                currentlyInFile = false;
                                currentFileInfo = null;
                            }
                            else
                            {
                                var updatedEmbed = UpdateEmbedListing(currentFileInfo, embed);
                                await ListingMessage.ModifyAsync(embed: updatedEmbed.Build());
                            }
                        }

                        continue;
                    }

                    var type = FileSystemUtilities.GetListingType(int.Parse(message), CurrentListing);

                    if (type == FileSystemUtilities.EListingType.Directory)
                    {
                        CurrentDirectory = CurrentListing.Directories[index - 1].FullName + "\\";
                        CurrentListing = FileSystemUtilities.NavigateNextFolder(index, CurrentListing);
                        var updatedEmbed = UpdateEmbedListing(embed);
                        await ListingMessage.ModifyAsync(embed: updatedEmbed.Build());
                    }
                    else
                    {
                        var fileInfo = FileSystemUtilities.GetFileInfo(index - CurrentListing.Directories.Length, CurrentListing);
                        currentFileInfo = fileInfo;
                        currentlyInFile = true;
                        var updatedEmbed = UpdateEmbedListing(fileInfo, embed);
                        await ListingMessage.ModifyAsync(embed: updatedEmbed.Build());
                    }

                    continue;
                }

                if (char.TryParse(message, out var result))
                {
                    var action = ToEnum<FileSystemUtilities.EDirectoryActions>(result.ToString().ToUpper());

                    if (!Enum.IsDefined(typeof(FileSystemUtilities.EDirectoryActions), action))
                    {
                        await CommandContext.RespondAsync("Cannot execute option if does not exist.");
                        continue;
                    }

                    if (await FileSystemUtilities.DirectoryAction(action, CurrentListing, CurrentDirectory))
                    {
                        VirtualDirectorySecurity.AddAction(
                            $"{CommandContext.Member}\n{CommandContext.Channel}",
                            $"Action: {action.ToString()}\nAffected directory: {CurrentDirectory}",
                            DateTime.UtcNow);

                        if (action == FileSystemUtilities.EDirectoryActions.DeleteFolder)
                        {
                            CurrentListing = FileSystemUtilities.NavigateBackFolder(CurrentDirectory);
                            var updatedEmbed = UpdateEmbedListing(embed);
                            await ListingMessage.ModifyAsync(embed: updatedEmbed.Build());
                        }
                        else
                        {
                            CurrentListing = FileSystemUtilities.NavigateCurrentFolder(CurrentDirectory);
                            var updatedEmbed = UpdateEmbedListing(embed);
                            await ListingMessage.ModifyAsync(embed: updatedEmbed.Build());
                        }
                    }
                    else
                    {
                        await CommandContext.RespondAsync("Error in refreshing directory");
                    }
                }
                else
                {
                    await CommandContext.RespondAsync("I don't know what you mean by " + message);
                }
            }
        }

        private T ToEnum<T>(string @string)
        {
            if (string.IsNullOrEmpty(@string)) throw new ArgumentException("Argument null or empty");
            if (@string.Length > 1) throw new ArgumentException("Argument length greater than one");
            return (T) Enum.ToObject(typeof(T), @string[0]);
        }

        private DiscordEmbedBuilder UpdateEmbedListing(DiscordEmbedBuilder embed)
        {
            embed.ClearFields();
            embed.Description = $"**Navigating {CurrentDirectory}\n\nType `{_settings.GoBackCommand[0]}` to go back a directory\nType {_settings.ExitCommand[0]} at anytime to quit.**\n\n";
            var id = 1;

            foreach (var directory in CurrentListing.Directories)
            {
                embed.Description += $"**{id}**) {directory.Name}\n";
                id++;
            }

            foreach (var file in CurrentListing.Files)
            {
                embed.Description += $"**{id}**) {file.Name}\n";
                id++;
            }

            if (embed.Description.Length > 1800)
            {
                embed.Description = "**Cannot view contents of this directory!**";
            }

            var actions = string.Empty;
            foreach (var action in Enum.GetNames(typeof(FileSystemUtilities.EDirectoryActions)))
            {
                var val = (FileSystemUtilities.EDirectoryActions) Enum.Parse(typeof(FileSystemUtilities.EDirectoryActions), action);
                var ch = (char) val;
                actions += $"{ch}) {action}\n";
            }

            embed.AddField("Actions", actions);

            return embed;
        }

        private DiscordEmbedBuilder UpdateEmbedListing(FileInfo file, DiscordEmbedBuilder embed)
        {
            embed.Description = $"Current File: **{file.Name}\n\nType `{_settings.GoBackCommand[0]}` to go back a directory\nType {_settings.ExitCommand[0]} at anytime to quit.**\n\n";

            var fileContent = FileSystemUtilities.GetFileContent(FileSystemUtilities.GetFile(file.Directory, file.Name));
            embed.Description += fileContent.Length > 1200 ? fileContent.Substring(0, 1200) + "\n__***Too large to view further...***__" : fileContent;

            var actions = string.Empty;
            var id = 1;
            foreach (var name in Enum.GetNames(typeof(FileSystemUtilities.EFileActions)))
            {
                actions += $"**{id}**) {name}\n";
                id++;
            }

            embed.ClearFields();
            embed.AddField("Actions", actions);

            return embed;
        }
    }
}