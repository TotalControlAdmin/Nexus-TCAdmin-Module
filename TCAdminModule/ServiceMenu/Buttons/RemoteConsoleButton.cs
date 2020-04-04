using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Interactivity;
using TCAdmin.GameHosting.SDK.Objects;
using TCAdmin.Interfaces.Server;
using TCAdminModule.Modules;
using TCAdminModule.Objects;

namespace TCAdminModule.ServiceMenu.Buttons
{
    public class RemoteConsoleButton : NexusServiceMenuModule
    {
        public RemoteConsoleButton()
        {
            this.Name = "Remote Console Button";
            var attribute =
                new ActionCommandAttribute("Remote Console", "Access the servers Remote Console", ":RemoteConsole:", new List<string> {"StartStop"},
                    false);
            this.ViewOrder = 7;
            this.ActionCommandAttribute = attribute;
        }

        public override async Task DoAction()
        {
            await base.DoAction();
            await CommandContext.TriggerTypingAsync();

            await RconTask();
        }

        public async Task RconTask()
        {
            var interactivity = CommandContext.Client.GetInteractivity();
            await CommandContext.RespondAsync("Please enter the RCON command to send to the server.");

            var command = await interactivity.WaitForMessageAsync(
                x => x.Author.Id == CommandContext.User.Id && x.Channel.Id == CommandContext.Channel.Id);
            await RconTask(command.Result.Content);
        }
        
        public async Task RconTask(string command)
        {
            await CommandContext.TriggerTypingAsync();

            if (Authentication.Service.Status.ServiceStatus != ServiceStatus.Running)
            {
                await CommandContext.RespondAsync("**RCON Commands cannot be sent when the service is offline.**");
                return;
            }

            try
            {
                var server = new Server(Authentication.Service.ServerId);

                var rconResponse = server.GameAdminService.ExecuteRConCommand(Authentication.Service.ServiceId,
                    Authentication.Service.Variables["RConPassword"].ToString(), command);

                await CommandContext.RespondAsync("RCON RESPONSE: " + rconResponse);

                if (string.IsNullOrWhiteSpace(rconResponse) || rconResponse.Contains("Invalid password!"))
                {
                    var interactivity = CommandContext.Client.GetInteractivity();
                    await CommandContext.RespondAsync(
                        "**Sending RCON commands doesn't seem to work! It's possible that I may have your RCON Password Incorrect, would you like to set it? [Y/N]**");

                    var choice = await interactivity.WaitForMessageAsync(x =>
                        x.Channel.Id == CommandContext.Channel.Id && x.Author.Id == CommandContext.User.Id);
                    switch (choice.Result.Content.ToLower())
                    {
                        case "y":
                        case "yes:":
                            await CommandContext.RespondAsync("**Please enter a RCON password for me to remember**");
                            var pass = await interactivity.WaitForMessageAsync(x =>
                                x.Channel.Id == CommandContext.Channel.Id && x.Author.Id == CommandContext.User.Id);

                            await pass.Result.DeleteAsync("Password Protection");
                            await CommandContext.RespondAsync("**Saving new password...**");
                            Authentication.Service.Variables["RConPassword"] = pass.Result.Content;
                            Authentication.Service.Configure();
                            Authentication.Service.Save();
                            await CommandContext.RespondAsync(
                                "**Password has been saved. If RCON continues to not function, ensure that you can send commands with Web Console.**");
                            return;
                        case "n":
                        case "no":
                            await CommandContext.RespondAsync("Aborting...");
                            return;
                    }

                    return;
                }

                rconResponse = rconResponse.Length > 1500 ? rconResponse.Substring(0, 1500) : rconResponse;
                await CommandContext.RespondAsync($"```yaml\n{rconResponse}\n```");
            }
            catch (Exception e)
            {
                await CommandContext.RespondAsync("ERROR:\n" + e.Message + "\n" + e.StackTrace);
            }
        }
    }
}