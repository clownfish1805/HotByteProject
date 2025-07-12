using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotByteProject.Migrations
{
    /// <inheritdoc />
    public partial class ContactaddeddinUSer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Users");
        }
    }
}
