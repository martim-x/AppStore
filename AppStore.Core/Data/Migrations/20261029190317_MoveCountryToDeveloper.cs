using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppStore.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveCountryToDeveloper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // переносим страну приложения разработчику, если она там не задана
            migrationBuilder.Sql(@"
                update developers d set country_id = a.country_id
                from apps a
                where a.developer_id = d.id and d.country_id is null and a.country_id is not null;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_apps_countries_country_id",
                table: "apps");

            migrationBuilder.DropIndex(
                name: "IX_apps_country_id",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "country_id",
                table: "apps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "country_id",
                table: "apps",
                type: "uuid",
                nullable: true);
            migrationBuilder.Sql(@"
                update apps a set country_id = d.country_id
                from developers d where d.id = a.developer_id;
            ");


            migrationBuilder.CreateIndex(
                name: "IX_apps_country_id",
                table: "apps",
                column: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_apps_countries_country_id",
                table: "apps",
                column: "country_id",
                principalTable: "countries",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
