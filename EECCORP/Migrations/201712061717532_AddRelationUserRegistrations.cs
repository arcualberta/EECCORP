namespace EECCORP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRelationUserRegistrations : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Registrations", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Registrations", "UserId");
            AddForeignKey("dbo.Registrations", "UserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Registrations", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Registrations", new[] { "UserId" });
            AlterColumn("dbo.Registrations", "UserId", c => c.String());
        }
    }
}
