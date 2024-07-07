using CrmApiTelegramBot.BotLogic.Classes;
using CrmApiTelegramBot.Model;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Telegram.Bot;

namespace CrmApiTelegramBot
{
    public class Program
    {
        //public static readonly ILoggerFactory logFactory =  LoggerFactory.Create(conf => conf.AddConsole());

        public static void Main(string[] args)
        {

            
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Configuration.AddJsonFile("token.json");

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddLogging(conf => conf.AddConsole()
                                                    .SetMinimumLevel(LogLevel.Information));
            //builder.Logging.AddConsole()
            //               .SetMinimumLevel(LogLevel.Information);

            builder.Services.AddHttpClient();
            builder.Services.AddTransient<StartBot>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            //IStartBot botClient = new StartBot();
            //var token = botClient.ReadApiToken(builder.Configuration);
            app.MapPost("/sendtrip", async (SentTrip trip) =>
            {
               var httpCode =  await StartBot.SendMessageOfNewTrip(trip);
            });

            StartBot.startBot(builder.Configuration);
            app.Run();
           
        }
    }
}