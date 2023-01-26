using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace domain.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "tbl_feeds",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    image = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    link = table.Column<string>(type: "TEXT", nullable: false),
                    lastupdate = table.Column<DateTime>(name: "last_update", type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_feeds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "TEXT", nullable: false),
                    password = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_posts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    link = table.Column<string>(type: "TEXT", nullable: false),
                    publicationdate = table.Column<DateTime>(name: "publication_date", type: "TEXT", nullable: false),
                    AuthorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_posts", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_posts_tbl_feeds_AuthorId",
                        column: x => x.AuthorId,
                        principalSchema: "public",
                        principalTable: "tbl_feeds",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tbl_subscriptions",
                schema: "public",
                columns: table => new
                {
                    userid = table.Column<int>(name: "user_id", type: "INTEGER", nullable: false),
                    feedid = table.Column<int>(name: "feed_id", type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_subscriptions", x => new { x.userid, x.feedid });
                    table.ForeignKey(
                        name: "FK_tbl_subscriptions_tbl_feeds_feed_id",
                        column: x => x.feedid,
                        principalSchema: "public",
                        principalTable: "tbl_feeds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_subscriptions_tbl_users_user_id",
                        column: x => x.userid,
                        principalSchema: "public",
                        principalTable: "tbl_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_posts_AuthorId",
                schema: "public",
                table: "tbl_posts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_subscriptions_feed_id",
                schema: "public",
                table: "tbl_subscriptions",
                column: "feed_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_posts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tbl_subscriptions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tbl_feeds",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tbl_users",
                schema: "public");
        }
    }
}
