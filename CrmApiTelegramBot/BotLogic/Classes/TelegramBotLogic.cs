
using CrmApiTelegramBot.BotLogic.Interfaces;
using CrmApiTelegramBot.Model;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Contracts;
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
    public class TelegramBotLogic : ITelegramBotLogic
    {
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();
        private static ITelegramBotClient botClient;
        //private static IHttpClientFactory _httpClientFactory;
        private GoogleService google;
        private static ILogger _logger;
        private static Dictionary<string, TripsFormGoogleSheets> testListTrips;

        //public TelegramBotLogic(IHttpClientFactory httpClientFactory, ILogger<TelegramBotLogic> logger)
        //{
        //    _httpClientFactory = httpClientFactory;
        //    _logger = logger ;
        //}


        public async void StartBot(IConfiguration config)
        {
            google = new();
            //ILogger logger = _loggerFactory.CreateLogger("StartBot");
            testListTrips = new();
            var token = ReadApiToken(config);
            botClient = new TelegramBotClient(token);
            var cancelationToken = cts.Token;
            var reciveOpt = new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
            };
            var bot = await botClient.GetMeAsync(cancelationToken);

            await botClient.ReceiveAsync(HandleUpdateAsync, HandleErrorAsync, reciveOpt, cancelationToken);

        }

        public static async Task SendMessage(ITelegramBotClient client, Chat chat, string message, ParseMode parseMode = ParseMode.Html)
        {
            //var buttons = new InlineKeyboardButton("Один");
            var inlineKeyboard = new InlineKeyboardMarkup()
                .AddButton("1.1", "2.2");

            try
            {

                await client.SendTextMessageAsync(
                    chatId: chat,
                    text: message,
                    parseMode: parseMode
                    //replyMarkup: new ReplyKeyboardRemove()
                );
            }
            catch (Exception e)
            {

            }
        }

        public static async Task SendMessageWithKeyboard(ITelegramBotClient client, Chat chat, string message, ParseMode parseMode = ParseMode.Html)
        {
            var inlineKeyboard = new InlineKeyboardMarkup()
                .AddButton("Закончил командировку и отправил документы", "writeConsole");
            try
            {
                await client.SendTextMessageAsync(
                    chatId: chat,
                    text: message,
                    parseMode: parseMode,
                    replyMarkup: inlineKeyboard
                );
            }
            catch (Exception e)
            {
                
            }
        }


        private async Task HandleUnknownAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.CallbackQuery.Message.Text.Split("\n");
            var rowContract = message[4].Split(":");
            var contract = rowContract[1].Trim();

            await ColoredSheetCells(contract);
            
            await Console.Out.WriteLineAsync("Colored!");

            if (update.CallbackQuery.Data == "writeConsole")
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, null);
                await Console.Out.WriteLineAsync($"Пользователь: {update.CallbackQuery.Message.Chat.Id} Закончил контакт: {contract} Менеджер: {testListTrips[contract].managerName}");
            }
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
                            await SendMessage(botClient, message.Chat,  $"Вам назначена коммандировка." +
                                                                        $"\nМенеджер: {e.managerName}" +
                                                                        $"\nОт кого едем: {e.company}" +
                                                                        $"\nДата выезда: {e.dateOfTrip}" +
                                                                        $"\nЗаказчик: {e.customer}" +
                                                                        $"\nАдрес заказчика: {e.address}" +
                                                                        $"\nПримечание: {e.caption}");
                    }
                    return;
                case ("/myId"):
                    await SendMessage(botClient, message.Chat, $"{message.Chat.Id}");
                    return;
                default:
                    await SendMessageWithKeyboard(botClient, message.Chat, $"Сам ты - {message.Text}") ;
                    return;
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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

        /// <summary>
        /// Отправка сообщений о новой командировке
        /// </summary>
        /// <param name="trip">Полученные сведения о команировке и чате в который нужно отправить</param>
        public async Task<HttpStatusCode> SendMessageOfNewTrip(SentTrip trip)
        {
            
            Chat chat = new Chat()
            {
                Id = trip.telegramId
            };
            await SendMessageWithKeyboard(botClient, chat, $"Вам назначена коммандировка." +
                                                            $"\nМенеджер: {trip.managerName}" +
                                                            $"\nОт кого едем: {trip.company}" +
                                                            $"\nДата выезда: {trip.dateOfTrip}" +
                                                            $"\nЗаказчик: {trip.customer}" +
                                                            $"\nАдрес заказчика: {trip.address}" +
                                                            $"\nПримечание: {trip.caption}");
            return HttpStatusCode.OK;
        }


        public async Task<HttpStatusCode> SendMessageFromGoogleSheets(TripsFormGoogleSheets trip)
        {
            testListTrips.Add(trip.contractNumber, trip);
            Chat chat = new Chat()
            {
                Id = trip.telegramId
            };
            await SendMessageWithKeyboard(botClient, chat, $"Вам назначена коммандировка." +
                                                            $"\nМенеджер: {trip.managerName}" +
                                                            $"\nОт кого едем: {trip.company}" +
                                                            $"\nДата выезда: {trip.dateOfTrip}" +
                                                            $"\nНомер контракта: {trip.contractNumber}" +
                                                            $"\nЗаказчик: {trip.customer}" +
                                                            $"\nАдрес заказчика: {trip.address}" +
                                                            $"\nПримечание: {trip.caption}");
            return HttpStatusCode.OK;
        }

        private async Task<bool> ColoredSheetCells(string contract)
        {
            const string spreadsheetId = "1OMnOhusZiKWaEy47Wg8FMBXpSyRrGzA0DhwDeh9smAY";

            Spreadsheet table = google.Service.Spreadsheets.Get(spreadsheetId).Execute();
            Sheet workSheet = table.Sheets.Where(s => s.Properties.Title == "ВЫЕЗДНЫЕ РАБОТЫ 2021").FirstOrDefault();
            int sheet_id = (int)workSheet.Properties.SheetId;

            var userEnteredFormat = new CellFormat()
            {
                BackgroundColor = new Google.Apis.Sheets.v4.Data.Color()
                {
                    Blue = 0,
                    Red = 0,
                    Green = 1,
                    Alpha = (float)0.1
                },
                TextFormat = new TextFormat()
                {
                    FontFamily = "Arial",
                    Bold = false
                }
            };

            BatchUpdateSpreadsheetRequest bussr = new BatchUpdateSpreadsheetRequest();

            //create the update request for cells from the first row
            var updateCellsRequest = new Request()
            {
                RepeatCell = new RepeatCellRequest()
                {
                    Range = new GridRange()
                    {
                        SheetId = sheet_id,
                        StartColumnIndex = 0,
                        StartRowIndex = Int32.Parse(testListTrips[contract].row),
                        EndColumnIndex = 26,
                        EndRowIndex = Int32.Parse(testListTrips[contract].row) + 1
                    },
                    Cell = new CellData()
                    {
                        UserEnteredFormat = userEnteredFormat
                    },
                    Fields = "UserEnteredFormat(BackgroundColor,TextFormat)"
                }
            };
            bussr.Requests = new List<Request>
            {
                updateCellsRequest
            };

            var bur = google.Service.Spreadsheets.BatchUpdate(bussr, spreadsheetId);
            bur.Execute();
            return true;
        }
    }
}
