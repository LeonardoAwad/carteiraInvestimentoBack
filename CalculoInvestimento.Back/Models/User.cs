namespace CalculoInvestimento.Back.Models
{
    public record User
    {
        public Guid? Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }

        public List<Investiments> Investiments { get; set; }
    }
}