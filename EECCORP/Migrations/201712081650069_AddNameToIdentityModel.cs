namespace EECCORP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNameToIdentityModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ImageUrl", c => c.String());
            AddColumn("dbo.AspNetUsers", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Name");
            DropColumn("dbo.AspNetUsers", "ImageUrl");
        }
    }
}
