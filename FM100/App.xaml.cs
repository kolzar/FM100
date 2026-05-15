using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using FM100.Data.DependencyInjection;
using FM100.Data.Repositories;
using FM100.Data.Seeders;
using FM100.Core.DependencyInjection;

namespace FM100
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        public App()
        {
            InitializeServices();
        }

        private void InitializeServices()
        {
            var services = new ServiceCollection();

            // Register data layer
            services.AddDataServices();

            // Register core services
            services.AddPerformanceServices();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Seed database with fake players if empty
            if (_serviceProvider != null)
            {
                var playerRepository = _serviceProvider.GetRequiredService<IFootballPlayerRepository>();
                var seeder = new FootballPlayerSeeder(playerRepository);
                await seeder.SeedIfEmptyAsync(23);
            }
        }

        public ServiceProvider GetServiceProvider()
        {
            return _serviceProvider ?? throw new InvalidOperationException("Service provider not initialized");
        }
    }

}
