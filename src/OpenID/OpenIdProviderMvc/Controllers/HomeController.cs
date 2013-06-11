namespace OpenIdProviderMvc.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    [HandleError, Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.AcceptTypes.Contains("application/xrds+xml"))
            {
                ViewData["OPIdentifier"] = true;

                return View("Xrds");
            }

            return View();
        }

        public ActionResult Xrds()
        {
            ViewData["OPIdentifier"] = true;

            return View();
        }
    }
}
