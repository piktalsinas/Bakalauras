using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bakalauras.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigureNodeAndConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          /*  migrationBuilder.CreateTable(
                name: "NodeConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeConnections_Nodes_FromNodeId",
                        column: x => x.FromNodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NodeConnections_Nodes_ToNodeId",
                        column: x => x.ToNodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodeConnections_FromNodeId",
                table: "NodeConnections",
                column: "FromNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeConnections_ToNodeId",
                table: "NodeConnections",
                column: "ToNodeId");*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          /*  migrationBuilder.DropTable(
                name: "NodeConnections");*/
        }
    }
}
