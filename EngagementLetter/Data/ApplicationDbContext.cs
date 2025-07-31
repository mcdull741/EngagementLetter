using Microsoft.EntityFrameworkCore;
using EngagementLetter.Models;

namespace EngagementLetter.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {}

    public DbSet<Questionnaire> Questionnaires { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<UserResponse> UserResponses { get; set; }
    public DbSet<EngLetter> EngLetters { get; set; }
}