﻿using CrmApiTelegramBot.BotLogic.Interfaces;
using CrmApiTelegramBot.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrmApiTelegramBot.BotLogic.Classes
{
    public class StartBot : IStartBot
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private static ITelegramBotClient botClient;
        private IHttpClientFactory _httpClientFactory;
        static ILogger logger = Program.logFactory.CreateLogger<StartBot>();
        IConfiguration _config { get; }

        public StartBot(IConfiguration configuration) => _config = configuration;

        public StartBot(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        async void IStartBot.StartBot()
        {
            var token = ReadApiToken(_config);
            botClient = new TelegramBotClient(token);
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

        public async Task SendMessage(ITelegramBotClient client, Chat chat, string message, ParseMode parseMode = ParseMode.Html)
        {
            try
            {
                await client.SendTextMessageAsync(
                    chatId: chat,
                    text: message,
                    parseMode: parseMode
                );
            }
            catch (Exception e)
            {

            }
        }

        private async Task HandleUnknownAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //await SendMessage($"Telegram UpdateType \"{update.Type.ToString()}\" not handled", ParseMode.Html, cancellationToken);
        }

        public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            switch (message.Text)
            {
                case ("/trips"):
                    var trips = await GetMyTrips(message.Chat.Id);
                    List<string> messages = new List<string>();
                    foreach (var e in trips)
                    {
                            await SendMessage(botClient, message.Chat,  $"Менеджер: {e.managerName}" +
                                                                        $"\nОт кого едем: {e.company}" +
                                                                        $"\nДата выезда: {e.dateOfTrip}" +
                                                                        $"\nЗаказчик: {e.customer}" +
                                                                        $"\nАдрес заказчика: {e.address} ");
                    }
                    return;
                case ("/myId"):
                    await SendMessage(botClient, message.Chat, $"{message.Chat.Id}");
                    return;
                default:
                    await SendMessage(botClient, message.Chat, "Дарова ебать!");
                    return;
                
            }
            if (message != null)
            {
            }
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            var handler = update switch
            {
                { Message: { } message } => HandleMessageAsync(botClient, message, cancellationToken),
                _ => HandleUnknownAsync(botClient, update, cancellationToken)
            };

            await handler;

        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


        private static string ReadApiToken(IConfiguration appconfig)
        {
            string token = appconfig["token"];
            if (token != null)
            {
                logger.LogInformation("Token read success!");
                return token;
            }
            logger.LogInformation("Token don't read!!!");
            return null;
        }

        public void TelegramBotDisable()
        {
            if (botClient != null)
            {
                cts.Cancel();
            }
        }

        public async Task<List<Trips>> GetMyTrips(long telegramId)
        {
            //var httpClient = _httpClientFactory.CreateClient("GetMyTrips");
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("http://localhost:5000/getmytrips/" + telegramId);
            if (response.IsSuccessStatusCode)
            {
                var jsonresult = await response.Content.ReadAsStringAsync();
                var trips = JsonSerializer.Deserialize<List<Trips>>(jsonresult);
                return trips;
            }
            return null;
        }
    }
}
