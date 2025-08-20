using Microsoft.EntityFrameworkCore;
using EngagementLetter.Models;

namespace EngagementLetter.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {}

        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<UserResponse> UserResponses { get; set; }
        public DbSet<EngLetter> EngLetters { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<TemplateCondition> TemplateConditions { get; set; }
        public DbSet<ReplaceContent> ReplaceContents { get; set; }
        public DbSet<ReplaceContentCondition> ReplaceContentConditions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Template配置
            modelBuilder.Entity<Template>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Priority).HasDefaultValue(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.QuestionnaireId);
                entity.HasIndex(e => e.CreatedDate);

                entity.HasOne(e => e.Questionnaire)
                      .WithMany()
                      .HasForeignKey(e => e.QuestionnaireId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // TemplateCondition配置
            modelBuilder.Entity<TemplateCondition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ConditionType).HasMaxLength(50).HasDefaultValue("Equals");
                entity.Property(e => e.LogicOperator).HasMaxLength(10).HasDefaultValue("AND");

                entity.Property(e => e.OrderIndex).HasDefaultValue(0);

                entity.HasIndex(e => e.TemplateId);
                entity.HasIndex(e => e.QuestionnaireId);
                entity.HasIndex(e => e.QuestionId);

                entity.HasOne(e => e.Template)
                      .WithMany(e => e.Conditions)
                      .HasForeignKey(e => e.TemplateId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Questionnaire)
                      .WithMany()
                      .HasForeignKey(e => e.QuestionnaireId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Question)
                      .WithMany()
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ReplaceContent配置
            modelBuilder.Entity<ReplaceContent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);


                entity.HasIndex(e => e.QuestionnaireId);
                entity.HasIndex(e => e.Key);

                entity.HasOne(e => e.Questionnaire)
                      .WithMany()
                      .HasForeignKey(e => e.QuestionnaireId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ReplaceContentCondition配置
            modelBuilder.Entity<ReplaceContentCondition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LogicOperator).HasMaxLength(10).HasDefaultValue("AND");
                entity.Property(e => e.OrderIndex).HasDefaultValue(0);

                entity.HasIndex(e => e.QuestionnaireId);
                entity.HasIndex(e => e.QuestionId);
                entity.HasIndex(e => e.ReplaceContentId);

                entity.HasOne(e => e.Questionnaire)
                      .WithMany()
                      .HasForeignKey(e => e.QuestionnaireId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Question)
                      .WithMany()
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReplaceContent)
                      .WithMany(e => e.Conditions)
                      .HasForeignKey(e => e.ReplaceContentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}