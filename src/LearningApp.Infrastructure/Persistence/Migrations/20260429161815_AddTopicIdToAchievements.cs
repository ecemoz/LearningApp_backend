using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearningApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicIdToAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TopicId",
                table: "Achievements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_TopicId",
                table: "Achievements",
                column: "TopicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Achievements_Topics_TopicId",
                table: "Achievements",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Achievements_Topics_TopicId",
                table: "Achievements");

            migrationBuilder.DropIndex(
                name: "IX_Achievements_TopicId",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "Achievements");
        }
    }
}
