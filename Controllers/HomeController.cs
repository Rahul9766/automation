using Microsoft.AspNetCore.Mvc;
using PowerShellWebApp.Services;

namespace PowerShellWebApp.Controllers;

public class HomeController : Controller
{
    private readonly PowerShellService _powerShellService;

    public HomeController(PowerShellService powerShellService)
    {
        _powerShellService = powerShellService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetServiceStatus()
    {
        try
        {
            var services = await _powerShellService.GetServiceStatusAsync();
            return Json(services);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }
} 