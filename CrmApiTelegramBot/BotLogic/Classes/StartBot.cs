using CrmApiTelegramBot.BotLogic.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CrmApiTelegramBot.BotLogic.Classes
{
    public class StartBot : IStartBot
    {
        private readonly ITelegramBotClient botClient = new TelegramBotClient(ReadApiToken());
        ILogger logger = Program.logFactory.CreateLogger<StartBot>();
        async void IStartBot.StartBot()
        {
            var cts = new CancellationTokenSource();
            var cancelationToken = cts.Token;
            var reciveOpt = new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
                ThrowPendingUpdates = true 
            };

            var bot = await botClient.GetMeAsync(cancelationToken);
            logger.LogInformation("Start receiving updates for {BotName}", bot.Username ?? "Integration Trip Bot");
            await botClient.ReceiveAsync(HandleUpdateAsync, HandleErrorAsync, reciveOpt, cancelationToken);
        }

        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {

        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            //await instanceSem.WaitAsync(ct);

            //var handler = update switch
            //{
            //    { Message: { } message } => HandleMessageAsync(botClient, message, cancellationToken),
            //    _ => HandleUnknownAsync(botClient, update, cancellationToken)
            //};

            //await handler;

            //instanceSem.Release();
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        private static string ReadApiToken()
        {
            string token = null;
            System.IO.File.WriteAllTextAsync("token.txt", token);
            return token;
        }
    }
}
