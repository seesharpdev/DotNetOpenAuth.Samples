namespace OAuthConsumer
{
    using System;
    using System.Configuration;

    using DotNetOpenAuth.ApplicationBlock;
    using DotNetOpenAuth.Messaging;
    using DotNetOpenAuth.OAuth;

    public partial class GoogleApps2Legged : System.Web.UI.Page
    {
        private InMemoryTokenManager TokenManager
        {
            get
            {
                var tokenManager = (InMemoryTokenManager)Application["GoogleTokenManager"];
                if (tokenManager == null)
                {
                    string consumerKey = ConfigurationManager.AppSettings["googleConsumerKey"];
                    string consumerSecret = ConfigurationManager.AppSettings["googleConsumerSecret"];
                    if (!string.IsNullOrEmpty(consumerKey))
                    {
                        tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
                        Application["GoogleTokenManager"] = tokenManager;
                    }
                }

                return tokenManager;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var google = new WebConsumer(GoogleConsumer.ServiceDescription, TokenManager);
            string accessToken = google.RequestNewClientAccount();
            ////string tokenSecret = google.TokenManager.GetTokenSecret(accessToken);
            MessageReceivingEndpoint ep = null; // set up your authorized call here.
            google.PrepareAuthorizedRequestAndSend(ep, accessToken);
        }
    }
}