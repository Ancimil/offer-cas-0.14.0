using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel
{
    public class ProductDocumentation
    {
        public DocumentContextKind DocumentContextKind { get; set; }
        public string ProductCode { get; set; }
        public PartyRoleEnum? PartyRole { get; set; }
        public string CollateralKind { get; set; }
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public string WorkItemKind { get; set; }
        public string DocumentReviewPeriod { get; set; }
        public bool IsMandatory { get; set; }
        public string WorkItemPhase { get; set; }
        public bool IsComposedFromTemplate { get; set; }
        public string TemplateUrl { get; set; }
        public bool IsForSigning { get; set; }
        public bool IsForUpload { get; set; }
        public bool IsForPhysicalArchiving { get; set; }
        public bool IsInternal { get; set; }
        public bool SupportsMultipleFiles { get; set; } = false;
        public DocumentOrigin Origin = DocumentOrigin.Product;
        public bool IsForProposal { get; set; }
    }

    public class ProductDocumentationItems
    {
        public List<ProductDocumentation> Documentation;
    }


    public enum PartyRoleEnum
    {
        [EnumMember(Value = "customer")]
        CustomerEnum,

        [EnumMember(Value = "new-customer")]
        NewCustomerEnum,

        [EnumMember(Value = "authorized-person")]
        AuthorizedPersonEnum,

        [EnumMember(Value = "new-authorized-person")]
        NewAuthorizedPersonEnum,

        [EnumMember(Value = "co-debtor")]
        CoDebtorEnum,

        [EnumMember(Value = "new-co-debtor")]
        NewCoDebtorEnum,

        [EnumMember(Value = "guarantor")]
        GuarantorEnum,

        [EnumMember(Value = "new-guarantor")]
        NewGuarantorEnum
    }
}
