using CrmApiTelegramBot.BotLogic.Interfaces;
using System.Net;
using Telegram.Bot;

namespace CrmApiTelegramBot.BotLogic
{
    public class RunBot
    {
        ITelegramBotLogic _bot;
        public RunBot() { }
        public RunBot(ITelegramBotLogic bot)
        {
            _bot = bot;
        }

        public HttpStatusCode Run(IConfiguration config)
        {
            _bot.StartBot(config);
            return HttpStatusCode.OK;
        }
    }
}
