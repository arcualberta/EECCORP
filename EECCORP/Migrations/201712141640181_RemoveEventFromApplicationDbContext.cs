namespace EECCORP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveEventFromApplicationDbContext : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.Events");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Summary = c.String(),
                        Description = c.String(),
                        Start = c.DateTime(nullable: false),
                        IsSelected = c.Boolean(nullable: false),
                        Week = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
    }
}
