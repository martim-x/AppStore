using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppStore.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncAppModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Текстовые колонки уже удалены миграцией NormalizeCatalog,
            // поэтому операция идемпотентна.
            migrationBuilder.Sql(@"
                alter table apps drop column if exists age_rating;
                alter table apps drop column if exists category;
                alter table apps drop column if exists country;
                alter table apps drop column if exists developer;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                alter table apps add column if not exists age_rating text not null default '';
                alter table apps add column if not exists category text not null default '';
                alter table apps add column if not exists country text not null default '';
                alter table apps add column if not exists developer text not null default '';
            ");
        }
    }
}
