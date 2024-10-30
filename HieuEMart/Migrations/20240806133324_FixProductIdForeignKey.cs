using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HieuEMart.Migrations
{
    public partial class FixProductIdForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xóa ràng buộc khóa ngoại từ OrderDetails tới Products
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails");

            // Xóa khóa chính hiện tại trên bảng Products
            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            // Thay đổi kiểu dữ liệu của cột Id trong bảng Products
            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Products",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            // Tạo lại khóa chính trên bảng Products
            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            // Tạo lại ràng buộc khóa ngoại từ OrderDetails tới Products
            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa ràng buộc khóa ngoại từ OrderDetails tới Products
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails");

            // Xóa khóa chính hiện tại trên bảng Products
            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            // Thay đổi kiểu dữ liệu của cột Id trong bảng Products trở lại int
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            // Tạo lại khóa chính trên bảng Products
            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            // Tạo lại ràng buộc khóa ngoại từ OrderDetails tới Products
            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails");
        }
    }
}
