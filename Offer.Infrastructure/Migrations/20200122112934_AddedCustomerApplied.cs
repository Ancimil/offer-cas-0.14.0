using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddedCustomerApplied : Migration
    {
        private readonly IDbContextSchema _schema;
        public AddedCustomerApplied(IDbContextSchema schema)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CustomerApplied",
                schema: _schema.Schema,
                table: "applications",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerApplied",
                schema: _schema.Schema,
                table: "applications");
        }
    }
}
