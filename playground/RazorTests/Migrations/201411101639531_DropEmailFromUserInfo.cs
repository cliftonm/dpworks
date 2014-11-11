namespace RazorTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropEmailFromUserInfo : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.UserInfo", "Email");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserInfo", "Email", c => c.String(nullable: false));
        }
    }
}
