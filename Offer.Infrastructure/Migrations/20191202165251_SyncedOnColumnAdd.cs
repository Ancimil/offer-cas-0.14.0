using System;
using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Offer.Infrastructure.Migrations
{
    public partial class SyncedOnColumnAdd : Migration
    {
		private readonly IDbContextSchema _schema;

		public SyncedOnColumnAdd(IDbContextSchema schema)
		{
			_schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SyncedOn",
                schema: _schema.Schema,
                table: "OrganizationUnits",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SyncedOn",
                schema: _schema.Schema,
                table: "OrganizationUnits");
        }
    }
}
