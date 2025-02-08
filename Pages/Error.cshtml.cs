using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASAssignment1.Pages
{
    public class ErrorModel : PageModel
    {
        [BindProperty]
        public int? StatusCode { get; set; }

        public string ErrorMessage { get; set; } = "An unexpected error occurred.";

        public void OnGet(int? code)
        {
            StatusCode = code;
            if (code.HasValue)
            {
                ErrorMessage = code switch
                {
                    404 => "Page not found.",
                    500 => "Internal server error.",
                    403 => "Access denied.",
                    _ => "An unexpected error occurred."
                };
            }
        }
    }
}
