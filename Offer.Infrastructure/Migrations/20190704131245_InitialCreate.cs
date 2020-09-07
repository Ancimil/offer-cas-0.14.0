using System;
using MicroserviceCommon.Infrastructure.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Offer.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        private readonly IDbContextSchema _schema;

        public InitialCreate(IDbContextSchema schema)
        {
            _schema = schema;
        }
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: _schema.Schema);

            migrationBuilder.CreateTable(
                name: "applications",
                schema: _schema.Schema,
                columns: table => new
                {
                    ApplicationId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExposureInfo = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Phase = table.Column<string>(nullable: true),
                    ProductCode = table.Column<string>(maxLength: 128, nullable: true),
                    ProductName = table.Column<string>(maxLength: 128, nullable: true),
                    CustomerNumber = table.Column<string>(maxLength: 256, nullable: true),
                    CustomerName = table.Column<string>(maxLength: 256, nullable: true),
                    CustomerSegment = table.Column<string>(maxLength: 256, nullable: true),
                    OrganizationUnitId = table.Column<string>(maxLength: 64, nullable: true),
                    ChannelCode = table.Column<string>(maxLength: 128, nullable: true),
                    PortfolioId = table.Column<string>(maxLength: 1024, nullable: true),
                    CampaignCode = table.Column<string>(maxLength: 128, nullable: true),
                    DecisionNumber = table.Column<string>(maxLength: 256, nullable: true),
                    SettlementAccount = table.Column<string>(maxLength: 256, nullable: true),
                    ArrangementNumber = table.Column<string>(maxLength: 128, nullable: true),
                    Initiator = table.Column<string>(maxLength: 128, nullable: true),
                    CountryCode = table.Column<string>(maxLength: 128, nullable: true),
                    PrefferedCulture = table.Column<string>(maxLength: 128, nullable: true),
                    SigningOption = table.Column<string>(maxLength: 256, nullable: true),
                    CollateralModel = table.Column<string>(maxLength: 256, nullable: true),
                    RiskScore = table.Column<decimal>(nullable: true),
                    CreditRating = table.Column<string>(nullable: true),
                    CustomerValue = table.Column<decimal>(nullable: true),
                    StatusInformation_Description = table.Column<string>(maxLength: 1024, nullable: true),
                    StatusInformation_Title = table.Column<string>(nullable: true),
                    StatusInformation_Html = table.Column<string>(nullable: true),
                    RequestDate = table.Column<DateTime>(nullable: true),
                    ExpirationDate = table.Column<DateTime>(nullable: true),
                    CancelationReason = table.Column<string>(nullable: true),
                    CancelationComment = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: true),
                    StatusChangeDate = table.Column<DateTime>(nullable: true),
                    LastModified = table.Column<DateTime>(nullable: true),
                    CreatedByName = table.Column<string>(nullable: true),
                    RequestedActivationDate = table.Column<DateTime>(nullable: true),
                    Comments = table.Column<string>(nullable: true),
                    TermLimitBreached = table.Column<bool>(nullable: false),
                    AmountLimitBreached = table.Column<bool>(nullable: false),
                    PreferencialPrice = table.Column<bool>(nullable: false),
                    LoanToValue = table.Column<decimal>(nullable: true),
                    MaximalAnnuity = table.Column<decimal>(nullable: true),
                    MaximalAmount = table.Column<decimal>(nullable: true),
                    DebtToIncome = table.Column<decimal>(nullable: true),
                    CustomerRemainingAbilityToPay = table.Column<decimal>(nullable: true),
                    EffectiveRemainingAbilityToPay = table.Column<decimal>(nullable: true),
                    IsPreApproved = table.Column<bool>(nullable: false),
                    PreApprovalType = table.Column<string>(nullable: true),
                    OriginatesBundle = table.Column<bool>(nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.ApplicationId);
                });

            migrationBuilder.CreateTable(
                name: "requests",
                schema: _schema.Schema,
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    Time = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "arrangement_requests",
                schema: _schema.Schema,
                columns: table => new
                {
                    ArrangementRequestId = table.Column<int>(nullable: false),
                    ApplicationId = table.Column<long>(nullable: false),
                    ProductCode = table.Column<string>(maxLength: 128, nullable: false),
                    ParentProductCode = table.Column<string>(nullable: true),
                    ProductName = table.Column<string>(maxLength: 128, nullable: false),
                    ArrangementKind = table.Column<int>(nullable: false),
                    ArrangementNumber = table.Column<string>(maxLength: 128, nullable: true),
                    Enabled = table.Column<bool>(nullable: true, defaultValue: true),
                    ProductSnapshot = table.Column<string>(nullable: true),
                    ArrangementRequestCondition = table.Column<string>(nullable: true),
                    Accounts = table.Column<string>(nullable: true),
                    CalculationDate = table.Column<DateTime>(nullable: true),
                    NumberOfInstallments = table.Column<int>(nullable: false),
                    InstallmentPlan = table.Column<string>(nullable: true),
                    _Campaign = table.Column<string>(nullable: true),
                    _Options = table.Column<string>(nullable: true),
                    BundleInfo = table.Column<string>(nullable: true),
                    OverrideProductLimits = table.Column<bool>(nullable: false),
                    _RequestedValues = table.Column<string>(nullable: true),
                    _ApprovedLimits = table.Column<string>(nullable: true),
                    _AcceptedValues = table.Column<string>(nullable: true),
                    IsAbstractOrigin = table.Column<bool>(nullable: false),
                    IsOptional = table.Column<bool>(nullable: true, defaultValue: true),
                    Discriminator = table.Column<string>(maxLength: 256, nullable: false),
                    Currency = table.Column<string>(nullable: true),
                    Eapr = table.Column<decimal>(nullable: true),
                    Napr = table.Column<decimal>(nullable: true),
                    SavingsPlan_Iteration = table.Column<string>(nullable: true),
                    SavingsPlan_Amount = table.Column<decimal>(nullable: true),
                    TermDepositRequest_Amount = table.Column<decimal>(nullable: true),
                    TermDepositRequest_MaturityDate = table.Column<DateTime>(nullable: true),
                    TermDepositRequest_Term = table.Column<string>(nullable: true),
                    RolloverOption = table.Column<int>(nullable: true),
                    MaxRollovers = table.Column<int>(nullable: true),
                    SavingsPlan_Iteration1 = table.Column<string>(nullable: true),
                    SavingsPlan_Amount1 = table.Column<decimal>(nullable: true),
                    InterestCapOnRollover = table.Column<int>(nullable: true),
                    Amount = table.Column<decimal>(nullable: true),
                    FinanceServiceArrangementRequest_Currency = table.Column<string>(nullable: true),
                    AmountInDomesticCurrency = table.Column<decimal>(nullable: true),
                    MaturityDate = table.Column<DateTime>(nullable: true),
                    FinanceServiceArrangementRequest_Eapr = table.Column<decimal>(nullable: true),
                    FinanceServiceArrangementRequest_Napr = table.Column<decimal>(nullable: true),
                    Term = table.Column<string>(nullable: true),
                    CollateralModel = table.Column<string>(nullable: true),
                    _AlternativeOffers = table.Column<string>(nullable: true),
                    LoanToValue = table.Column<decimal>(nullable: true),
                    MaximalAnnuity = table.Column<decimal>(nullable: true),
                    MaximalAmount = table.Column<decimal>(nullable: true),
                    MinRepAmount_Amount = table.Column<decimal>(nullable: true),
                    MinRepAmount_Code = table.Column<string>(nullable: true),
                    MinRepPerc = table.Column<decimal>(nullable: true),
                    RevPerc = table.Column<decimal>(nullable: true),
                    Annuity = table.Column<decimal>(nullable: true),
                    InvoiceAmount = table.Column<decimal>(nullable: true),
                    InvoiceAmountInLoanCurrency = table.Column<decimal>(nullable: true),
                    DownpaymentAmount = table.Column<decimal>(nullable: true),
                    DownpaymentInLoanCurrency = table.Column<decimal>(nullable: true),
                    DownpaymentPercentage = table.Column<decimal>(nullable: true),
                    GracePeriod = table.Column<string>(maxLength: 64, nullable: true),
                    GracePeriodStartDate = table.Column<DateTime>(nullable: true),
                    DrawdownPeriod = table.Column<string>(maxLength: 64, nullable: true),
                    DrawdownPeriodStartDate = table.Column<DateTime>(nullable: true),
                    RepaymentPeriod = table.Column<string>(maxLength: 64, nullable: true),
                    RepaymentPeriodStartDate = table.Column<DateTime>(nullable: true),
                    IsRefinancing = table.Column<bool>(nullable: true, defaultValue: false),
                    DisbursementsInfo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_arrangement_requests", x => new { x.ApplicationId, x.ArrangementRequestId });
                    table.ForeignKey(
                        name: "FK_arrangement_requests_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: _schema.Schema,
                        principalTable: "applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parties",
                schema: _schema.Schema,
                columns: table => new
                {
                    ApplicationId = table.Column<long>(nullable: false),
                    PartyId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(maxLength: 200, nullable: true),
                    CustomerNumber = table.Column<string>(maxLength: 200, nullable: true),
                    CustomerName = table.Column<string>(maxLength: 256, nullable: true),
                    EmailAddress = table.Column<string>(maxLength: 256, nullable: true),
                    PartyRole = table.Column<int>(nullable: false),
                    PartyKind = table.Column<int>(nullable: false),
                    LegalAddr_Kind = table.Column<int>(nullable: true),
                    LegalAddr_Formatted = table.Column<string>(maxLength: 1024, nullable: true),
                    LegalAddr_Street = table.Column<string>(maxLength: 256, nullable: true),
                    LegalAddr_StreetNumber = table.Column<string>(maxLength: 50, nullable: true),
                    LegalAddr_PostalCode = table.Column<string>(maxLength: 10, nullable: true),
                    LegalAddr_Locality = table.Column<string>(maxLength: 256, nullable: true),
                    LegalAddr_Country = table.Column<string>(maxLength: 256, nullable: true),
                    LegalAddr_AddressCode = table.Column<string>(maxLength: 256, nullable: true),
                    LegalAddr_Coordinates_Lat = table.Column<double>(nullable: true),
                    LegalAddr_Coordinates_Long = table.Column<double>(nullable: true),
                    ContactAddr_Kind = table.Column<int>(nullable: true),
                    ContactAddr_Formatted = table.Column<string>(maxLength: 1024, nullable: true),
                    ContactAddr_Street = table.Column<string>(maxLength: 256, nullable: true),
                    ContactAddr_StreetNumber = table.Column<string>(maxLength: 50, nullable: true),
                    ContactAddr_PostalCode = table.Column<string>(maxLength: 10, nullable: true),
                    ContactAddr_Locality = table.Column<string>(maxLength: 256, nullable: true),
                    ContactAddr_Country = table.Column<string>(maxLength: 256, nullable: true),
                    ContactAddr_AddressCode = table.Column<string>(maxLength: 256, nullable: true),
                    ContactAddr_Coordinates_Lat = table.Column<double>(nullable: true),
                    ContactAddr_Coordinates_Long = table.Column<double>(nullable: true),
                    PrimarySegment = table.Column<string>(maxLength: 100, nullable: true),
                    CustomerSegment = table.Column<string>(maxLength: 100, nullable: true),
                    CreditRating = table.Column<string>(nullable: true),
                    CustomerValue = table.Column<decimal>(nullable: true),
                    CountryOfResidence = table.Column<string>(nullable: true),
                    ProductUsageInfo = table.Column<string>(nullable: true),
                    IdentificationNumberKind = table.Column<int>(nullable: false),
                    IdentificationNumber = table.Column<string>(maxLength: 64, nullable: true),
                    IdentificationDocument = table.Column<string>(nullable: true),
                    ProfileImageUrl = table.Column<string>(nullable: true),
                    DebtToIncome = table.Column<decimal>(nullable: true),
                    RemainingAbilityToPay = table.Column<decimal>(nullable: true),
                    _CbData = table.Column<string>(nullable: true),
                    GivenName = table.Column<string>(maxLength: 256, nullable: true),
                    ParentName = table.Column<string>(maxLength: 256, nullable: true),
                    Surname = table.Column<string>(maxLength: 256, nullable: true),
                    MaidenName = table.Column<string>(maxLength: 256, nullable: true),
                    MothersMaidenName = table.Column<string>(maxLength: 256, nullable: true),
                    LifecycleStatus = table.Column<int>(nullable: true),
                    MobilePhone = table.Column<string>(maxLength: 256, nullable: true),
                    HomePhoneNumber = table.Column<string>(maxLength: 256, nullable: true),
                    Gender = table.Column<int>(nullable: true),
                    DateOfBirth = table.Column<DateTime>(nullable: true),
                    PlaceOfBirth = table.Column<string>(maxLength: 256, nullable: true),
                    ResidentialStatus = table.Column<string>(maxLength: 256, nullable: true),
                    ResidentialStatusDate = table.Column<DateTime>(maxLength: 256, nullable: true),
                    ResidentialAddressDate = table.Column<DateTime>(nullable: true),
                    MaritalStatus = table.Column<int>(nullable: true),
                    EducationLevel = table.Column<int>(nullable: true),
                    HomeOwnership = table.Column<int>(nullable: true),
                    CarOwnership = table.Column<int>(nullable: true),
                    Occupation = table.Column<string>(maxLength: 256, nullable: true),
                    EmploymentData = table.Column<string>(nullable: true),
                    FinancialProfile = table.Column<string>(nullable: true),
                    PreviousWorkPeriod = table.Column<string>(maxLength: 128, nullable: true),
                    HouseholdInfo = table.Column<string>(nullable: true),
                    RegisteredName = table.Column<string>(maxLength: 256, nullable: true),
                    CommercialName = table.Column<string>(nullable: true),
                    LegalStructure = table.Column<string>(nullable: true),
                    OrganizationPurpose = table.Column<string>(nullable: true),
                    IsSoleTrader = table.Column<bool>(nullable: true),
                    Established = table.Column<DateTime>(nullable: true),
                    IndustrySector = table.Column<string>(nullable: true),
                    OwnershipInfo = table.Column<string>(nullable: true),
                    Relationships = table.Column<string>(nullable: true),
                    BankAccounts = table.Column<string>(nullable: true),
                    IdNumbers = table.Column<string>(nullable: true),
                    Size = table.Column<string>(nullable: true),
                    FileKind = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(maxLength: 256, nullable: true),
                    LegalBasisForRegistration = table.Column<string>(nullable: true),
                    LegalStatus = table.Column<string>(nullable: true),
                    DocumentationStatus = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parties", x => new { x.ApplicationId, x.PartyId });
                    table.ForeignKey(
                        name: "FK_parties_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: _schema.Schema,
                        principalTable: "applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questionnaires",
                schema: _schema.Schema,
                columns: table => new
                {
                    ApplicationId = table.Column<long>(nullable: false),
                    QuestionnaireId = table.Column<string>(maxLength: 256, nullable: false),
                    Purpose = table.Column<string>(maxLength: 256, nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    QuestionnaireName = table.Column<string>(maxLength: 256, nullable: true),
                    Discriminator = table.Column<string>(maxLength: 256, nullable: false),
                    Entries = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questionnaires", x => new { x.ApplicationId, x.QuestionnaireId });
                    table.ForeignKey(
                        name: "FK_questionnaires_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: _schema.Schema,
                        principalTable: "applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "collateral_requirements",
                schema: _schema.Schema,
                columns: table => new
                {
                    CollateralRequirementId = table.Column<long>(nullable: false),
                    ApplicationId = table.Column<long>(nullable: false),
                    ArrangementRequestId = table.Column<int>(nullable: false),
                    CollateralArrangementCode = table.Column<string>(nullable: true),
                    FromModel = table.Column<bool>(nullable: false),
                    MinimalCoverage = table.Column<decimal>(nullable: false),
                    MinimalCoverageInLoanCurrency = table.Column<decimal>(nullable: false),
                    ActualCoverage = table.Column<decimal>(nullable: false),
                    SecuredDealLink = table.Column<string>(nullable: true),
                    CollateralOwner = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collateral_requirements", x => new { x.ApplicationId, x.ArrangementRequestId, x.CollateralRequirementId });
                    table.ForeignKey(
                        name: "FK_collateral_requirements_arrangement_requests_ApplicationId_ArrangementRequestId",
                        columns: x => new { x.ApplicationId, x.ArrangementRequestId },
                        principalSchema: _schema.Schema,
                        principalTable: "arrangement_requests",
                        principalColumns: new[] { "ApplicationId", "ArrangementRequestId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_documents",
                schema: _schema.Schema,
                columns: table => new
                {
                    DocumentId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ApplicationId = table.Column<long>(nullable: false),
                    DocumentContextKind = table.Column<int>(nullable: false),
                    Context = table.Column<string>(maxLength: 256, nullable: true),
                    ArrangementRequestId = table.Column<int>(maxLength: 256, nullable: true),
                    CollateralId = table.Column<string>(maxLength: 256, nullable: true),
                    PartyId = table.Column<long>(maxLength: 256, nullable: true),
                    DocumentName = table.Column<string>(maxLength: 256, nullable: true),
                    DocumentKind = table.Column<string>(maxLength: 256, nullable: true),
                    DocumentReviewPeriod = table.Column<string>(maxLength: 256, nullable: true),
                    IsMandatory = table.Column<bool>(nullable: false),
                    IsComposedFromTemplate = table.Column<bool>(nullable: false),
                    TemplateUrl = table.Column<string>(maxLength: 1024, nullable: true),
                    IsForSigning = table.Column<bool>(nullable: false),
                    IsForUpload = table.Column<bool>(nullable: false),
                    IsForPhysicalArchiving = table.Column<bool>(nullable: false),
                    IsInternal = table.Column<bool>(nullable: false),
                    SupportsMultipleFiles = table.Column<bool>(nullable: false),
                    Origin = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_documents", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_application_documents_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: _schema.Schema,
                        principalTable: "applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_documents_arrangement_requests_ApplicationId_ArrangementRequestId",
                        columns: x => new { x.ApplicationId, x.ArrangementRequestId },
                        principalSchema: _schema.Schema,
                        principalTable: "arrangement_requests",
                        principalColumns: new[] { "ApplicationId", "ArrangementRequestId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_application_documents_parties_ApplicationId_PartyId",
                        columns: x => new { x.ApplicationId, x.PartyId },
                        principalSchema: _schema.Schema,
                        principalTable: "parties",
                        principalColumns: new[] { "ApplicationId", "PartyId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_application_documents_ApplicationId_ArrangementRequestId",
                schema: _schema.Schema,
                table: "application_documents",
                columns: new[] { "ApplicationId", "ArrangementRequestId" });

            migrationBuilder.CreateIndex(
                name: "IX_application_documents_ApplicationId_PartyId",
                schema: _schema.Schema,
                table: "application_documents",
                columns: new[] { "ApplicationId", "PartyId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_documents",
                schema: _schema.Schema);

            migrationBuilder.DropTable(
                name: "collateral_requirements",
                schema: _schema.Schema);

            migrationBuilder.DropTable(
                name: "questionnaires",
                schema: _schema.Schema);

            migrationBuilder.DropTable(
                name: "requests",
                schema: _schema.Schema);

            migrationBuilder.DropTable(
                name: "parties",
                schema: _schema.Schema);

            migrationBuilder.DropTable(
                name: "arrangement_requests",
                schema: _schema.Schema);

            migrationBuilder.DropTable(
                name: "applications",
                schema: _schema.Schema);
        }
    }
}
