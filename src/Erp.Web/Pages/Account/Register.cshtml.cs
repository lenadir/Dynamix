using System.Security.Claims;
using Erp.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Erp.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly InMemoryUserStore _users;

    public RegisterModel(InMemoryUserStore users) => _users = users;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public string FirstName { get; set; } = "";

    [BindProperty]
    public string LastName { get; set; } = "";

    [BindProperty]
    public string Email { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    [BindProperty]
    public string ConfirmPassword { get; set; } = "";

    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName)
            || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            Error = "All fields marked with * are required.";
            return Page();
        }

        if (Password != ConfirmPassword)
        {
            Error = "Passwords do not match.";
            return Page();
        }

        if (Password.Length < 8)
        {
            Error = "Password must be at least 8 characters.";
            return Page();
        }

        if (!_users.Register(Email, FirstName, LastName, Password))
        {
            Error = "An account with this email already exists.";
            return Page();
        }

        // Auto-login after registration
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, Email.ToLowerInvariant()),
            new(ClaimTypes.Name, $"{FirstName} {LastName}"),
            new(ClaimTypes.GivenName, FirstName),
        };
        var identity = new ClaimsIdentity(claims, "DynamixCookie");
        await HttpContext.SignInAsync("DynamixCookie", new ClaimsPrincipal(identity));

        // Prevent open redirect attacks
        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return Redirect(ReturnUrl);

        return Redirect("/app/dashboard");
    }
}
