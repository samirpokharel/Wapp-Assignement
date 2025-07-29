using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SimpleLMS.Models;

namespace SimpleLMS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SignInManager<IdentityUser> _signInManager;

    public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> signInManager)
    {
        _logger = logger;
        _signInManager = signInManager;
    }

    public IActionResult Index()
    {
        // If user is signed in, redirect to dashboard
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Index", "Dashboard");
        }
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
