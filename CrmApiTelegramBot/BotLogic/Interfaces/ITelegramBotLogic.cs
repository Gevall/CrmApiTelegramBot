using CrmApiTelegramBot.Model;
using System.Net;

namespace CrmApiTelegramBot.BotLogic.Interfaces
{
    public interface ITelegramBotLogic
    {
        public void StartBot(IConfiguration config);

        public Task<List<Trips>> GetMyTrips(long telegramId);

        public Task<HttpStatusCode> SendMessageOfNewTrip(SentTrip trip);
    }
}
