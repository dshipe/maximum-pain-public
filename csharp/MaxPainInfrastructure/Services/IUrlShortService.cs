namespace MaxPainInfrastructure.Services
{
    public interface IUrlShortService
    {
        Task<string> Bitly(string url);
        Task<string> Google(string url);
    }
}