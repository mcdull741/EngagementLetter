using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EngagementLetter.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBindQuestionFromReplaceContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BindQuestion",
                table: "ReplaceContents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BindQuestion",
                table: "ReplaceContents",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }
    }
}
