using Microsoft.EntityFrameworkCore.Migrations;
using MicroserviceCommon.Infrastructure.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class UpdatePartyModel : Migration
    {
		private readonly IDbContextSchema _schema;

		public UpdatePartyModel(IDbContextSchema schema)
		{
			_schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRegisteredAsCustomer",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRegisteredAsProspect",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRegisteredAsCustomer",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "IsRegisteredAsProspect",
                schema: _schema.Schema,
                table: "parties");
        }
    }
}
