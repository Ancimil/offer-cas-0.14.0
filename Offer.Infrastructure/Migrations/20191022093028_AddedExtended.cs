using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddedExtended : Migration
    {
        private readonly IDbContextSchema _schema;
        public AddedExtended(IDbContextSchema schema)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: _schema.Schema);

            migrationBuilder.AddColumn<string>(
                name: "Extended",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extended",
                schema: _schema.Schema,
                table: "arrangement_requests",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extended",
                schema: _schema.Schema,
                table: "applications",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Extended",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "Extended",
                schema: _schema.Schema,
                table: "arrangement_requests");

            migrationBuilder.DropColumn(
                name: "Extended",
                schema: _schema.Schema,
                table: "applications");
        }
    }
}
