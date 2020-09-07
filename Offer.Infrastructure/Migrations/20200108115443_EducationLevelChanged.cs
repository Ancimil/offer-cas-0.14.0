using Microsoft.EntityFrameworkCore.Migrations;
using MicroserviceCommon.Infrastructure.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class EducationLevelChanged : Migration
    {
		private readonly IDbContextSchema _schema;
		public EducationLevelChanged(IDbContextSchema schema)
		{
			_schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isOracleCheck = migrationBuilder.ActiveProvider.Equals("Oracle.EntityFrameworkCore") ||
                            migrationBuilder.ActiveProvider.Equals("Devart.Data.Oracle.Entity.EFCore") ||
                            (System.Environment.GetEnvironmentVariable("DATABASE_TYPE") != null ? System.Environment.GetEnvironmentVariable("DATABASE_TYPE").ToLower().Equals("oracle") : false);

            if (isOracleCheck)
            {
                migrationBuilder.DropColumn(
                                name: "EducationLevel",
                                schema: _schema.Schema,
                                table: "parties");

                migrationBuilder.AddColumn<string>(
                    name: "EducationLevel",
                    schema: _schema.Schema,
                    table: "parties",
                    nullable: true,
                    maxLength: 256);
            } else
            {
                migrationBuilder.AlterColumn<string>(
                    name: "EducationLevel",
                    schema: _schema.Schema,
                    table: "parties",
                    nullable: true,
                    oldClrType: typeof(int),
                    oldNullable: true);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var isOracleCheck = migrationBuilder.ActiveProvider.Equals("Oracle.EntityFrameworkCore") ||
                            migrationBuilder.ActiveProvider.Equals("Devart.Data.Oracle.Entity.EFCore") ||
                            (System.Environment.GetEnvironmentVariable("DATABASE_TYPE") != null ? System.Environment.GetEnvironmentVariable("DATABASE_TYPE").ToLower().Equals("oracle") : false);

            if (isOracleCheck)
            {
                migrationBuilder.DropColumn(
                name: "EducationLevel",
                schema: _schema.Schema,
                table: "parties");

                migrationBuilder.AddColumn<int>(
                    name: "EducationLevel",
                    schema: _schema.Schema,
                    table: "parties",
                    maxLength: 32,
                    nullable: true);
            }
            else
            {
                migrationBuilder.AlterColumn<int>(
                    name: "EducationLevel",
                    schema: _schema.Schema,
                    table: "parties",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldNullable: true);
            }
        }
    }
}
