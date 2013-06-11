namespace OpenIdWebRingSsoRelyingParty
{
    using System;

    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Array.IndexOf(Request.AcceptTypes, "application/xrds+xml") >= 0)
            {
                Server.Transfer("xrds.aspx");
            }
            else if (!User.Identity.IsAuthenticated)
            {
                Response.Redirect("Login.aspx");
            }
        }
    }
}
