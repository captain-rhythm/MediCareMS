using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin")]
public class SettingsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
