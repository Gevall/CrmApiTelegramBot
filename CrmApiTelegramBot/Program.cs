using CrmApiTelegramBot.BotLogic.Classes;
using CrmApiTelegramBot.BotLogic.Interfaces;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Telegram.Bot;

namespace CrmApiTelegramBot
{
    public class Program
    {
        public static readonly ILoggerFactory logFactory =  LoggerFactory.Create(conf => conf.AddConsole());
        private static IHttpClientFactory _httpClientFactory;

        public Program(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;
        public static void Main(string[] args)
        {

            
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Configuration.AddJsonFile("token.json");
            IStartBot botClient = new StartBot(builder.Configuration);
            botClient.StartBot();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpClient();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGet("/sendtrip", () =>
            {

            });
            //TestHttpCLientFactory();
            app.Run();

        }

        private static  void TestHttpCLientFactory()
        {
            var httpClient = _httpClientFactory.CreateClient("Test");
        }
    }
}