using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppStore.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppDescriptionEn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description_en",
                table: "apps",
                type: "text",
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description_en",
                table: "apps");
        }
    }
}
