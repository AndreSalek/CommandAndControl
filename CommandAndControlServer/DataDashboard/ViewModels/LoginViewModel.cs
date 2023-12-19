using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DataDashboard.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "This field is required")]
        [StringLength(128, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 4)]
        public string UserName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        [StringLength(128, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 6)]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
