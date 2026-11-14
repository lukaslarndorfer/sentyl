using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sentyl.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildConfig",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "INTEGER", nullable: false),
                    ChannelId = table.Column<long>(type: "INTEGER", nullable: false),
                    UntisHost = table.Column<string>(type: "TEXT", nullable: false),
                    UntisSchool = table.Column<string>(type: "TEXT", nullable: false),
                    UntisUser = table.Column<string>(type: "TEXT", nullable: false),
                    UntisPasswordEncrypted = table.Column<string>(type: "TEXT", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildConfig", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "GuildTimetableState",
                columns: table => new
                {
                    GuildId = table.Column<long>(type: "INTEGER", nullable: false),
                    Lessons = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildTimetableState", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_GuildTimetableState_GuildConfig_GuildId",
                        column: x => x.GuildId,
                        principalTable: "GuildConfig",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildTimetableState");

            migrationBuilder.DropTable(
                name: "GuildConfig");
        }
    }
}
