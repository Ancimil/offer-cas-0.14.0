using System;
using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Offer.Infrastructure.Migrations
{
    public partial class ModelsUpdate : Migration
    {
		private readonly IDbContextSchema _schema;

		public ModelsUpdate(IDbContextSchema schema)
		{
			_schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousWorkPeriod",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "ArrangementNumber",
                schema: _schema.Schema,
                table: "applications");

            migrationBuilder.DropColumn(
                name: "Comments",
                schema: _schema.Schema,
                table: "applications");

            migrationBuilder.DropColumn(
                name: "RequestedActivationDate",
                schema: _schema.Schema,
                table: "applications");

            migrationBuilder.DropColumn(
                name: "SettlementAccount",
                schema: _schema.Schema,
                table: "applications");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviousWorkPeriod",
                schema: _schema.Schema,
                table: "parties",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArrangementNumber",
                schema: _schema.Schema,
                table: "applications",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                schema: _schema.Schema,
                table: "applications",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedActivationDate",
                schema: _schema.Schema,
                table: "applications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettlementAccount",
                schema: _schema.Schema,
                table: "applications",
                maxLength: 256,
                nullable: true);
        }
    }
}
