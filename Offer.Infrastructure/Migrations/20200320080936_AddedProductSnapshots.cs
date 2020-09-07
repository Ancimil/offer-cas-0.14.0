using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddedProductSnapshots : Migration
    {
        private readonly IDbContextSchema _schema;
        public AddedProductSnapshots(IDbContextSchema schema)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isOracleCheck = migrationBuilder.ActiveProvider.Equals("Oracle.EntityFrameworkCore") ||
                            migrationBuilder.ActiveProvider.Equals("Devart.Data.Oracle.Entity.EFCore") ||
                            (System.Environment.GetEnvironmentVariable("DATABASE_TYPE") != null ? System.Environment.GetEnvironmentVariable("DATABASE_TYPE").ToLower().Equals("oracle") : false);

            var typeForOracle = isOracleCheck ? "NVARCHAR2(450)" : null;

            migrationBuilder.AddColumn<string>(
                name: "ProductSnapshotHash",
                schema: _schema.Schema,
                table: "arrangement_requests",
                nullable: true,
                type: typeForOracle);

            migrationBuilder.CreateTable(
                name: "product_snapshots",
                schema: _schema.Schema,
                columns: table => new
                {
                    Hash = table.Column<string>(nullable: false, type: typeForOracle),
                    ProdctSnapshot = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_snapshots", x => x.Hash);
                });

            migrationBuilder.CreateIndex(
                name: "IX_arrangement_requests_ProductSnapshotHash",
                schema: _schema.Schema,
                table: "arrangement_requests",
                column: "ProductSnapshotHash");

            migrationBuilder.AddForeignKey(
                name: "FK_arrangement_requests_product_snapshots_ProductSnapshotHash",
                schema: _schema.Schema,
                table: "arrangement_requests",
                column: "ProductSnapshotHash",
                principalSchema: _schema.Schema,
                principalTable: "product_snapshots",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_arrangement_requests_product_snapshots_ProductSnapshotHash",
                schema: _schema.Schema,
                table: "arrangement_requests");

            migrationBuilder.DropTable(
                name: "product_snapshots",
                schema: _schema.Schema);

            migrationBuilder.DropIndex(
                name: "IX_arrangement_requests_ProductSnapshotHash",
                schema: _schema.Schema,
                table: "arrangement_requests");

            migrationBuilder.DropColumn(
                name: "ProductSnapshotHash",
                schema: _schema.Schema,
                table: "arrangement_requests");
        }
    }
}
