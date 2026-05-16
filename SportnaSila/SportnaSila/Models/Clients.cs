using Microsoft.AspNetCore.Identity;

namespace SportnaSila.Models
{
    public class Clients:IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Orders> Orders { get; set; }
    }
}
