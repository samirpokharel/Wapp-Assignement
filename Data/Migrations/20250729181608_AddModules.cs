using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleLMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                table: "Progresses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentPath",
                table: "Courses",
                type: "TEXT",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255);

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentType = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    VideoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PdfFilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modules_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Progresses_ModuleId",
                table: "Progresses",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_CourseId",
                table: "Modules",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_Modules_ModuleId",
                table: "Progresses",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_Modules_ModuleId",
                table: "Progresses");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Progresses_ModuleId",
                table: "Progresses");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "Progresses");

            migrationBuilder.AlterColumn<string>(
                name: "ContentPath",
                table: "Courses",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
