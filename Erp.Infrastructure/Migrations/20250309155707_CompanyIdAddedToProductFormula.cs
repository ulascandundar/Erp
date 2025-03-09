using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Erp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompanyIdAddedToProductFormula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "ProductFormulas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ProductFormulas_CompanyId",
                table: "ProductFormulas",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductFormulas_Companies_CompanyId",
                table: "ProductFormulas",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductFormulas_Companies_CompanyId",
                table: "ProductFormulas");

            migrationBuilder.DropIndex(
                name: "IX_ProductFormulas_CompanyId",
                table: "ProductFormulas");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "ProductFormulas");
        }
    }
}
