using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClientWeb.Models;
using GrainInterfaces;
using Microsoft.AspNetCore.Authorization;

namespace ClientWeb.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger) => _logger = logger;

    public IActionResult Index()
    {
        return View(new HomeIndexModel() {Value = 200, State = "", UserName = User.Identity!.Name!});
    }

    [HttpPost]
    public async Task<IActionResult> GetData(int value, [FromServices] IGrainFactory client)
    {
        var newValue = await client.GetGrain<IUnitCalculator>(value).Calculate(value);
        return View("Index", new HomeIndexModel() {Value = value, Calculated = newValue, UserName = User.Identity!.Name!});
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}