using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppStore.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnUserLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Идемпотентно: база, созданная EnsureCreated() из текущей модели,
            // уже содержит индекс IX_users_login — обычный CreateIndex() упал бы
            // с "already exists". IF NOT EXISTS делает миграцию безопасной
            // и для EnsureCreated-баз, и для baseline-баз (см. PostgreSQLRepEF).
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_users_login\" ON \"users\" (\"login\");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_users_login\";");
        }
    }
}
