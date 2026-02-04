using ITTicketingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ITTicketingSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        public static async Task EnsureDatabaseCreated(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            
            // Seed initial data if needed
            if (!await context.Users.AnyAsync())
            {
                await SeedInitialData(context);
            }
        }

        private static async Task SeedInitialData(ApplicationDbContext context)
        {
            // Add admin user
            var adminUser = new User
            {
                Email = "admin@itticketing.com",
                PasswordHash = "Admin123!", // In production, this should be hashed
                FirstName = "Admin",
                LastName = "User",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(adminUser);

            // Add manager user
            var managerUser = new User
            {
                Email = "manager@itticketing.com",
                PasswordHash = "Manager123!", // In production, this should be hashed
                FirstName = "Alex",
                LastName = "Rivera",
                Role = "Manager",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(managerUser);

            // Add sample users
            var sampleUsers = new[]
            {
                new User { Email = "john.doe@company.com", PasswordHash = "Password123!", FirstName = "John", LastName = "Doe", Role = "User", IsActive = true, CreatedAt = DateTime.Now },
                new User { Email = "jane.smith@company.com", PasswordHash = "Password123!", FirstName = "Jane", LastName = "Smith", Role = "User", IsActive = true, CreatedAt = DateTime.Now },
                new User { Email = "mike.wilson@company.com", PasswordHash = "Password123!", FirstName = "Mike", LastName = "Wilson", Role = "Support", IsActive = true, CreatedAt = DateTime.Now }
            };
            context.Users.AddRange(sampleUsers);

            await context.SaveChangesAsync();

            // Add sample tickets
            var sampleTickets = new[]
            {
                new Ticket 
                { 
                    Subject = "VPN Access Issue - London Branch", 
                    Description = "Cannot connect to VPN from London office. Getting authentication error.", 
                    Priority = "Critical", 
                    Status = "Open", 
                    Department = "Engineering", 
                    Category = "Network", 
                    CreatedById = adminUser.Id,
                    CreatedAt = DateTime.Now
                },
                new Ticket 
                { 
                    Subject = "Outlook License Renewal", 
                    Description = "Need to renew Outlook license for new employees.", 
                    Priority = "Medium", 
                    Status = "In-Progress", 
                    Department = "HR", 
                    Category = "Software", 
                    CreatedById = managerUser.Id,
                    CreatedAt = DateTime.Now
                },
                new Ticket 
                { 
                    Subject = "New Workstation Setup (HR)", 
                    Description = "Setup new workstation for HR department.", 
                    Priority = "Low", 
                    Status = "Resolved", 
                    Department = "HR", 
                    Category = "Hardware", 
                    CreatedById = 3,
                    CreatedAt = DateTime.Now
                },
                new Ticket 
                { 
                    Subject = "Database Connectivity Error", 
                    Description = "Application cannot connect to production database.", 
                    Priority = "High", 
                    Status = "Open", 
                    Department = "Engineering", 
                    Category = "Software", 
                    CreatedById = adminUser.Id,
                    CreatedAt = DateTime.Now
                }
            };
            context.Tickets.AddRange(sampleTickets);

            await context.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("User");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(50).HasDefaultValue("Medium");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Open");
                entity.Property(e => e.Department).HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.WorkLocation).HasMaxLength(50);
                entity.Property(e => e.TeamViewerId).HasMaxLength(100);
                entity.Property(e => e.TeamViewerPassword).HasMaxLength(100);
                entity.Property(e => e.ContactNumber).HasMaxLength(50);
                entity.Property(e => e.ContactEmail).HasMaxLength(255);
                entity.Property(e => e.Attachments).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.AssignedTo)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedToId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
