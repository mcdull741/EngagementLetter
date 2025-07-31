using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EngagementLetter.Migrations
{
    /// <inheritdoc />
    public partial class RenameEngagementLetterToEngLetter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserResponses_EngagementLetters_EngagementLetterId",
                table: "UserResponses");

            migrationBuilder.DropTable(
                name: "EngagementLetters");

            migrationBuilder.RenameColumn(
                name: "EngagementLetterId",
                table: "UserResponses",
                newName: "EngLetterId");

            migrationBuilder.RenameIndex(
                name: "IX_UserResponses_EngagementLetterId",
                table: "UserResponses",
                newName: "IX_UserResponses_EngLetterId");

            migrationBuilder.CreateTable(
                name: "EngLetters",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    QuestionnaireId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngLetters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngLetters_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngLetters_QuestionnaireId",
                table: "EngLetters",
                column: "QuestionnaireId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserResponses_EngLetters_EngLetterId",
                table: "UserResponses",
                column: "EngLetterId",
                principalTable: "EngLetters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserResponses_EngLetters_EngLetterId",
                table: "UserResponses");

            migrationBuilder.DropTable(
                name: "EngLetters");

            migrationBuilder.RenameColumn(
                name: "EngLetterId",
                table: "UserResponses",
                newName: "EngagementLetterId");

            migrationBuilder.RenameIndex(
                name: "IX_UserResponses_EngLetterId",
                table: "UserResponses",
                newName: "IX_UserResponses_EngagementLetterId");

            migrationBuilder.CreateTable(
                name: "EngagementLetters",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionnaireId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngagementLetters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EngagementLetters_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EngagementLetters_QuestionnaireId",
                table: "EngagementLetters",
                column: "QuestionnaireId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserResponses_EngagementLetters_EngagementLetterId",
                table: "UserResponses",
                column: "EngagementLetterId",
                principalTable: "EngagementLetters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
