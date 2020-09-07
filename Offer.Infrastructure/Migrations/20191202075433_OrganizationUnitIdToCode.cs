using Microsoft.EntityFrameworkCore.Migrations;
using MicroserviceCommon.Infrastructure.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class OrganizationUnitIdToCode : Migration
    {
		private readonly IDbContextSchema _schema;

		public OrganizationUnitIdToCode(IDbContextSchema schema)
		{
			_schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrganizationUnitId",
                schema: _schema.Schema,
                table: "applications",
                newName: "OrganizationUnitCode"
               );

            migrationBuilder.CreateTable(
                name: "OrganizationUnits",
                schema: _schema.Schema,
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false, maxLength: 64),
                    ParentCode = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    NavigationCode = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnits", x => x.Code);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationUnits",
                schema: _schema.Schema
            );

            migrationBuilder.RenameColumn(
                name: "OrganizationUnitCode",
                schema: _schema.Schema,
                table: "applications",
                newName: "OrganizationUnitId"
           );
        }
    }
}
