using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Offer.Infrastructure.Migrations
{
    public partial class AddedIsForProposalInApplicationDocuments : Migration
    {
        private readonly IDbContextSchema _schema;
        public AddedIsForProposalInApplicationDocuments(IDbContextSchema schema)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsForProposal",
                schema: _schema.Schema,
                table: "application_documents",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsForProposal",
                schema: _schema.Schema,
                table: "application_documents");
        }
    }
}
