using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bakalauras.Persistence.Migrations
{
    public partial class AddNameColumnToNodeConnections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "NodeConnections", 
                maxLength: 100, 
                nullable: false, 
                defaultValue: ""); 
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name", 
                table: "NodeConnections");
        }
    }
}
