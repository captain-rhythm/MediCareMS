using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin,Receptionist")]
public class BillingController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
