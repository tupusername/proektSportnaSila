using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportnaSila.Data.Migrations
{
    /// <inheritdoc />
    public partial class SportnaSilaDb1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_ClientId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ClientId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ClientId",
                table: "Orders",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_ClientId",
                table: "Orders",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_ClientId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ClientId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ClientId1",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ClientId1",
                table: "Orders",
                column: "ClientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_ClientId1",
                table: "Orders",
                column: "ClientId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
