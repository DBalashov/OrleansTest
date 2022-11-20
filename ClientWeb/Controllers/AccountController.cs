using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ClientWeb.Models;
using GrainInterfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Orleans;

namespace ClientWeb.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger) => _logger = logger;

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string userName, [FromServices] IHttpContextAccessor contextAccessor)
    {
        var claimsIdentity = new ClaimsIdentity(new[]
                                                {
                                                    new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
                                                },
                                                CookieAuthenticationDefaults.AuthenticationScheme);


        await contextAccessor.HttpContext!.SignInAsync(new ClaimsPrincipal(claimsIdentity),
                                                       new AuthenticationProperties()
                                                       {
                                                           IsPersistent = false
                                                       });
        return RedirectToAction("Index", "Home");
    }
}