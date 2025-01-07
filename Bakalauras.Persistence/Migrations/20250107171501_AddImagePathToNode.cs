using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bakalauras.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathToNode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Nodes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Nodes");
        }
    }
}
