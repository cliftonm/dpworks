namespace RazorTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SiteProfile : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SiteProfile",
                c => new
                    {
                        SiteId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Municipality = c.String(),
                        State = c.String(),
                        ContactName = c.String(),
                        ContactPhone = c.String(),
                        ContactEmail = c.String(),
                        LogoUrl = c.String(),
                    })
                .PrimaryKey(t => t.SiteId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SiteProfile");
        }
    }
}
