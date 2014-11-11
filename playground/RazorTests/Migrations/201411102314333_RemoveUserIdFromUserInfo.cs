namespace RazorTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUserIdFromUserInfo : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserInfo", "UserId", "dbo.UserProfile");
            DropIndex("dbo.UserInfo", new[] { "UserId" });
            DropColumn("dbo.UserInfo", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserInfo", "UserId", c => c.Int(nullable: false));
            CreateIndex("dbo.UserInfo", "UserId");
            AddForeignKey("dbo.UserInfo", "UserId", "dbo.UserProfile", "UserId", cascadeDelete: true);
        }
    }
}
