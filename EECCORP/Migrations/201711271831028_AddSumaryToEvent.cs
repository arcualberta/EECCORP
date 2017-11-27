namespace EECCORP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSumaryToEvent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "Summary", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "Summary");
        }
    }
}
