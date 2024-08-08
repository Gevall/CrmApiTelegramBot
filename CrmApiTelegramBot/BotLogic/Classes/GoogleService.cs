using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;

namespace CrmApiTelegramBot.BotLogic.Classes
{
    public class GoogleService
    {
        public SheetsService Service { get; set; } // Инициализация класса для работы с таблицой
        const string APP_NAME = "testSheet";   // Имя таблицы (сменить на нужную страницу в таблице Google)
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

        public GoogleService() 
        {
            InitializeService();
        }

        private void InitializeService()
        {
            var credential = GetCredentialsFromFile();
            Service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential.Result,
                ApplicationName = APP_NAME
            });
        }

        /// <summary>
        /// Чтение json токена авторизации в API Google
        /// </summary>
        /// <returns></returns>
        private async Task<UserCredential> GetCredentialsFromFile()
        {
            //GoogleCredential credential;
            //using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            //{
            //    credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            //}

            UserCredential credential;
            using (var stream = new FileStream("client_secrets_webapp.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                                   GoogleClientSecrets.Load(stream).Secrets,
                                   new[] { SheetsService.Scope.Spreadsheets },
                                   "user", CancellationToken.None, new FileDataStore("sheets.MyListLibrary"));
            }
            return credential;
        }
    }


}
