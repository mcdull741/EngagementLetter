using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EngagementLetter.Migrations
{
    /// <inheritdoc />
    public partial class AddReplaceContentModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReplaceContents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionnaireId = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    BindQuestion = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplaceContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReplaceContents_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReplaceContentConditions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionnaireId = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<string>(type: "TEXT", nullable: false),
                    ReplaceContentId = table.Column<string>(type: "TEXT", nullable: false),
                    TextResponse = table.Column<string>(type: "TEXT", nullable: false),
                    LogicOperator = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: "AND"),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplaceContentConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReplaceContentConditions_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReplaceContentConditions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReplaceContentConditions_ReplaceContents_ReplaceContentId",
                        column: x => x.ReplaceContentId,
                        principalTable: "ReplaceContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReplaceContentConditions_QuestionId",
                table: "ReplaceContentConditions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplaceContentConditions_QuestionnaireId",
                table: "ReplaceContentConditions",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplaceContentConditions_ReplaceContentId",
                table: "ReplaceContentConditions",
                column: "ReplaceContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplaceContents_Key",
                table: "ReplaceContents",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_ReplaceContents_QuestionnaireId",
                table: "ReplaceContents",
                column: "QuestionnaireId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReplaceContentConditions");

            migrationBuilder.DropTable(
                name: "ReplaceContents");
        }
    }
}
