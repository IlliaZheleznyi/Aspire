public class CustomHandlerFactory : IHttpMessageHandlerFactory
{
    public HttpMessageHandler CreateHandler(string name)
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    }
}