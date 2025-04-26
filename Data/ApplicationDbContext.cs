using Microsoft.EntityFrameworkCore;
using JobDescriptionAgent.Models;
using System.Text.Json;

namespace JobDescriptionAgent.Data
{
    /// <summary>
    /// Entity Framework Core database context for the application.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by the DbContext.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the saved job descriptions table.
        /// </summary>
        public DbSet<SavedJobDescription> SavedJobDescriptions { get; set; }

        /// <summary>
        /// Configures the model and relationships for the context.
        /// </summary>
        /// <param name="modelBuilder">The model builder to be used for configuration.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SavedJobDescription>()
                .Property(s => s.Stages)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null)
                );
        }
    }
} 