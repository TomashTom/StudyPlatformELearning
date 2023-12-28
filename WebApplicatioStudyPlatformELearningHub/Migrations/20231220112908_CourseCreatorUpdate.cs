using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlatformELearningHub.Migrations
{
    public partial class CourseCreatorUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorFullName",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorFullName",
                table: "Courses");
        }
    }
}
