using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddSuppliersBuyersReportIdToInvParty : Migration
    {
        private readonly IDbContextSchema _schema;

        public AddSuppliersBuyersReportIdToInvParty(IDbContextSchema schema)
        {
            _schema = schema;
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SupppliersBuyersReportId",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupppliersBuyersReportId",
                schema: _schema.Schema,
                table: "parties");
        }
    }
}
