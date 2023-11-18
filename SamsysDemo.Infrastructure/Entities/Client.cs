using System.ComponentModel.DataAnnotations;

namespace SamsysDemo.Infrastructure.Entities
{
    public class Client
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DataNascimento { get; set; }
        public bool IsActive { get; set; }

        [Timestamp]
        public byte[] ConcurrencyToken { get; private set; }
        public bool IsRemoved { get; set; } = false;
        public DateTime? DateRemoved { get; set; }
        public void Update(string name, string phoneNumber, DateTime? nascimento)
        {
            Name = name;
            PhoneNumber = phoneNumber;
            DataNascimento = (nascimento ?? this.DataNascimento);
        }
        public void SetStatus(bool status)
        {
            IsActive = status;
        }

    }





}
