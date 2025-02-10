using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASAssignment1.Migrations
{
    /// <inheritdoc />
    public partial class AddemailtoPasswordHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "PasswordHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "PasswordHistories");
        }
    }
}
