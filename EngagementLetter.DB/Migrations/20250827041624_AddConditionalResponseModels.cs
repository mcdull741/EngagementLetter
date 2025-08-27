using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EngagementLetter.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddConditionalResponseModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Questionnaires",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questionnaires", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionnaireId = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    OptionsJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReplaceContents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionnaireId = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
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
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionnaireId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    TemplatePath = table.Column<string>(type: "TEXT", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 50),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Templates_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConditionalResponses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionnaireId = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<string>(type: "TEXT", nullable: false),
                    Response = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionalResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConditionalResponses_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConditionalResponses_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserResponses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<string>(type: "TEXT", nullable: false),
                    EngLetterId = table.Column<string>(type: "TEXT", nullable: false),
                    TextResponse = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserResponses_EngLetters_EngLetterId",
                        column: x => x.EngLetterId,
                        principalTable: "EngLetters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserResponses_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    ConditionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "TemplateConditions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionnaireId = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<string>(type: "TEXT", nullable: false),
                    TemplateId = table.Column<string>(type: "TEXT", nullable: false),
                    TextResponse = table.Column<string>(type: "TEXT", nullable: false),
                    ConditionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "Equals"),
                    LogicOperator = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: "AND"),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateConditions_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemplateConditions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemplateConditions_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConditionalResponseConditions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ConditionalResponseId = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionId = table.Column<string>(type: "TEXT", nullable: false),
                    QuestionnaireId = table.Column<string>(type: "TEXT", nullable: false),
                    TextResponse = table.Column<string>(type: "TEXT", nullable: false),
                    ConditionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LogicOperator = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionalResponseConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConditionalResponseConditions_ConditionalResponses_ConditionalResponseId",
                        column: x => x.ConditionalResponseId,
                        principalTable: "ConditionalResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConditionalResponseConditions_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConditionalResponseConditions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalResponseConditions_ConditionalResponseId",
                table: "ConditionalResponseConditions",
                column: "ConditionalResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalResponseConditions_QuestionId",
                table: "ConditionalResponseConditions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalResponseConditions_QuestionnaireId",
                table: "ConditionalResponseConditions",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalResponses_QuestionId",
                table: "ConditionalResponses",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConditionalResponses_QuestionnaireId",
                table: "ConditionalResponses",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_EngLetters_QuestionnaireId",
                table: "EngLetters",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuestionnaireId",
                table: "Questions",
                column: "QuestionnaireId");

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

            migrationBuilder.CreateIndex(
                name: "IX_TemplateConditions_QuestionId",
                table: "TemplateConditions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateConditions_QuestionnaireId",
                table: "TemplateConditions",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateConditions_TemplateId",
                table: "TemplateConditions",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_CreatedDate",
                table: "Templates",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Priority",
                table: "Templates",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_QuestionnaireId",
                table: "Templates",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_EngLetterId",
                table: "UserResponses",
                column: "EngLetterId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_QuestionId",
                table: "UserResponses",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConditionalResponseConditions");

            migrationBuilder.DropTable(
                name: "ReplaceContentConditions");

            migrationBuilder.DropTable(
                name: "TemplateConditions");

            migrationBuilder.DropTable(
                name: "UserResponses");

            migrationBuilder.DropTable(
                name: "ConditionalResponses");

            migrationBuilder.DropTable(
                name: "ReplaceContents");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "EngLetters");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Questionnaires");
        }
    }
}
