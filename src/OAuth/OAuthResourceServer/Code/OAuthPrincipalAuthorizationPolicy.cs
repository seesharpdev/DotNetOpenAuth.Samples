namespace OAuthResourceServer
{
    using System;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Principal;

    public class OAuthPrincipalAuthorizationPolicy : IAuthorizationPolicy
    {
        private readonly Guid _uniqueId = Guid.NewGuid();

        private readonly IPrincipal _principal;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthPrincipalAuthorizationPolicy"/> class.
        /// </summary>
        /// <param name="principal">The principal.</param>
        public OAuthPrincipalAuthorizationPolicy(IPrincipal principal)
        {
            _principal = principal;
        }

        #region IAuthorizationComponent Members

        /// <summary>
        /// Gets a unique ID for this instance.
        /// </summary>
        public string Id
        {
            get { return _uniqueId.ToString(); }
        }

        #endregion

        #region IAuthorizationPolicy Members

        public ClaimSet Issuer
        {
            get { return ClaimSet.System; }
        }

        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            evaluationContext.AddClaimSet(this, new DefaultClaimSet(Claim.CreateNameClaim(_principal.Identity.Name)));
            evaluationContext.Properties["Principal"] = _principal;

            return true;
        }

        #endregion
    }
}