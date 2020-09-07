using Microsoft.EntityFrameworkCore.Migrations;
using MicroserviceCommon.Infrastructure.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class SameLengthOfOrganizationUnitCode : Migration
    {
		private readonly IDbContextSchema _schema;

		public SameLengthOfOrganizationUnitCode(IDbContextSchema schema)
		{
			_schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: _schema.Schema,
                table: "OrganizationUnits",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationUnitCode",
                schema: _schema.Schema,
                table: "applications",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1024,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: _schema.Schema,
                table: "OrganizationUnits",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationUnitCode",
                schema: _schema.Schema,
                table: "applications",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
