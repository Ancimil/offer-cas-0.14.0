using AutoMapper;
using MicroserviceCommon.API.ApiUtils;
using MicroserviceCommon.ApiUtil;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Offer.API.Mappers
{
    public static class OrganizationMapper
    {
        public static void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PostalAddress, PostalAddress>();
            cfg.CreateMap<Coordinates, Coordinates>();
            cfg.CreateMap<ApplicationDocument, GdprApplicationDocument>();
            cfg.CreateMap<JObject, OrganizationParty>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(obj => obj["contact-name"].ToString()))
                .ForMember(dest => dest.CustomerNumber, opt => opt.MapFrom(obj => obj["party-number"].ToString()))
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(obj => obj["profile-image-url"].ToString()))
                .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(obj => obj["primary-id"]["number"].ToString()))
                .ForMember(dest => dest.CustomerSegment, opt => opt.MapFrom(obj => obj["customer"]["segment"].ToString()))
                .ForMember(dest => dest.PrimarySegment, opt => opt.MapFrom(obj => obj["customer"]["segment"].ToString()))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(obj => obj["size"].ToString()))
                .ForMember(dest => dest.RegisteredName, opt => opt.MapFrom(obj => obj["contact-name"].ToString()))
                .ForMember(dest => dest.CommercialName, opt => opt.MapFrom(obj => obj["commercial-name"].ToString()))
                .ForMember(dest => dest.LegalStructure, opt => opt.MapFrom(obj => obj["legal-structure"].ToString()))
                .ForMember(dest => dest.OrganizationPurpose, opt => opt.MapFrom(obj => obj["organization-purpose"].ToString()))
                .ForMember(dest => dest.IsSoleTrader, opt => opt.MapFrom(obj => obj["is-sole-trader"].ToString()))
                .ForMember(dest => dest.Established, opt => opt.MapFrom(obj => DateTime.Parse(obj["established"].ToString())))
                .ForMember(dest => dest.IndustrySector, opt => opt.MapFrom(obj => obj["industry-sector"].ToString()))
                .ForMember(dest => dest.FileKind, opt => opt.MapFrom(obj => obj["file-kind"].ToString()))
                .ForMember(dest => dest.AccountingMethod, opt => opt.MapFrom(obj => obj["accounting-method"].ToString()))
                .AfterMap((src, dest) =>
                {
                    var cp = ((JArray)src["contact-points"]).ToList();
                    var legalAddress = cp.Where(x => x["method"].ToString().Equals("postal") && x["usage"].ToString().Equals("legal")).FirstOrDefault();
                    if (legalAddress != null)
                    {
                        var coordinates = dest.LegalAddress.Coordinates;
                        var legalAddressObj = (PostalAddress)CaseUtil.ConvertFromJsonToObject(legalAddress["address"].ToString(), typeof(PostalAddress));
                        var newCoordinates = AutoMapper.Mapper.Map(legalAddressObj.Coordinates, coordinates, typeof(Coordinates), typeof(Coordinates));
                        AutoMapper.Mapper.Map(legalAddressObj, dest.LegalAddress, typeof(PostalAddress), typeof(PostalAddress));
                        dest.LegalAddress.Coordinates = coordinates;
                    }
                    var identificationKind = EnumUtils.ToEnum<IdentificationKind>(src["primary-id"]["kind"].ToString());
                    if (identificationKind != null)
                    {
                        dest.IdentificationNumberKind = identificationKind.Value;
                    }

                    var email = cp.Where(x => x["method"].ToString().Equals("email") && x["usage"].ToString().Equals("default")).FirstOrDefault();
                    if (email != null)
                    {
                        dest.EmailAddress = email["address"]["formatted"].ToString();
                    }

                    var mobilePhone = cp.Where(x => x["method"].ToString().Equals("gsm") && x["usage"].ToString().Equals("default")).FirstOrDefault();
                    if (mobilePhone != null)
                    {
                        dest.Phone = mobilePhone["address"]["formatted"].ToString();
                    }

                    var ownershipInfo = new Ownership();
                    ownershipInfo.Kind = src["ownership-info"]["kind"].ToString();
                    ownershipInfo.ResidentalStatus = src["ownership-info"]["residental-status"].ToString();
                    dest.OwnershipInfo = ownershipInfo;

                    dest.PreferredCulture = src["contact-preference"]["preferred-language"].ToString();

                    var relationships = ((JArray)src["relationships"])?.ToList();

                    if (relationships != null)
                    {
                        var rels = new List<Relationship>();
                        foreach (var item in relationships)
                        {
                            Relationship relation = new Relationship
                            {
                                Kind = item["kind"].ToString(),
                                Role = item["role"].ToString(),
                                ToParty = new ToParty
                                {
                                    Name = item["to-party"]["name"].ToString(),
                                    Number = item["to-party"]["number"].ToString()
                                }
                            };

                            if (Enum.TryParse(typeof(PartyKind), item["to-party"]["kind"].ToString(), true, out object kind))
                            {
                                relation.ToParty.Kind = (PartyKind)kind;
                            }
                            else
                            {
                                relation.ToParty.Kind = PartyKind.Individual;
                            }
                            rels.Add(relation);
                        }
                        dest.Relationships = rels;
                    }
                    

                    var bankAccountsFromParty = ((JArray)src["bank-accounts"])?.ToList();
                    if (bankAccountsFromParty != null)
                    {
                        var bankAccounts = new List<BankAccount>();
                        foreach (var item in bankAccountsFromParty)
                        {
                            BankAccount bankAccount = new BankAccount
                            {
                                BankName = item["bank-name"].ToString(),
                                Account = item["account-number"].ToString(),
                            };

                            bankAccounts.Add(bankAccount);
                        }
                        dest.BankAccounts = bankAccounts;
                    }

                    var idNumbersFromParty = ((JArray)src["id-numbers"])?.ToList();

                    if (idNumbersFromParty != null)
                    {
                        var idNumbers = new List<IdNumber>();
                        foreach (var item in idNumbersFromParty)
                        {
                            IdNumber idNumber = new IdNumber
                            {
                                Number = item["number"].ToString(),
                                Kind = item["kind"].ToString(),
                            };

                            idNumbers.Add(idNumber);
                        }
                        dest.IdNumbers = idNumbers;
                    }
                    
                    //var relationships = new Relationships();
                    //relationships.Kind = src["relationships"][0]["kind"].ToString();
                    //relationships.Role = src["relationships"][0]["role"].ToString();
                    //relationships.ToParty = new ToParty();
                    //relationships.ToParty.Kind = src["relationships"][0]["to-party"]["kind"].ToString();
                    //relationships.ToParty.Name = src["relationships"][0]["to-party"]["name"].ToString();
                    //relationships.ToParty.Number = src["relationships"][0]["to-party"]["number"].ToString();
                    //dest.Relationships = relationships;
                });
        }
    }
}