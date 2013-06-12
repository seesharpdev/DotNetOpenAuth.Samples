namespace OAuthResourceServer
{
    using System.ServiceModel;
    using System.ServiceModel.Web;

    [ServiceContract]
    public interface IDataApi
    {
        [OperationContract, WebGet(UriTemplate = "/age", ResponseFormat = WebMessageFormat.Json)]
        int? GetAge();

        [OperationContract, WebGet(UriTemplate = "/name", ResponseFormat = WebMessageFormat.Json)]
        string GetName();

        [OperationContract, WebGet(UriTemplate = "/favoritesites", ResponseFormat = WebMessageFormat.Json)]
        string[] GetFavoriteSites();
    }
}