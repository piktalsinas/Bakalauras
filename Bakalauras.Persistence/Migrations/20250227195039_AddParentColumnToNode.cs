using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bakalauras.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParentColumnToNode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the ParentName column to the Node table
            migrationBuilder.AddColumn<string>(
                name: "ParentName",
                table: "Nodes",
                type: "nvarchar(max)", 
                nullable: true);       
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the ParentName column from the Node table when rolling back the migration
            migrationBuilder.DropColumn(
                name: "ParentName",
                table: "Nodes");
        }
    }
}
