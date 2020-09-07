using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class ArrangementRequestAddedFields : Migration
    {
        private readonly IDbContextSchema _schema;

        public ArrangementRequestAddedFields(IDbContextSchema schema)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstallmentScheduleDayOfMonth",
                schema: _schema.Schema,
                table: "arrangement_requests",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RepaymentType",
                schema: _schema.Schema,
                table: "arrangement_requests",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstallmentScheduleDayOfMonth",
                schema: _schema.Schema,
                table: "arrangement_requests");

            migrationBuilder.DropColumn(
                name: "RepaymentType",
                schema: _schema.Schema,
                table: "arrangement_requests");
        }
    }
}
