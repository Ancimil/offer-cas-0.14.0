using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Offer.Infrastructure.Migrations
{
    public partial class CreditLineRequest : Migration
    {
        private readonly IDbContextSchema _schema;

        public CreditLineRequest(IDbContextSchema schema)
        {
            _schema = schema;
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "_CreditLineLimits",
                schema: _schema.Schema,
                table: "arrangement_requests",
                newName: "CreditLineLimits");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreditLineLimits",
                schema: _schema.Schema,
                table: "arrangement_requests",
                newName: "_CreditLineLimits");
        }
    }
}
