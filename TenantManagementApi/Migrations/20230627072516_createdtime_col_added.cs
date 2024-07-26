using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TenantManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class createdtime_col_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatedDateTime",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatedDateTime",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedDateTime",
                table: "Groups");
        }
    }
}
