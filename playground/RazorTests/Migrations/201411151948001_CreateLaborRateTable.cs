namespace RazorTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateLaborRateTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LaborRate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SiteId = c.Int(nullable: false),
                        Position = c.String(nullable: false),
                        HourlyRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SiteProfile", t => t.SiteId, cascadeDelete: true)
                .Index(t => t.SiteId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.LaborRate", new[] { "SiteId" });
            DropForeignKey("dbo.LaborRate", "SiteId", "dbo.SiteProfile");
            DropTable("dbo.LaborRate");
        }
    }
}
