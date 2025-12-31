namespace Core.Dtos
{
    public class PaymentSettingsDto
    {
        public string Token { get; set; }
        public int SponsorId { get; set; }
        public string UserId { get; set; }
        public string ExternalPosId { get; set; }
        public string BaseUrl { get; set; }
        public string PaymentController { get; set; }

    }
}
