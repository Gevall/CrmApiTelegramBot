
using CrmApiTelegramBot.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CrmApiTelegramBot.BotLogic.Classes
{
    public class StartBot
    {
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();
        private static ITelegramBotClient botClient;
        private static IHttpClientFactory _httpClientFactory;
        //private static IConfiguration _configuration;
        //private static ILoggerFactory _loggerFactory { get; set; }
        private static ILogger _logger;

        public StartBot(IHttpClientFactory httpClientFactory, ILogger<StartBot> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger ;
        }


        public static async void startBot(IConfiguration config)
        {
            //ILogger logger = _loggerFactory.CreateLogger("StartBot");
            var token = ReadApiToken(config);
            botClient = new TelegramBotClient(token);
            var cancelationToken = cts.Token;
            var reciveOpt = new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
                ThrowPendingUpdates = true 
            };
            var bot = await botClient.GetMeAsync(cancelationToken);
            //_logger.LogInformation("Start receiving updates for {BotName}", bot.Username ?? "Integration Trip Bot");
            await botClient.ReceiveAsync(HandleUpdateAsync, HandleErrorAsync, reciveOpt, cancelationToken);
        }

        public static async Task SendMessage(ITelegramBotClient client, Chat chat, string message, ParseMode parseMode = ParseMode.Html)
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

        private static async Task HandleUnknownAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //await SendMessage($"Telegram UpdateType \"{update.Type.ToString()}\" not handled", ParseMode.Html, cancellationToken);
        }

        public static async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            var handler = update switch
            {
                { Message: { } message } => HandleMessageAsync(botClient, message, cancellationToken),
                _ => HandleUnknownAsync(botClient, update, cancellationToken)
            };

            await handler;

        }

        private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


        private static string ReadApiToken(IConfiguration config)
        {
            string token = config["token"];
            if (token != null)
            {
                //_logger.LogInformation("Token read success!");
                return token;
            }
            //_logger.LogInformation("Token don't read!!!");
            return null;
        }

        private static void TelegramBotDisable()
        {
            if (botClient != null)
            {
                cts.Cancel();
            }
        }

        private static async Task<List<Trips>> GetMyTrips(long telegramId)
        {
            var httpClient = _httpClientFactory.CreateClient("GetMyTrips");
            //var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("http://localhost:5000/getmytrips/" + telegramId);
            if (response.IsSuccessStatusCode)
            {
                var jsonresult = await response.Content.ReadAsStringAsync();
                var trips = JsonSerializer.Deserialize<List<Trips>>(jsonresult);
                return trips;
            }
            return null;
        }

        /// <summary>
        /// Отправка сообщений о новой командировке
        /// </summary>
        /// <param name="trip">Полученные сведения о команировке и чате в который нужно отправить</param>
        public static async Task<HttpStatusCode> SendMessageOfNewTrip(SentTrip trip)
        {
            Chat chat = new Chat()
            {
                Id = trip.telegramId
            };
            await SendMessage(botClient, chat, $"Менеджер: {trip.managerName}" +
                                                $"\nОт кого едем: {trip.company}" +
                                                $"\nДата выезда: {trip.dateOfTrip}" +
                                                $"\nЗаказчик: {trip.customer}" +
                                                $"\nАдрес заказчика: {trip.address} ");
            return HttpStatusCode.OK;
        }
    }
}
