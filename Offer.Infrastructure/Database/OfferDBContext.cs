using Microsoft.EntityFrameworkCore;
using Offer.Domain.AggregatesModel.ApplicationAggregate;
using Offer.Infrastructure.EntityConfigurations;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceCommon.Domain.SeedWork;
using MicroserviceCommon.Infrastructure.Migrations;

namespace Offer.Infrastructure
{
    public class ConfigDesignTimeServices : CustomDesignTimeServices
    {

    }
    public class OfferDBContext : BaseDbContext, IUnitOfWork
    {
        public DbSet<Application> Applications { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<IndividualParty> IndividualParties { get; set; }
        public DbSet<OrganizationParty> OrganizationParties { get; set; }
        public DbSet<ArrangementRequest> ArrangementRequests { get; set; }
        public DbSet<CreditCardFacilityRequest> CreditCardFacilityRequest { get; set; }
        public DbSet<ApplicationDocument> ApplicationDocuments { get; set; }
        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<CollateralRequirement> CollateralRequirements { get; set; }
        public DbSet<PortfolioChangeRequests> PortfolioChangeRequests { get; set; }
        public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
        public DbSet<ProductSnapshotDb> ProductSnapshots { get; set; }


        public OfferDBContext()
        {

        }
        public OfferDBContext(DbContextOptions<OfferDBContext> options) : base(options)
        {

        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {

            // After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
            // performed throught the DbContext will be commited
            await base.SaveChangesAsync();
            return true;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Party>()
                .HasDiscriminator<PartyKind>("PartyKind")
                .HasValue<IndividualParty>(PartyKind.Individual)
                .HasValue<OrganizationParty>(PartyKind.Organization);

            modelBuilder.Entity<CreditCardFacilityRequest>().OwnsOne(
               o => o.MinimalRepaymentAmount,
               sa =>
               {
                   sa.Property(p => p.Amount).HasColumnName("MinRepAmount_Amount");
                   sa.Property(p => p.Code).HasColumnName("MinRepAmount_Code");
               });
            modelBuilder.Entity<CreditFacilityRequest>().OwnsOne(
               o => o.MinimalRepaymentAmount,
               sa =>
               {
                   sa.Property(p => p.Amount).HasColumnName("MinRepAmount_Amount");
                   sa.Property(p => p.Code).HasColumnName("MinRepAmount_Code");
               });
            modelBuilder.Entity<PortfolioChangeRequests>().ToTable("portfolio_change_requests");
            modelBuilder.ApplyConfiguration(new ApplicationDocumentEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PartyEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new IndividualEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ArrangementRequestEntityTypeConfiguration());
            modelBuilder.Entity<DemandDepositRequest>().OwnsOne(x => x.SavingsPlan);
            modelBuilder.Entity<TermLoanRequest>().Property(x => x.IsRefinancing).HasDefaultValue(false);
            modelBuilder.ApplyConfiguration(new FinancialQuestionnaireEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new GenericQuestionnaireEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new QuestionnaireEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ClientRequestEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CollateralRequirementsEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationUnitEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ProductSnapshotEntityTypeConfiguration());

            // modelBuilder.ApplyConfiguration(new ArrangementRequestSnapshotEntityTypeConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}