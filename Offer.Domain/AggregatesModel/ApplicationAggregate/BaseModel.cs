using System;

namespace Offer.Domain.AggregatesModel.ApplicationAggregate
{
    [AttributeUsage(AttributeTargets.Property)]
    public class BaseModel : Attribute
    {
    }
}
