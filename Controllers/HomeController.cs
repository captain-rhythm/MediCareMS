using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MediCareMS.Models;

namespace MediCareMS.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Root route redirects to Login (or Dashboard if already authenticated)
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Patient"))
                return RedirectToAction("Dashboard", "User");
            return RedirectToAction("Dashboard", "Admin");
        }
        return RedirectToAction("Login", "Auth");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
