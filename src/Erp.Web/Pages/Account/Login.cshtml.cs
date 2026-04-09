using System.Security.Claims;
using Erp.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Erp.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly InMemoryUserStore _users;

    public LoginModel(InMemoryUserStore users) => _users = users;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public string Email { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            Error = "Please enter your email and password.";
            return Page();
        }

        var user = _users.Validate(Email, Password);
        if (user is null)
        {
            Error = "Invalid email or password.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.GivenName, user.FirstName),
        };
        var identity = new ClaimsIdentity(claims, "DynamixCookie");
        await HttpContext.SignInAsync("DynamixCookie", new ClaimsPrincipal(identity));

        // Prevent open redirect attacks
        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return Redirect(ReturnUrl);

        return Redirect("/app/dashboard");
    }
}
