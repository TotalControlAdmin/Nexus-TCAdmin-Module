using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Nexus.SDK.Modules;
using TCAdminModule.Attributes;
using TCAdminModule.Objects.Emulators;
using TCAdminModule.ServiceMenu.Buttons;

namespace TCAdminModule.Commands.Client
{
    [Group("tca")]
    [Aliases("service")]
    [Cooldown(1, 5.0, CooldownBucketType.User)]
    public class TcAdminCommands : NexusCommandModule
    {
        [GroupCommand]
        public async Task MainCommand(CommandContext ctx)
        {
            var context = new CommandAttributes.RequireAuthentication();

            await context.ExecuteCheckAsync(ctx, false);

            await new TcaServiceMenu().MenuEmulation(ctx, context);
        }

        [Command("rcon")]
        public async Task RemoteConsoleCommand(CommandContext ctx, [RemainingText] string command)
        {
            var context = new CommandAttributes.RequireAuthentication();

            await context.ExecuteCheckAsync(ctx, false);

            var remoteConsole = new RemoteConsoleButton()
            {
                Authentication = context,
                CommandContext = ctx,
            };

            await remoteConsole.RconTask(command);
        }
    }
}