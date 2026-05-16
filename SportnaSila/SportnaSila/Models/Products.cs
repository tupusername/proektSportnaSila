using System.ComponentModel.DataAnnotations.Schema;

namespace SportnaSila.Models
{
    public class Products
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImgUrl { get; set; }
        [Column(TypeName ="decimal(10,2)")]
        public decimal Price { get; set; }
       
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        public int BrandId { get; set; }
        

        public Categories Category { get; set; }
        public Suppliers Supplier { get; set; }
        public Brands Brand { get; set; }
      public ICollection<Orders> Order {  get; set; }
    }
}
