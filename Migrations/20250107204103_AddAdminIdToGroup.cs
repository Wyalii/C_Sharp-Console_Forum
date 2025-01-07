using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleForum.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminIdToGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Groups");
        }
    }
}
