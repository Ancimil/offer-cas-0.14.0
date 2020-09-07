using Microsoft.EntityFrameworkCore.Migrations;
using MicroserviceCommon.Infrastructure.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddForeignKeyToOrganizationCode : Migration
    {
		private readonly IDbContextSchema _schema;

		public AddForeignKeyToOrganizationCode(IDbContextSchema schema)
		{
			_schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_applications_OrganizationUnitCode",
                schema: _schema.Schema,
                table: "applications",
                column: "OrganizationUnitCode");

            migrationBuilder.AddForeignKey(
                name: "FK_applications_OrganizationUnits_OrganizationUnitCode",
                schema: _schema.Schema,
                table: "applications",
                column: "OrganizationUnitCode",
                principalSchema: _schema.Schema,
                principalTable: "OrganizationUnits",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_applications_OrganizationUnits_OrganizationUnitCode",
                schema: _schema.Schema,
                table: "applications");

            migrationBuilder.DropIndex(
                name: "IX_applications_OrganizationUnitCode",
                schema: _schema.Schema,
                table: "applications");
        }
    }
}
