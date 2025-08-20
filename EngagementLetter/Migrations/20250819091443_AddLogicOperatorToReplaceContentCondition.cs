using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EngagementLetter.Migrations
{
    /// <inheritdoc />
    public partial class AddLogicOperatorToReplaceContentCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConditionType",
                table: "ReplaceContentConditions",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConditionType",
                table: "ReplaceContentConditions");
        }
    }
}
