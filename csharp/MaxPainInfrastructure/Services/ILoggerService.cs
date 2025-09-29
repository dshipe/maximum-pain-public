namespace MaxPainInfrastructure.Services
{
    public interface ILoggerService
    {
        public Task<bool> InfoAsync(string subject, string body);
    }
}
