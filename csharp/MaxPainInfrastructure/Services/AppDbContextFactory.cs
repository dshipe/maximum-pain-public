using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MaxPainInfrastructure.Services
{
    public interface IAppDbContextFactory
    {
        DbContext Create();
    }

    public class AppDbContextFactory : IAppDbContextFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AppDbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public DbContext Create()
        {
            return _serviceProvider.GetService<DbContext>();
        }
    }
}
