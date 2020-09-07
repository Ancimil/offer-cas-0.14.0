using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddSchedulingPeriodsToARequest : Migration
    {
        private readonly IDbContextSchema _schema;

        public AddSchedulingPeriodsToARequest(IDbContextSchema schema)
        {
            _schema = schema;
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "_Periods",
                schema: _schema.Schema,
                table: "arrangement_requests",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "_Periods",
                schema: _schema.Schema,
                table: "arrangement_requests");
        }
    }
}
