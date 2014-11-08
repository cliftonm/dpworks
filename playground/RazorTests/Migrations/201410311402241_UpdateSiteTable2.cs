namespace RazorTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateSiteTable2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SiteProfile", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.SiteProfile", "Municipality", c => c.String(nullable: false));
            AlterColumn("dbo.SiteProfile", "State", c => c.String(nullable: false, maxLength: 2));
            AlterColumn("dbo.SiteProfile", "ContactName", c => c.String(nullable: false));
            AlterColumn("dbo.SiteProfile", "ContactPhone", c => c.String(nullable: false));
            AlterColumn("dbo.SiteProfile", "ContactEmail", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SiteProfile", "ContactEmail", c => c.String());
            AlterColumn("dbo.SiteProfile", "ContactPhone", c => c.String());
            AlterColumn("dbo.SiteProfile", "ContactName", c => c.String());
            AlterColumn("dbo.SiteProfile", "State", c => c.String());
            AlterColumn("dbo.SiteProfile", "Municipality", c => c.String());
            AlterColumn("dbo.SiteProfile", "Name", c => c.String());
        }
    }
}
