using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddedLeadIdToApplication : Migration
    {
        private readonly IDbContextSchema _schema;
        public AddedLeadIdToApplication(IDbContextSchema schema)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LeadId",
                schema: _schema.Schema,
                table: "applications",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeadId",
                schema: _schema.Schema,
                table: "applications");
        }
    }
}
