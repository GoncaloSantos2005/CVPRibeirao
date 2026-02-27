using Microsoft.AspNetCore.Mvc;
using SistemaPDI.Web.Services;

namespace SistemaPDI.Web.Controllers
{
    public class DashboardController : BaseController
    {
        public DashboardController(IPdiApiService pdiService) : base(pdiService) { }

        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard";
            return View();
        }
    }
}
