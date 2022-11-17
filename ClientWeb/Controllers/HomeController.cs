using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClientWeb.Models;
using GrainInterfaces;
using Orleans;

namespace ClientWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger) => _logger = logger;

    public async Task<IActionResult> Index([FromServices] IGrainFactory client)
    {
        //var stateValue = await client.GetGrain<IStateGrain>(0).Get();
        return View(new HomeIndexModel() {Value = 200, State = ""});
    }

    [HttpPost]
    public async Task<IActionResult> GetData(int value, [FromServices] IGrainFactory client)
    {
        var newValue   = await client.GetGrain<IUnitCalculator>(0).Calculate(value);

        return View("Index", new HomeIndexModel() {Value = value, Calculated = newValue});
    }

    [HttpPost]
    public async Task<IActionResult> SetData(string value, [FromServices] IGrainFactory client)
    {
        // var grain = client.GetGrain<IStateGrain>(0);
        // await grain.Set(value);

        return View("Index", new HomeIndexModel() {State = value});
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}