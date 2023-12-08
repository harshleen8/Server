using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerBlogManagement.Migrations
{
    public partial class UserTableUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Users",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users') " +
                                 "BEGIN " +
                                 "CREATE TABLE [Users] (" +
                                 "    [Id] int NOT NULL IDENTITY," +
                                 "    [Username] nvarchar(max) NOT NULL," +
                                 "    [PasswordHash] nvarchar(max) NOT NULL," +
                                 "    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])" +
                                 ");" +
                                 "END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.Sql("DROP TABLE IF EXISTS [Users];");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "UserId");
        }
    }
}
