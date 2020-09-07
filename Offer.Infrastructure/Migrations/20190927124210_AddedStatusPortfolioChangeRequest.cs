using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddedStatusPortfolioChangeRequest : Migration
    {
        private readonly IDbContextSchema _schema;

        public AddedStatusPortfolioChangeRequest(IDbContextSchema schema)
        {
            _schema = schema;
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: _schema.Schema,
                table: "portfolio_change_requests",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: _schema.Schema,
                table: "portfolio_change_requests");
        }
    }
}
