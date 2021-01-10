using System;
using System.IO;

using FFluentMigrator.Example.Dynamic.Migrations;
using FluentMigrator;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json.Linq;

namespace FluentMigrator.Example.Dynamic
{
    class Program
    {
        static IServiceProvider _serviceProvider;

        static void Main(string[] args)
        {
            _serviceProvider = CreateServices();
            

            Console.WriteLine("Hello and welcome to dynamic migrator!");
            Console.ReadLine();

            var manifestTxt = File.ReadAllText(Directory.GetCurrentDirectory() + "\\App_Data\\manifest.json");
            JObject manifest = JObject.Parse(manifestTxt);

            TestMigrations testMigration = new TestMigrations() { Version = 1, Description = "Initial Migration"};

            testMigration.UpMethod = delegate () 
            {
              foreach (var table in manifest["tables"].Children())
                {
                    testMigration.Create.Table(table["name"].ToString()).WithColumn("Id").AsInt64().PrimaryKey().Identity().WithColumn("Text").AsString();
                }
            };

            testMigration.DownMethod = delegate ()
            {
                foreach (var table in manifest["tables"].Children())
                {
                    testMigration.Delete.Table(table["name"].ToString());
                }
            };

            ApplyMigration(testMigration);
        }

        /// <summary>
        /// Configure the dependency injection services
        /// </summary>
        private static IServiceProvider CreateServices()
        {
            return new ServiceCollection()
                // Add common FluentMigrator services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    // Add SQLite support to FluentMigrator
                    .AddSQLite()
                    // Set the connection string
                    .WithGlobalConnectionString("Data Source=test.db")
                    // Define the assembly containing the migrations
                    .WithMigrationsIn(typeof(TestMigrations).Assembly))
                // Enable logging to console in the FluentMigrator way
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                // Build the service provider
                .BuildServiceProvider(false);
        }

        private static void ApplyMigration(TestMigrations migration)
        {

            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using (var scope = _serviceProvider.CreateScope())
            {
                var runner = _serviceProvider.GetRequiredService<IMigrationRunner>();

                // Execute the migrations
                runner.DynamicMigrateUp(migration, true);
            }

        }
    }
}
