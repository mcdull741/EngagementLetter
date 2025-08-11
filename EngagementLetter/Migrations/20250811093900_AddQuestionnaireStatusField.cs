using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EngagementLetter.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaireStatusField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Questionnaires",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Questionnaires",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Questionnaires",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Questionnaires");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Questionnaires");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Questionnaires");
        }
    }
}
