namespace EECCORP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWeekToEvent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "Week", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "Week");
        }
    }
}
