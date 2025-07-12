using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotByteProject.Migrations
{
    /// <inheritdoc />
    public partial class dbcontextchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Menus_MenuId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Menus_MenuId",
                table: "OrderItems");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Menus_MenuId",
                table: "CartItems",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "MenuId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Menus_MenuId",
                table: "OrderItems",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "MenuId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Menus_MenuId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Menus_MenuId",
                table: "OrderItems");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Menus_MenuId",
                table: "CartItems",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "MenuId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Menus_MenuId",
                table: "OrderItems",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "MenuId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
