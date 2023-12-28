﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlatformELearningHub.Migrations
{
    public partial class VideoViewUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "VideoFiles");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "VideoViewModel",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "VideoViewModel");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "VideoFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
