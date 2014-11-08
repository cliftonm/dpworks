namespace RazorTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateSiteTable : DbMigration
    {
        public override void Up()
        {
			DropPrimaryKey("dbo.SiteProfile", new[] { "SiteId" });
			DropColumn("dbo.SiteProfile", "SiteId");
			AddColumn("dbo.SiteProfile", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.SiteProfile", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SiteProfile", "SiteId", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.SiteProfile", new[] { "Id" });
            AddPrimaryKey("dbo.SiteProfile", "SiteId");
            DropColumn("dbo.SiteProfile", "Id");
        }
    }
}
