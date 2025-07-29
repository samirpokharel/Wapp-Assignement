using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleLMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicsAndContentItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_Modules_ModuleId",
                table: "Progresses");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.RenameColumn(
                name: "ModuleId",
                table: "Progresses",
                newName: "TopicId");

            migrationBuilder.RenameIndex(
                name: "IX_Progresses_ModuleId",
                table: "Progresses",
                newName: "IX_Progresses_TopicId");

            migrationBuilder.AddColumn<int>(
                name: "ContentItemId",
                table: "Progresses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Topics_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TopicId = table.Column<int>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_ContentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentItems_Topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Progresses_ContentItemId",
                table: "Progresses",
                column: "ContentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentItems_TopicId",
                table: "ContentItems",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_CourseId",
                table: "Topics",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_ContentItems_ContentItemId",
                table: "Progresses",
                column: "ContentItemId",
                principalTable: "ContentItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_Topics_TopicId",
                table: "Progresses",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_ContentItems_ContentItemId",
                table: "Progresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_Topics_TopicId",
                table: "Progresses");

            migrationBuilder.DropTable(
                name: "ContentItems");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Progresses_ContentItemId",
                table: "Progresses");

            migrationBuilder.DropColumn(
                name: "ContentItemId",
                table: "Progresses");

            migrationBuilder.RenameColumn(
                name: "TopicId",
                table: "Progresses",
                newName: "ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_Progresses_TopicId",
                table: "Progresses",
                newName: "IX_Progresses_ModuleId");

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    PdfFilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    VideoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
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
    }
}
