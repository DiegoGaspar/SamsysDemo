namespace SamsysDemo.Infrastructure.Models.Client
{
    public class CreateClientDTO
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
    }
}
