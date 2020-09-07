using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Offer.Infrastructure.Migrations
{
    public partial class RenameSuppliersBuyersReportIdColumn : Migration
    {
        private readonly IDbContextSchema _schema;

        public RenameSuppliersBuyersReportIdColumn(IDbContextSchema schema)
        {
            _schema = schema;
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupppliersBuyersReportId",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.AddColumn<long>(
                name: "SuppliersBuyersReportId",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuppliersBuyersReportId",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.AddColumn<long>(
                name: "SupppliersBuyersReportId",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);
        }
    }
}
