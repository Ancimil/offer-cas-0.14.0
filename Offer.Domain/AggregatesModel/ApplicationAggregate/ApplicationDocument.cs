using MicroserviceCommon.Contracts;
using Newtonsoft.Json;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    #region Enumerations
    [Enumeration("document-context-kind", "Document Context Kind", "Document Context Kind")]
    public enum DocumentContextKind
    {
        [EnumMember(Value = "application")]
        [Description("Application context")]
        ApplicationEnum,

        [EnumMember(Value = "party")]
        [Description("Party context")]
        PartyEnum,

        [EnumMember(Value = "collateral")]
        [Description("Collateral context")]
        CollateralEnum,

        [EnumMember(Value = "arrangement-request")]
        [Description("Arrangement request context")]
        ArrangementRequestEnum,


        [EnumMember(Value = "collateral-arrangement")]
        [Description("Collateral arrangement context")]
        CollateralArrangementEnum,
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

    [Enumeration("document-status", "Document Status", "Document Status")]
    public enum DocumentStatus
    {
        [EnumMember(Value = "empty")]
        [Description("Document is empty")]
        EmptyEnum = 0,

        [EnumMember(Value = "composed")]
        [Description("Document is composed from template")]
        ComposedEnum = 1,

        [EnumMember(Value = "uploaded")]
        [Description("Document is uploaded")]
        UploadedEnum = 2,

        [EnumMember(Value = "signed")]
        [Description("Document is signed")]
        SignedEnum = 3,

        [EnumMember(Value = "signed-by-bank")]
        [Description("Document is signed by bank")]
        SignedByBank = 6,

        [EnumMember(Value = "accepted-by-customer")]
        [Description("Document is accepted by customer")]
        AcceptedByCustomerEnum = 4 ,

        [EnumMember(Value = "accepted-by-agent")]
        [Description("Document is accepted by agent")]
        AcceptedByAgentEnum = 5

    }

    [Enumeration("document-origin", "Document Origin", "Origin of application document")]
    public enum DocumentOrigin
    {
        [EnumMember(Value = "product")]
        [Description("Document is defined in product requirements")]
        Product,

        [EnumMember(Value = "process")]
        [Description("Document is added through business process")]
        Process,

        [EnumMember(Value = "agent")]
        [Description("Agent added the document")]
        Agent
    }

    [Enumeration("document-requirement-validation-status", "Document Requirement Validation Status", "Document Requirement Validation Status")]
    public enum DocumentRequirementValidationStatus
    {
        [EnumMember(Value = "valid")]
        [Description("Status is valid due to milestone requirements")]
        ValidEnum,

        [EnumMember(Value = "not-found")]
        [Description("Document is not found")]
        NotFoundEnum,

        [EnumMember(Value = "wrong-status")]
        [Description("Status is not same as required")]
        WrongStatusEnum
    }
    #endregion
    public class ApplicationDocument : IEquatable<ApplicationDocument>
    {
        [Column(Order = 0), ForeignKey("Application")]
        [JsonIgnore]
        public long ApplicationId { get; set; } // references parent app number, part of key
        [Key]
        public int DocumentId { get; set; } // key, together with parent Application Number

        [NotMapped]
        public string ApplicationNumber
        {
            get
            {
                var result = "0000000000" + ApplicationId;
                return result.Substring(result.Length - 10);
            }
        }
        public DocumentContextKind DocumentContextKind { get; set; }
        [MaxLength(256)]
        public string Context { get; set; }
        [MaxLength(256)]
        public int? ArrangementRequestId { get; set; }
        [JsonIgnore]
        public ArrangementRequest ArrangementRequest { get; set; }
        [MaxLength(256)]
        public string CollateralId { get; set; }
        [MaxLength(256)]
        public long? PartyId { get; set; }
        [JsonIgnore]
        public Party Party { get; set; }
        [MaxLength(256)]
        public string DocumentName { get; set; }
        [MaxLength(256)]
        public string DocumentKind { get; set; }
        [MaxLength(256)]
        public string DocumentReviewPeriod { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsComposedFromTemplate { get; set; }
        [MaxLength(1024)]
        public string TemplateUrl { get; set; }
        public bool IsForSigning { get; set; }
        public bool IsForUpload { get; set; }
        public bool IsForPhysicalArchiving { get; set; }
        public bool IsInternal { get; set; }
        public bool SupportsMultipleFiles { get; set; } = false;
        [DefaultValue(DocumentOrigin.Product)]
        public DocumentOrigin Origin { get; set; }
        public DocumentStatus Status { get; set; }
        public bool IsForProposal { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as ApplicationDocument);
        }

        public bool Equals(DocumentValidationItemResponse other)
        {
            return other != null &&
                   // ApplicationNumber == other.ApplicationNumber &&
                   DocumentContextKind == other.DocumentContextKind &&
                   ArrangementRequestId == other.ArrangementRequestId &&
                   CollateralId == other.CollateralId &&
                   PartyId == other.PartyId &&
                   DocumentKind == other.DocumentKind;
        }

        public bool Equals(ApplicationDocument other)
        {
            return other != null &&
                   ApplicationNumber == other.ApplicationNumber &&
                   DocumentContextKind == other.DocumentContextKind &&
                   ArrangementRequestId == other.ArrangementRequestId &&
                   CollateralId == other.CollateralId &&
                   PartyId == other.PartyId &&
                   DocumentKind == other.DocumentKind &&
                   DocumentName == other.DocumentName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ApplicationId, DocumentId, ApplicationNumber, DocumentContextKind, ArrangementRequestId, CollateralId, PartyId, DocumentKind);
        }


        public static ApplicationDocument FromProductDocument(ProductDocumentation productDocumentation)
        {
            return new ApplicationDocument
            {
                DocumentContextKind = productDocumentation.DocumentContextKind,
                DocumentKind = productDocumentation.DocumentType,
                DocumentName = productDocumentation.DocumentName,
                DocumentReviewPeriod = productDocumentation.DocumentReviewPeriod,
                IsComposedFromTemplate = productDocumentation.IsComposedFromTemplate,
                IsForPhysicalArchiving = productDocumentation.IsForPhysicalArchiving,
                IsForSigning = productDocumentation.IsForSigning,
                IsForUpload = productDocumentation.IsForUpload,
                IsInternal = productDocumentation.IsInternal,
                IsMandatory = productDocumentation.IsMandatory,
                TemplateUrl = productDocumentation.TemplateUrl,
                Origin = productDocumentation.Origin,
                Status = DocumentStatus.EmptyEnum,
                IsForProposal = productDocumentation.IsForProposal
            };
        }
    }

    public class ApplicationDocumentList
    {
        public List<ApplicationDocument> Documents { get; set; }
    }
}
