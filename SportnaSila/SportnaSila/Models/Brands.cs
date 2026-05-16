namespace SportnaSila.Models
{
    public class Brands
    {
        public int Id { get; set; }
        public string BrandName { get; set; }
        public string Description { get; set; }
        public DateTime DateReceived { get; set; }

        public ICollection<Products> Products { get; set; }
    }
}
