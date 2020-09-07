using Microsoft.EntityFrameworkCore.Migrations;
using MicroserviceCommon.Infrastructure.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddAppartmentAndFloor : Migration
    {
		private readonly IDbContextSchema _schema;

		public AddAppartmentAndFloor(IDbContextSchema schema)
		{
			_schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactAddress_Apartment",
                schema: _schema.Schema,
                table: "parties",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactAddress_Floor",
                schema: _schema.Schema,
                table: "parties",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalAddress_Apartment",
                schema: _schema.Schema,
                table: "parties",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalAddress_Floor",
                schema: _schema.Schema,
                table: "parties",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactAddress_Apartment",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "ContactAddress_Floor",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "LegalAddress_Apartment",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "LegalAddress_Floor",
                schema: _schema.Schema,
                table: "parties");
        }
    }
}
