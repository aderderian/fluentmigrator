using System;

namespace FluentMigrator.Tests.Helpers
{
    public class DynamicMigrationFactory : DynamicMigration
    {
        public string ParentTableName { get; set; }
        public string ChildTableName { get; set; }

        public DynamicMigrationFactory(string parentTable, string childParent)
        {
            UpMethod = BuildUpMethod;
            DownMethod = BuildDownMethod;
            Version = 1;
            Description = "Testing Dynamic Integration";
            ParentTableName = parentTable;
            ChildTableName = childParent;
        }

        public void BuildUpMethod()
        {
            Create.Table(ParentTableName)
                  .WithColumn("KeyColumn").AsInt32().NotNullable().PrimaryKey()
                  .WithColumn("Column1").AsInt32().NotNullable()
                  .WithColumn("Column2").AsInt32().NotNullable();

            Create.Table(ChildTableName)
                  .WithColumn("KeyColumn").AsInt32().NotNullable().PrimaryKey()
                  .WithColumn("Column1").AsInt32().NotNullable()
                  .WithColumn("FkColumn").AsInt32()
                    .ForeignKey(ParentTableName, "KeyColumn");
        }

        public void BuildDownMethod()
        {
            this.Execute.Sql($"DROP TABLE {ChildTableName};");
            this.Execute.Sql($"DROP TABLE {ParentTableName};");
            this.Execute.Sql($"DROP TABLE VersionInfo;");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        public override void Up()
        {
            throw new NotImplementedException();
        }
    }
}
