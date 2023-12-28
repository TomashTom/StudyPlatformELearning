using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlatformELearningHub.Migrations
{
    public partial class AddVideos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoFiles_Courses_CourseId",
                table: "VideoFiles");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoFiles_Courses_CourseId",
                table: "VideoFiles",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VideoFiles_Courses_CourseId",
                table: "VideoFiles");

            migrationBuilder.AddForeignKey(
                name: "FK_VideoFiles_Courses_CourseId",
                table: "VideoFiles",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId");
        }
    }
}
