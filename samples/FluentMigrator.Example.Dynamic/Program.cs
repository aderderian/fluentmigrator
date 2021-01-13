using System;
using System.IO;
using FFluentMigrator.Example.Dynamic.Migrations;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Versioning;
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
            
            Console.WriteLine("---Hello and welcome to Dynamic Migrator!---");
            Console.WriteLine("Dynamic Migrator Sample allows for dyanmic migrations to be executed based on user input or from a manifest file. Both can be built dynamically and executed at runtime.");
            Console.WriteLine("Press any key to test migrations from a json file...");

            Console.ReadLine();

            Console.WriteLine("Reading json file...");
            var manifestTxt = File.ReadAllText(Directory.GetCurrentDirectory() + "\\App_Data\\manifest.json");
            JObject migrations = JObject.Parse(manifestTxt);
            Console.WriteLine("File read and migrations stored as JObject.");

            JToken migration = migrations["migrations"][0];

            long version = long.Parse(migration["version"].ToString());
            string description = migration["description"].ToString();

            //this sample handles the instance that you would be loading a single dynamic migration, you could of course load multiple and version in json.

            TestMigrations testMigration = new TestMigrations() { Version = version, Description = description };

            Console.WriteLine("Creating up and down delegates for migration...");

            //delegate for up method
            testMigration.UpMethod = delegate () 
            {
                foreach (var table in migration["tables"].Children())
                {
                    //add table
                    string tableName = table["name"].ToString();
                    var columns = table["columns"];

                    //FM requires first column to be added when creating a table
                    var firstColumn = columns[0];
                    string first_columnName = firstColumn["name"].ToString();
                    string first_columnType = firstColumn["type"].ToString();
                    bool first_isPrimaryKey = Convert.ToBoolean(firstColumn["isPrimaryKey"]);
                    bool first_isIdentity = Convert.ToBoolean(firstColumn["isIdentity"]);
                    bool first_isNullable = Convert.ToBoolean(firstColumn["isNullable"]);

                    //some shortcuts here and this really needs to be done better
                    switch (first_columnType)
                    {
                        case "Int32":
                            if(first_isPrimaryKey && first_isIdentity)
                                testMigration.Create.Table(tableName).WithColumn(first_columnName).AsInt32().PrimaryKey().Identity();
                            else if (first_isPrimaryKey && !first_isIdentity)
                                testMigration.Create.Table(tableName).WithColumn(first_columnName).AsInt32().PrimaryKey();
                            else if (!first_isPrimaryKey && first_isIdentity)
                                testMigration.Create.Table(tableName).WithColumn(first_columnName).AsInt32().Identity();
                            break;
                        case "String":
                            testMigration.Create.Table(tableName).WithColumn(first_columnName).AsAnsiString();
                            break;
                        case "DateTime":
                            testMigration.Create.Table(tableName).WithColumn(first_columnName).AsDateTime();
                            break;
                    }

                    //add subsequent columns to table, NOTE : I wish there was a more dynamic way to aggregate statements together or pass type as a [""] value, but for now this is the best we can do
                    int index = 0;
                    foreach (var column in columns)
                    {
                        //skip first column, we already added it
                        if (index == 0)
                        {
                            index++;
                            continue;
                        }

                        string columnName = column["name"].ToString();
                        string columnType = column["type"].ToString();
                        bool isPrimaryKey = Convert.ToBoolean(column["isPrimaryKey"]);
                        bool isIdentity = Convert.ToBoolean(column["isIdentity"]);
                        bool isNullable = Convert.ToBoolean(column["isNullable"]);

                        switch (columnType)
                        {
                            case "Int32":
                                testMigration.Alter.Table(tableName).AddColumn(columnName).AsInt32();
                                break;
                            case "String":
                                testMigration.Alter.Table(tableName).AddColumn(columnName).AsAnsiString();
                                break;
                            case "DateTime":
                                testMigration.Alter.Table(tableName).AddColumn(columnName).AsDateTime();
                                break;
                        }

                        index++;
                    }
                }
            };

            //delegate for down method
            testMigration.DownMethod = delegate ()
            {
                //remove table
                foreach (var table in migration["tables"].Children())
                {
                    testMigration.Delete.Table(table["name"].ToString());
                }
            };

            IVersionInfo versionInfo = null;

            using (var scope = _serviceProvider.CreateScope())
            {
                var runner = _serviceProvider.GetRequiredService<IMigrationRunner>();
                versionInfo = runner.GetCurrentVersionInfo();
            }

            //Pro Tip - Use the current version info to check your migrations and/or to increment your migration version number if you're not storing it on your own as well.
            if(versionInfo != null)
            {
                Console.WriteLine("Current Version Info : " + versionInfo.Latest().ToString());
                Console.WriteLine();
            }

            if (!versionInfo.HasAppliedMigration(version))
            {
                Console.WriteLine("Press [y/Y] to apply migrations...");
                var result = Console.ReadKey();

                if (!string.IsNullOrEmpty(result.KeyChar.ToString()) && (result.KeyChar.ToString() == "y" || result.KeyChar.ToString() == "Y"))
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var runner = _serviceProvider.GetRequiredService<IMigrationRunner>();
                            runner.GetCurrentVersionInfo();
                            // Execute the migrations
                            runner.DynamicMigrateUp(testMigration, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing migrations : " + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Migration not applied.");
                }
            }
            else
            {
                Console.WriteLine("Migration has already been applied, skipping this migration.");
            }

            Console.WriteLine("---Process Complete.---");
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
    }
}
