namespace RazorTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateUnitTable1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Units", "SiteId", "dbo.SiteProfile");
            DropIndex("dbo.Units", new[] { "SiteId" });
            CreateTable(
                "dbo.Unit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SiteId = c.Int(nullable: false),
                        Abbr = c.String(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SiteProfile", t => t.SiteId, cascadeDelete: true)
                .Index(t => t.SiteId);
            
            DropTable("dbo.Units");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Units",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SiteId = c.Int(nullable: false),
                        Abbr = c.String(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropIndex("dbo.Unit", new[] { "SiteId" });
            DropForeignKey("dbo.Unit", "SiteId", "dbo.SiteProfile");
            DropTable("dbo.Unit");
            CreateIndex("dbo.Units", "SiteId");
            AddForeignKey("dbo.Units", "SiteId", "dbo.SiteProfile", "Id", cascadeDelete: true);
        }
    }
}
