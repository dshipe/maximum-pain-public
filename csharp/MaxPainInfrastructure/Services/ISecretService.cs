namespace MaxPainInfrastructure.Services
{
    public interface ISecretService
    {
        public Task<string> GetValue(string key);
    }
}
