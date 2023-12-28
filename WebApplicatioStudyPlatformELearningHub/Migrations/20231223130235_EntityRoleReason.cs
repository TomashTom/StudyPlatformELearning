using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlatformELearningHub.Migrations
{
    public partial class EntityRoleReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "VideoFiles");

            migrationBuilder.AddColumn<string>(
                name: "BlockReason",
                table: "EntityRoles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockReason",
                table: "EntityRoles");

            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "VideoFiles",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
