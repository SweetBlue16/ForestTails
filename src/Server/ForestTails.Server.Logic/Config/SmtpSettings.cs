namespace ForestTails.Server.Logic.Config
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderName { get; set; } = "Forest Tails Game";
        public string SenderEmail { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }
}
