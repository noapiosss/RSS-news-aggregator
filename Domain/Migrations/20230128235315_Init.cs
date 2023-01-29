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
            migrationBuilder.CreateTable(
                name: "tbl_feeds",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    link = table.Column<string>(type: "TEXT", nullable: false),
                    author = table.Column<string>(type: "TEXT", nullable: true),
                    language = table.Column<string>(type: "TEXT", nullable: true),
                    copyright = table.Column<string>(type: "TEXT", nullable: true),
                    category = table.Column<string>(type: "TEXT", nullable: true),
                    generator = table.Column<string>(type: "TEXT", nullable: true),
                    docs = table.Column<string>(type: "TEXT", nullable: true),
                    ttl = table.Column<string>(type: "TEXT", nullable: true),
                    image = table.Column<string>(type: "TEXT", nullable: true),
                    textinputtitle = table.Column<string>(name: "text_input_title", type: "TEXT", nullable: true),
                    textinputdescription = table.Column<string>(name: "text_input_description", type: "TEXT", nullable: true),
                    textinputname = table.Column<string>(name: "text_input_name", type: "TEXT", nullable: true),
                    textinputlink = table.Column<string>(name: "text_input_link", type: "TEXT", nullable: true),
                    skiphours = table.Column<string>(name: "skip_hours", type: "TEXT", nullable: true),
                    skipdays = table.Column<string>(name: "skip_days", type: "TEXT", nullable: true),
                    lastupdate = table.Column<DateTime>(name: "last_update", type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_feeds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_users",
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
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    publicationdate = table.Column<DateTime>(name: "publication_date", type: "TEXT", nullable: false),
                    category = table.Column<string>(type: "TEXT", nullable: true),
                    guid = table.Column<string>(type: "TEXT", nullable: true),
                    source = table.Column<string>(type: "TEXT", nullable: true),
                    link = table.Column<string>(type: "TEXT", nullable: true),
                    feedid = table.Column<int>(name: "feed_id", type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_posts", x => x.id);
                    table.ForeignKey(
                        name: "FK_tbl_posts_tbl_feeds_feed_id",
                        column: x => x.feedid,
                        principalTable: "tbl_feeds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_subscriptions",
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
                        principalTable: "tbl_feeds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_subscriptions_tbl_users_user_id",
                        column: x => x.userid,
                        principalTable: "tbl_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_read_posts",
                columns: table => new
                {
                    userid = table.Column<int>(name: "user_id", type: "INTEGER", nullable: false),
                    postid = table.Column<int>(name: "post_id", type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_read_posts", x => new { x.userid, x.postid });
                    table.ForeignKey(
                        name: "FK_tbl_read_posts_tbl_posts_post_id",
                        column: x => x.postid,
                        principalTable: "tbl_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_read_posts_tbl_users_user_id",
                        column: x => x.userid,
                        principalTable: "tbl_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_posts_feed_id",
                table: "tbl_posts",
                column: "feed_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_read_posts_post_id",
                table: "tbl_read_posts",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_subscriptions_feed_id",
                table: "tbl_subscriptions",
                column: "feed_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_read_posts");

            migrationBuilder.DropTable(
                name: "tbl_subscriptions");

            migrationBuilder.DropTable(
                name: "tbl_posts");

            migrationBuilder.DropTable(
                name: "tbl_users");

            migrationBuilder.DropTable(
                name: "tbl_feeds");
        }
    }
}
