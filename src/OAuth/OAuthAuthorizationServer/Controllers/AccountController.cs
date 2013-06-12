namespace OAuthAuthorizationServer.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;

    using DotNetOpenAuth.Messaging;
    using DotNetOpenAuth.OpenId;
    using DotNetOpenAuth.OpenId.RelyingParty;

    using OAuthAuthorizationServer.Code;
    using OAuthAuthorizationServer.Models;

    [HandleError]
    public class AccountController : Controller
    {
        // **************************************
        // URL: /Account/LogOn
        // **************************************
        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var relyingParty = new OpenIdRelyingParty();
                var request = relyingParty.CreateRequest(model.UserSuppliedIdentifier, Realm.AutoDetect, new Uri(Request.Url, Url.Action("Authenticate")));
                if (request != null)
                {
                    if (returnUrl != null)
                    {
                        request.AddCallbackArguments("returnUrl", returnUrl);
                    }

                    return request.RedirectingResponse.AsActionResult();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "The identifier you supplied is not recognized as a valid OpenID Identifier.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult Authenticate(string returnUrl)
        {
            var relyingParty = new OpenIdRelyingParty();
            var response = relyingParty.GetResponse();
            if (response != null)
            {
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        // Make sure we have a user account for this guy.
                        string identifier = response.ClaimedIdentifier; // convert to string so LinqToSQL expression parsing works.
                        if (MvcApplication.DataContext.Users.FirstOrDefault(u => u.OpenIDClaimedIdentifier == identifier) == null)
                        {
                            MvcApplication.DataContext.Users.InsertOnSubmit(new User
                                {
                                    OpenIDFriendlyIdentifier = response.FriendlyIdentifierForDisplay,
                                    OpenIDClaimedIdentifier = response.ClaimedIdentifier,
                                });
                        }

                        FormsAuthentication.SetAuthCookie(response.ClaimedIdentifier, false);
                        return Redirect(returnUrl ?? Url.Action("Index", "Home"));
                    default:
                        ModelState.AddModelError(string.Empty, "An error occurred during login.");
                        break;
                }
            }

            return View("LogOn");
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }
    }
}
