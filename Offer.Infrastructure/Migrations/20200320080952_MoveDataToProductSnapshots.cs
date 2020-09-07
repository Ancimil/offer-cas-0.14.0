using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Offer.Infrastructure.Migrations
{
    public partial class MoveDataToProductSnapshots : Migration
    {
        private readonly IDbContextSchema _schema;
        public MoveDataToProductSnapshots(IDbContextSchema schema)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            List<string> list = new List<string>();
            using (var context = new OfferDBContext())
            {
                var arrRequests = context.ArrangementRequests.Where(a => !string.IsNullOrEmpty(a._ProductSnapshot) && string.IsNullOrEmpty(a._ProductSnapshotHash)).ToList();

                foreach (var arr in arrRequests)
                {
                    ProductSnapshot ps = new ProductSnapshot();
                    try
                    {
                        ps = JsonConvert.DeserializeObject<ProductSnapshot>(arr._ProductSnapshot);
                    }
                    catch
                    {
                        continue;
                    }

                    string hashCode = OfferUtility.CreateMD5(JsonConvert.SerializeObject(ps));
                    var sp = context.ProductSnapshots.Where(x => x.Hash.Equals(hashCode)).FirstOrDefault();
                    if (!list.Any(p => p.Equals(hashCode)))
                    {
                        list.Add(hashCode);
                        migrationBuilder.InsertData(
                            table: "product_snapshots",
                            schema: _schema.Schema,
                            columns: new[] { "Hash", "ProdctSnapshot" },
                        values: new object[,]
                        {
                            { hashCode, JsonConvert.SerializeObject(ps) }
                        });
                    }
                    migrationBuilder.UpdateData(
                            table: "arrangement_requests",
                            schema: _schema.Schema,
                            keyColumns: new string[] { "ApplicationId", "ArrangementRequestId" },
                            keyValues: new object[] { arr.ApplicationId, arr.ArrangementRequestId },
                            column: "ProductSnapshotHash",
                            value: hashCode);
                }
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
