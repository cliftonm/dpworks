namespace RazorTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EquipmentAndMaterialTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Equipment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SiteId = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                        HourlyRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SiteProfile", t => t.SiteId, cascadeDelete: true)
                .Index(t => t.SiteId);
            
            CreateTable(
                "dbo.Material",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SiteId = c.Int(nullable: false),
                        Name = c.String(nullable: false),
                        UnitId = c.Int(nullable: false),
                        UnitCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SiteProfile", t => t.SiteId, cascadeDelete: false)
                .ForeignKey("dbo.Unit", t => t.UnitId, cascadeDelete: false)
                .Index(t => t.SiteId)
                .Index(t => t.UnitId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Material", new[] { "UnitId" });
            DropIndex("dbo.Material", new[] { "SiteId" });
            DropIndex("dbo.Equipment", new[] { "SiteId" });
            DropForeignKey("dbo.Material", "UnitId", "dbo.Unit");
            DropForeignKey("dbo.Material", "SiteId", "dbo.SiteProfile");
            DropForeignKey("dbo.Equipment", "SiteId", "dbo.SiteProfile");
            DropTable("dbo.Material");
            DropTable("dbo.Equipment");
        }
    }
}
