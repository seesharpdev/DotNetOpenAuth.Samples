﻿namespace OAuthClient
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetOpenAuth.OAuth2;

    using SampleResourceServer;

    public partial class SampleWcf2 : System.Web.UI.Page
    {
        /// <summary>
        /// The OAuth 2.0 client object to use to obtain authorization and authorize outgoing HTTP requests.
        /// </summary>
        private static readonly WebServerClient Client;

        /// <summary>
        /// The details about the sample OAuth-enabled WCF service that this sample client calls into.
        /// </summary>
        private static AuthorizationServerDescription authServerDescription = new AuthorizationServerDescription
            {
                TokenEndpoint = new Uri("http://localhost:50172/OAuth/Token"),
                AuthorizationEndpoint = new Uri("http://localhost:50172/OAuth/Authorize"),
            };

        /// <summary>
        /// Initializes static members of the <see cref="SampleWcf2"/> class.
        /// </summary>
        static SampleWcf2()
        {
            Client = new WebServerClient(authServerDescription, "sampleconsumer", "samplesecret");
        }

        /// <summary>
        /// Gets or sets the authorization details for the logged in user.
        /// </summary>
        /// <value>The authorization details.</value>
        /// <remarks>
        /// Because this is a sample, we simply store the authorization information in memory with the user session.
        /// A real web app should store at least the access and refresh tokens in this object in a database associated with the user.
        /// </remarks>
        private static IAuthorizationState Authorization
        {
            get { return (AuthorizationState)HttpContext.Current.Session["Authorization"]; }
            set { HttpContext.Current.Session["Authorization"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check to see if we're receiving a end user authorization response.
                var authorization = Client.ProcessUserAuthorization();
                if (authorization != null)
                {
                    // We are receiving an authorization response.  Store it and associate it with this user.
                    Authorization = authorization;
                    Response.Redirect(Request.Path); // get rid of the /?code= parameter
                }
            }

            if (Authorization != null)
            {
                // Indicate to the user that we have already obtained authorization on some of these.
                foreach (var li in scopeList.Items.OfType<ListItem>().Where(li => Authorization.Scope.Contains(li.Value)))
                {
                    li.Selected = true;
                }

                authorizationLabel.Text = "Authorization received!";
                if (Authorization.AccessTokenExpirationUtc.HasValue)
                {
                    TimeSpan timeLeft = Authorization.AccessTokenExpirationUtc.Value - DateTime.UtcNow;
                    authorizationLabel.Text += string.Format(CultureInfo.CurrentCulture, "  (access token expires in {0} minutes)", Math.Round(timeLeft.TotalMinutes, 1));
                }
            }

            getNameButton.Enabled = getAgeButton.Enabled = getFavoriteSites.Enabled = Authorization != null;
        }

        protected void getAuthorizationButton_Click(object sender, EventArgs e)
        {
            string[] scopes = (from item in scopeList.Items.OfType<ListItem>()
                               where item.Selected
                               select item.Value).ToArray();

            Client.RequestUserAuthorization(scopes);
        }

        protected void getNameButton_Click(object sender, EventArgs e)
        {
            try
            {
                nameLabel.Text = CallService(client => client.GetName());
            }
            catch (SecurityAccessDeniedException)
            {
                nameLabel.Text = "Access denied!";
            }
            catch (MessageSecurityException)
            {
                nameLabel.Text = "Access denied!";
            }
        }

        protected void getAgeButton_Click(object sender, EventArgs e)
        {
            try
            {
                int? age = CallService(client => client.GetAge());
                ageLabel.Text = age.HasValue ? age.Value.ToString(CultureInfo.CurrentCulture) : "not available";
            }
            catch (SecurityAccessDeniedException)
            {
                ageLabel.Text = "Access denied!";
            }
            catch (MessageSecurityException)
            {
                ageLabel.Text = "Access denied!";
            }
        }

        protected void getFavoriteSites_Click(object sender, EventArgs e)
        {
            try
            {
                string[] favoriteSites = CallService(client => client.GetFavoriteSites());
                favoriteSitesLabel.Text = string.Join(", ", favoriteSites);
            }
            catch (SecurityAccessDeniedException)
            {
                favoriteSitesLabel.Text = "Access denied!";
            }
            catch (MessageSecurityException)
            {
                favoriteSitesLabel.Text = "Access denied!";
            }
        }

        private T CallService<T>(Func<DataApiClient, T> predicate)
        {
            if (Authorization == null)
            {
                throw new InvalidOperationException("No access token!");
            }

            var wcfClient = new DataApiClient();

            // Refresh the access token if it expires and if its lifetime is too short to be of use.
            if (Authorization.AccessTokenExpirationUtc.HasValue)
            {
                if (Client.RefreshAuthorization(Authorization, TimeSpan.FromSeconds(30)))
                {
                    TimeSpan timeLeft = Authorization.AccessTokenExpirationUtc.Value - DateTime.UtcNow;
                    authorizationLabel.Text += string.Format(CultureInfo.CurrentCulture, " - just renewed for {0} more minutes)", Math.Round(timeLeft.TotalMinutes, 1));
                }
            }

            var httpRequest = (HttpWebRequest)WebRequest.Create(wcfClient.Endpoint.Address.Uri);
            ClientBase.AuthorizeRequest(httpRequest, Authorization.AccessToken);

            var httpDetails = new HttpRequestMessageProperty();
            httpDetails.Headers[HttpRequestHeader.Authorization] = httpRequest.Headers[HttpRequestHeader.Authorization];
            using (var scope = new OperationContextScope(wcfClient.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpDetails;
                return predicate(wcfClient);
            }
        }
    }
}