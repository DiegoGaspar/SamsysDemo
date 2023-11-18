namespace SamsysDemo.Infrastructure.Models.Client
{
    public class UpdateClientDTO
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ConcurrencyToken { get; set; } = string.Empty;
        public DateTime? DataNascimento { get; set; }
    }
}
