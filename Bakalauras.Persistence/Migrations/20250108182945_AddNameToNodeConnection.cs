using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bakalauras.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToNodeConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionName",
                table: "NodeConnections");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "NodeConnections",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "NodeConnections");

            migrationBuilder.AddColumn<string>(
                name: "ConnectionName",
                table: "NodeConnections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
