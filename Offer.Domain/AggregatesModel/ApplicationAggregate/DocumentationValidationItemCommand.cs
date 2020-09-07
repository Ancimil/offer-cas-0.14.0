using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ProductPartyRoleEnum = Offer.Domain.AggregatesModel.ApplicationAggregate.ProductModel.PartyRoleEnum;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    public class DocumentationValidationItemCommand
    {
        [Required]
        public DocumentStatus RequiredStatus { get; set; }
        public DocumentContextKind DocumentContextKind { get; set; }
        public string ProductCode { get; set; }
        public ProductPartyRoleEnum? PartyRole { get; set; }
        public string CollateralKind { get; set; }
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }

        public List<string> ProductCodes { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is DocumentationValidationItemCommand comm)
            {
                return Equals(comm);
            }
            return false;
        }

        public bool Equals(DocumentationValidationItemCommand other)
        {
            if (other == null) return false;

            bool passedProductCodesCheck = false;
            bool instanceHasProductCodes = this.ProductCodes != null && this.ProductCodes.Any();
            bool compareHasProductCodes = other.ProductCodes != null && other.ProductCodes.Any();
            if (!instanceHasProductCodes || 
                !compareHasProductCodes ||
                (other.ProductCodes.Intersect(this.ProductCodes).DefaultIfEmpty().Count() > 0))
            {
                passedProductCodesCheck = true;
            }

            return passedProductCodesCheck &&
                   DocumentContextKind == other.DocumentContextKind &&
                   DocumentType == other.DocumentType && 
                   PartyRole == other.PartyRole &&
                   CollateralKind == other.CollateralKind;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DocumentContextKind, DocumentType, PartyRole, CollateralKind);
        }
    }
}
