namespace CrmApiTelegramBot.Model
{
    public class TripsFormGoogleSheets
    {
        public int telegramId { get; set; }
        public string? managerName { get; set; }
        public string? dateOfTrip { get; set; }
        public string? company { get; set; }
        public string? deadLine { get; set; }
        public string? contractNumber {  get; set; }
        public string? customer { get; set; }
        public string? address { get; set; }
        public string? caption { get; set; }
        public string? row { get; set; }
    }
}
