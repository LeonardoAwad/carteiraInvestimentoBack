namespace CalculoInvestimento.Back.Models
{
    public class Investiments
    {
        public Guid Id {  get; set; }
        public int Type { get; set; }
        public decimal Value {  get; set; }
        public DateTime Date {  get; set; }
        public User? User { get; set; }
    }
}
