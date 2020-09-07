using Microsoft.EntityFrameworkCore.Migrations;
using MicroserviceCommon.Infrastructure.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddedAvailableProducts : Migration
    {
		private readonly IDbContextSchema _schema;

		public AddedAvailableProducts(IDbContextSchema schema)
		{
			_schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailableProducts",
                schema: _schema.Schema,
                table: "applications",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableProducts",
                schema: _schema.Schema,
                table: "applications");
        }
    }
}
