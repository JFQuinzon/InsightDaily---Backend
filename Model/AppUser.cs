using Microsoft.AspNetCore.Identity;

namespace backend.Model
{
    public class AppUser : IdentityUser
    {
        public string? DisplayName {  get; set; }   
    }
}
