using Microsoft.EntityFrameworkCore.Migrations;
using MicroserviceCommon.Infrastructure.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class UpdateParty : Migration
    {
		private readonly IDbContextSchema _schema;

		public UpdateParty(IDbContextSchema schema)
		{
			_schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsentsGiven",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailVerfied",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmploymentDataVerified",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasRepresentative",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HouseholdInformationVerified",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IdDataVerified",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncomeVerified",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "KycQuestionnaireFilled",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PartyDataLoaded",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ProfileDataLoaded",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProspectNumber",
                schema: _schema.Schema,
                table: "parties",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentsGiven",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "EmailVerfied",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "EmploymentDataVerified",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "HasRepresentative",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "HouseholdInformationVerified",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "IdDataVerified",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "IncomeVerified",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "KycQuestionnaireFilled",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "PartyDataLoaded",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "ProfileDataLoaded",
                schema: _schema.Schema,
                table: "parties");

            migrationBuilder.DropColumn(
                name: "ProspectNumber",
                schema: _schema.Schema,
                table: "parties");
        }
    }
}
