using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vantus.Engine.Migrations
{
    /// <inheritdoc />
    public partial class AddContentColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "FileIndexItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "FileIndexItems");
        }
    }
}
