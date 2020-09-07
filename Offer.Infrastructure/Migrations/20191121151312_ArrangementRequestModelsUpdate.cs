using Microsoft.EntityFrameworkCore.Migrations;
using MicroserviceCommon.Infrastructure.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class ArrangementRequestModelsUpdate : Migration
    {
		private readonly IDbContextSchema _schema;

		public ArrangementRequestModelsUpdate(IDbContextSchema schema)
		{
			_schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
