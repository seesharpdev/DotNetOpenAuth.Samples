namespace OAuthClient
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Web;

    using DotNetOpenAuth.ApplicationBlock;
    using DotNetOpenAuth.OAuth2;

    public partial class WindowsLive : System.Web.UI.Page
    {
        private static readonly WindowsLiveClient Client = new WindowsLiveClient
            {
                ClientIdentifier = ConfigurationManager.AppSettings["windowsLiveAppID"],
                ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigurationManager.AppSettings["WindowsLiveAppSecret"]),
            };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.Equals("localhost", Request.Headers["Host"].Split(':')[0], StringComparison.OrdinalIgnoreCase))
            {
                localhostDoesNotWorkPanel.Visible = true;
                var builder = new UriBuilder(publicLink.NavigateUrl);
                builder.Port = Request.Url.Port;
                publicLink.NavigateUrl = builder.Uri.AbsoluteUri;
                publicLink.Text = builder.Uri.AbsoluteUri;
            }
            else
            {
                IAuthorizationState authorization = Client.ProcessUserAuthorization();
                if (authorization == null)
                {
                    // Kick off authorization request
                    Client.RequestUserAuthorization(new[] { WindowsLiveClient.Scopes.Basic }); // this scope isn't even required just to log in
                }
                else
                {
                    var request =
                        WebRequest.Create("https://apis.live.net/v5.0/me?access_token=" + Uri.EscapeDataString(authorization.AccessToken));
                    using (var response = request.GetResponse())
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            var graph = WindowsLiveGraph.Deserialize(responseStream);
                            nameLabel.Text = HttpUtility.HtmlEncode(graph.Name);
                        }
                    }
                }
            }
        }
    }
}
