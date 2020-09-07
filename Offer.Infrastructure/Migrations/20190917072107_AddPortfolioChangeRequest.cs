using System;
using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddPortfolioChangeRequest : Migration
    {
        private readonly IDbContextSchema _schema;

        public AddPortfolioChangeRequest(IDbContextSchema schema)
        {
            _schema = schema;
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "portfolio_change_requests",
                schema: _schema.Schema,
                columns: table => new
                {
                    PortfolioChangeRequestId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ApplicationId = table.Column<long>(nullable: false),
                    ChangeRequestTime = table.Column<DateTime>(nullable: false),
                    InitialValue = table.Column<string>(nullable: true),
                    RequestedValue = table.Column<string>(nullable: true),
                    FinalValue = table.Column<string>(nullable: true),
                    RequestDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolio_change_requests", x => x.PortfolioChangeRequestId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portfolio_change_requests",
                schema: _schema.Schema);
        }
    }
}
