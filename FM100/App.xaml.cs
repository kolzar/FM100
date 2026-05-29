using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using FM100.Data.DependencyInjection;
using FM100.Data.Repositories;
using FM100.Data.Seeders;
using FM100.Core.DependencyInjection;
using FM100.Core.Logging;
using FM100.Core.Management.Implementation;
using FM100.Core.Repositories;

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

            // Register logging services first
            services.AddLoggingServices();

            // Register data layer
            services.AddDataServices();

            // Register core services
            services.AddPerformanceServices();

            // Register game management services
            services.AddGameManagementServices();

            _serviceProvider = services.BuildServiceProvider();

            // Log application startup
            Logger.Information("Application", "FM100 services initialized successfully");
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Logger.Information("Application", "FM100 application starting...");

            // Seed database with fake players and clubs if empty
            if (_serviceProvider != null)
            {
                try
                {
                    var playerRepository = _serviceProvider.GetRequiredService<IFootballPlayerRepository>();
                    var playerSeeder = new FootballPlayerSeeder(playerRepository);
                    await playerSeeder.SeedIfEmptyAsync(23);
                    Logger.Information("Application", "Players seeded successfully");

                    var clubRepository = _serviceProvider.GetRequiredService<IClubRepository>();
                    var clubGenerator = _serviceProvider.GetRequiredService<ClubGenerator>();
                    var clubSeeder = new ClubSeeder(clubRepository, clubGenerator);
                    await clubSeeder.SeedIfEmptyAsync();
                    Logger.Information("Application", "Clubs seeded successfully");
                }
                catch (Exception ex)
                {
                    Logger.Error("Application", "Error seeding database", ex);
                }
            }

            Logger.Information("Application", "FM100 application started");
        }

        public ServiceProvider GetServiceProvider()
        {
            return _serviceProvider ?? throw new InvalidOperationException("Service provider not initialized");
        }
    }

}
