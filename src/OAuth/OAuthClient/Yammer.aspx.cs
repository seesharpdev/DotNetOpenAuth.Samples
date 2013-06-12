namespace OAuthClient
{
    using System;
    using System.Configuration;
    using System.Web;

    using DotNetOpenAuth.ApplicationBlock;
    using DotNetOpenAuth.OAuth;

    public partial class Yammer : System.Web.UI.Page
    {
        private string RequestToken
        {
            get { return (string)ViewState["YammerRequestToken"]; }
            set { ViewState["YammerRequestToken"] = value; }
        }

        private string AccessToken
        {
            get { return (string)Session["YammerAccessToken"]; }
            set { Session["YammerAccessToken"] = value; }
        }

        private InMemoryTokenManager TokenManager
        {
            get
            {
                var tokenManager = (InMemoryTokenManager)Application["YammerTokenManager"];
                if (tokenManager == null)
                {
                    string consumerKey = ConfigurationManager.AppSettings["YammerConsumerKey"];
                    string consumerSecret = ConfigurationManager.AppSettings["YammerConsumerSecret"];
                    if (!string.IsNullOrEmpty(consumerKey))
                    {
                        tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
                        Application["YammerTokenManager"] = tokenManager;
                    }
                }

                return tokenManager;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (TokenManager != null)
            {
                MultiView1.SetActiveView(BeginAuthorizationView);
            }
        }

        protected void getYammerMessages_Click(object sender, EventArgs e)
        {
            var yammer = new WebConsumer(YammerConsumer.ServiceDescription, TokenManager);
        }

        protected void obtainAuthorizationButton_Click(object sender, EventArgs e)
        {
            var yammer = YammerConsumer.CreateConsumer(TokenManager);
            string requestToken;
            Uri popupWindowLocation = YammerConsumer.PrepareRequestAuthorization(yammer, out requestToken);
            RequestToken = requestToken;
            string javascript = "window.open('" + popupWindowLocation.AbsoluteUri + "');";
            Page.ClientScript.RegisterStartupScript(GetType(), "YammerPopup", javascript, true);
            MultiView1.SetActiveView(CompleteAuthorizationView);
        }

        protected void finishAuthorizationButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            var yammer = YammerConsumer.CreateConsumer(TokenManager);
            var authorizationResponse = YammerConsumer.CompleteAuthorization(yammer, RequestToken, yammerUserCode.Text);
            if (authorizationResponse != null)
            {
                accessTokenLabel.Text = HttpUtility.HtmlEncode(authorizationResponse.AccessToken);
                MultiView1.SetActiveView(AuthorizationCompleteView);
            }
            else
            {
                MultiView1.SetActiveView(BeginAuthorizationView);
                authorizationErrorLabel.Visible = true;
            }
        }
    }
}