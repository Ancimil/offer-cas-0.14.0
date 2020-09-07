using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddFinancialStatements : Migration
    {
        private readonly IDbContextSchema _schema;

        public AddFinancialStatements(IDbContextSchema schema)
        {
            _schema = schema;
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinancialStatements",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinancialStatements",
                schema: _schema.Schema,
                table: "parties");
        }
    }
}
