using AutoMapper;
using Iso8601Duration;
using MicroserviceCommon.API.ApiUtils;
using MicroserviceCommon.ApiUtil;
using Newtonsoft.Json.Linq;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Domain.AggregatesModel.ApplicationAggregate.ApplicantModel;
using PriceCalculation.Calculations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Offer.API.Mappers
{
    public static class IndividualMapper
    {


        public static void Configure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<PostalAddress, PostalAddress>();
            cfg.CreateMap<Coordinates, Coordinates>();
            cfg.CreateMap<ApplicationDocument, GdprApplicationDocument>();
            cfg.CreateMap<JObject, IndividualParty>()
                .ForMember(dest => dest.GivenName, opt => opt.MapFrom(obj => obj["given-name"].ToString()))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(obj => obj["surname"].ToString()))
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(obj => obj["parent-name"].ToString()))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(obj => obj["customer-name"].ToString()))
                .ForMember(dest => dest.CustomerNumber, opt => opt.MapFrom(obj => obj["party-number"].ToString()))
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(obj => obj["profile-image-url"].ToString()))
                .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(obj => obj["primary-id"]["number"].ToString()))
                .ForMember(dest => dest.CustomerSegment, opt => opt.MapFrom(obj => obj["customer"]["segment"].ToString()))
                .ForMember(dest => dest.PrimarySegment, opt => opt.MapFrom(obj => obj["customer"]["segment"].ToString()))
                .ForMember(dest => dest.MaritalStatus, opt => opt.MapFrom(obj => obj["marital-status"].ToString()))
                .ForMember(dest => dest.PlaceOfBirth, opt => opt.MapFrom(obj => obj["place-of-birth"].ToString()))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(obj => DateTime.Parse(obj["birth-date"].ToString())))
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
                    var contactAddress = cp.Where(x => x["method"].ToString().Equals("postal") && x["usage"].ToString().Equals("home")).FirstOrDefault();
                    if (contactAddress != null)
                    {
                        var coordinates = dest.ContactAddress.Coordinates;
                        var contactAddressObj = (PostalAddress)CaseUtil.ConvertFromJsonToObject(contactAddress["address"].ToString(), typeof(PostalAddress));
                        var newCoordinates = AutoMapper.Mapper.Map(contactAddressObj.Coordinates, coordinates, typeof(Coordinates), typeof(Coordinates));
                        AutoMapper.Mapper.Map(contactAddressObj, dest.ContactAddress, typeof(PostalAddress), typeof(PostalAddress));
                        dest.ContactAddress.Coordinates = coordinates;
                    }
                    var identificationKind = EnumUtils.ToEnum<IdentificationKind>(src["primary-id"]["kind"].ToString());
                    if (identificationKind != null)
                    {
                        dest.IdentificationNumberKind = identificationKind.Value;
                    }

                    var gender = EnumUtils.ToEnum<Gender>(src["gender"].ToString());
                    if (gender != null)
                    {
                        dest.Gender = gender.Value;
                    }
                    if (src["customer"]["kind"].ToString().EndsWith("resident"))
                    {
                        dest.ResidentialStatus = src["customer"]["kind"].ToString();
                    }
                    var mobilePhone = cp.Where(x => x["method"].ToString().Equals("gsm") && x["usage"].ToString().Equals("default")).FirstOrDefault();
                    if (mobilePhone != null)
                    {
                        dest.MobilePhone = mobilePhone["address"]["formatted"].ToString();
                    }
                    var homePhoneNumber = cp.Where(x => x["method"].ToString().Equals("pstn") && x["usage"].ToString().Equals("default")).FirstOrDefault();
                    if (homePhoneNumber != null)
                    {
                        dest.HomePhoneNumber = homePhoneNumber["address"]["formatted"].ToString();
                    }
                    var email = cp.Where(x => x["method"].ToString().Equals("email") && x["usage"].ToString().Equals("default")).FirstOrDefault();
                    if (email != null)
                    {
                        dest.EmailAddress = email["address"]["formatted"].ToString();
                    }
                    if (src["contact-preference"] != null && src["contact-preference"].HasValues)
                    {
                        dest.PreferredCulture = src["contact-preference"]?["preferred-language"]?.ToString() ?? "sr-Latn-RS";
                    }
                    else { dest.PreferredCulture = "sr-Latn-RS"; }

                    var employmentData = new EmploymentData();

                    //var employmentStatus = EnumUtils.ToEnum<EmploymentStatus>(src["employment-status"]?.ToString());
                    //if (employmentStatus != null)
                    //{
                    //    employmentData.EmploymentStatus = employmentStatus.Value;
                    //}

                    var employmentStatus = src["employment-status"]?.ToString();
                    if (employmentStatus != null)
                    {
                        employmentData.EmploymentStatus = employmentStatus;
                    };


                    var employmentDataSource = src["employment"];
                    if (employmentDataSource != null && employmentDataSource.HasValues)
                    {
                        var employmentInfo = new EmploymentInfo();
                        var employer = src["employment"]["employer"]?.ToString();

                        var employerId = src["employment"]["employer-id"]?.ToString();

                        var positionCategory = src["employment"]["employment-position"]?.ToString();
                        if (!string.IsNullOrEmpty(employer))
                        {
                            employmentInfo.EmployerName = employer;
                        }
                        if (!string.IsNullOrEmpty(employerId))
                        {
                            employmentInfo.CompanyIdNumber = employerId;
                        }
                        if (!string.IsNullOrEmpty(positionCategory))
                        {
                            employmentInfo.PositionCategory = positionCategory;
                        }
                        employmentData.Employments = new List<EmploymentInfo> { employmentInfo };

                        var employmentDateString = src["employment"]["employment-date"]?.ToString();
                        if (!string.IsNullOrEmpty(employmentDateString))
                        {
                            var employmentDate = DateTime.Parse(employmentDateString);
                            employmentData.EmploymentStatusDate = employmentDate;
                            employmentInfo.EmploymentStartDate = employmentDate;
                            // employmentData.Employments = new List<EmploymentInfo> { employmentInfo };
                            var periodBuilder = new PeriodBuilder();
                            var durationStruct = periodBuilder.ToString(new TimeSpan(Utility.DaysBetween(employmentDate) * 24, 0, 0));
                            // employmentData.TotalWorkPeriod = durationStruct;
                            int numberOfYears = 0;
                            numberOfYears = Years(employmentDate, DateTime.Now);

                            int Years(DateTime start, DateTime end)
                            {
                                return (end.Year - start.Year - 1) +
                                    (((end.Month > start.Month) ||
                                    ((end.Month == start.Month) && (end.Day >= start.Day))) ? 1 : 0);
                            };
                            var dateForMonths = employmentDate.AddYears(numberOfYears);
                            //var durationMonths = periodBuilder.ToString(new TimeSpan(Utility.MonthsBetween(dateForMonths) * 24, 0, 0));
                            int numberOfMonths = 0;
                            numberOfMonths = GetMonthsBetween(dateForMonths, DateTime.Now);
                            var dateForDays = dateForMonths.AddMonths(numberOfMonths);
                            int numberOfDays = 0;
                            numberOfDays = Utility.DaysBetween(dateForDays);

                            int GetMonthsBetween(DateTime from, DateTime to)
                            {
                                if (from > to) return GetMonthsBetween(to, from);

                                var monthDiff = Math.Abs((to.Year * 12 + (to.Month - 1)) - (from.Year * 12 + (from.Month - 1)));

                                if (from.AddMonths(monthDiff) > to || to.Day < from.Day)
                                {
                                    return monthDiff - 1;
                                }
                                else
                                {
                                    return monthDiff;
                                }
                            }


                            employmentData.TotalWorkPeriod = "P" + numberOfYears + "Y" + numberOfMonths + "M" + numberOfDays + "D";

                        }
                    }
                    dest.EmploymentData = employmentData;


                    try
                    {
                        var relationships = ((JArray)src["relationships"]).ToList();

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
                    catch
                    {
                        // Just ignore it
                    }

                    var ids = ((JArray)src["id-documents"])?.ToList();
                    if (ids != null && ids.Count > 0)
                    {
                        IdentificationDocument idDocument = new IdentificationDocument
                        {
                            Kind = ids[0]["kind"]?.ToString(),
                            SerialNumber = ids[0]["serial-number"]?.ToString(),
                            IssuedDate = DateTime.Parse(ids[0]["issued"]?.ToString()),
                            ValidUntil = DateTime.Parse(ids[0]["valid-until"]?.ToString()),
                            Status = ids[0]["status"]?.ToString(),
                            PlaceOfIssue = ids[0]["place-of-issue"]?.ToString(),
                            IssuingAuthority = ids[0]["issuing-authority"]?.ToString(),
                            ContentUrls = ids[0]["content-url"]?.ToString()
                        };
                        dest.IdentificationDocument = idDocument;
                    }
                });
        }
    }
}
