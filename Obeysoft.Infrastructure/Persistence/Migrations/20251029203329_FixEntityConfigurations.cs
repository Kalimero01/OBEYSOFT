using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obeysoft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixEntityConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Posts",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "Posts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Posts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comments",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CategoryId_IsPublished_PublishedAt",
                table: "Posts",
                columns: new[] { "CategoryId", "IsPublished", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_IsPublished_PublishedAt",
                table: "Posts",
                columns: new[] { "IsPublished", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Slug",
                table: "Posts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentId",
                table: "Comments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostId_IsApproved_IsActive_CreatedAt",
                table: "Comments",
                columns: new[] { "PostId", "IsApproved", "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId_DisplayOrder_Name",
                table: "Categories",
                columns: new[] { "ParentId", "DisplayOrder", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParentId",
                table: "Categories",
                column: "ParentId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ParentId",
                table: "Comments",
                column: "ParentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Posts_PostId",
                table: "Comments",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Categories_CategoryId",
                table: "Posts",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ParentId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Posts_PostId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Categories_CategoryId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CategoryId_IsPublished_PublishedAt",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_IsPublished_PublishedAt",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_Slug",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ParentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_PostId_IsApproved_IsActive_CreatedAt",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentId_DisplayOrder_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Slug",
                table: "Categories");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Posts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(180)",
                oldMaxLength: 180);

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "Posts",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Posts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(180)",
                oldMaxLength: 180);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(160)",
                oldMaxLength: 160);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Categories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
