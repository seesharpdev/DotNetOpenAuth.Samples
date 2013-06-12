namespace OAuthClient
{
    using System;
    using System.Web.UI;

    using DotNetOpenAuth.ApplicationBlock;

    public partial class SignInWithTwitter : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (TwitterConsumer.IsTwitterConsumerConfigured)
            {
                MultiView1.ActiveViewIndex = 1;

                if (!IsPostBack)
                {
                    string screenName;
                    int userId;
                    if (TwitterConsumer.TryFinishSignInWithTwitter(out screenName, out userId))
                    {
                        loggedInPanel.Visible = true;
                        loggedInName.Text = screenName;

                        // In a real app, the Twitter username would likely be used
                        // to log the user into the application.
                        ////FormsAuthentication.RedirectFromLoginPage(screenName, false);
                    }
                }
            }
        }

        protected void signInButton_Click(object sender, ImageClickEventArgs e)
        {
            TwitterConsumer.StartSignInWithTwitter(forceLoginCheckbox.Checked).Send();
        }
    }
}