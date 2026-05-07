using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin")]
public class UserManagementController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
