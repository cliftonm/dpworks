namespace RazorTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PutEmailBack : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserInfo", "Email", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserInfo", "Email");
        }
    }
}
