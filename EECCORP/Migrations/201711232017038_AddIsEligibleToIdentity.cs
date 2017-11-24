namespace EECCORP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsEligibleToIdentity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsEligible", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "IsEligible");
        }
    }
}
