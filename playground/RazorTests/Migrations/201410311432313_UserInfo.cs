namespace RazorTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserInfo : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserInfo",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        SiteId = c.Int(nullable: false),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        RegistrationToken = c.String(nullable: false),
                        Activated = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.SiteProfile", t => t.SiteId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.SiteId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserInfo", new[] { "SiteId" });
            DropIndex("dbo.UserInfo", new[] { "UserId" });
            DropForeignKey("dbo.UserInfo", "SiteId", "dbo.SiteProfile");
            DropForeignKey("dbo.UserInfo", "UserId", "dbo.UserProfile");
            DropTable("dbo.UserInfo");
        }
    }
}
