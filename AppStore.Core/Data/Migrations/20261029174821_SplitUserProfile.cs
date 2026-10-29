using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppStore.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "profile_id",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    last_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    avatar_color = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    theme = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_user_profiles_user",
                table: "user_profiles",
                column: "user_id",
                unique: true);

            // перенос персональных данных и настроек в профили
            migrationBuilder.Sql(@"
                insert into user_profiles
                    (id, user_id, first_name, last_name, email, avatar_color, language, theme)
                select gen_random_uuid(), u.id, u.first_name, u.last_name, u.email,
                       coalesce(u.avatar_color, '#007AFF'), 'ru', 'light'
                from users u;

                update users u set profile_id = p.id
                from user_profiles p where p.user_id = u.id;
            ");

            migrationBuilder.DropColumn(name: "avatar_color", table: "users");
            migrationBuilder.DropColumn(name: "email", table: "users");
            migrationBuilder.DropColumn(name: "first_name", table: "users");
            migrationBuilder.DropColumn(name: "last_name", table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Восстанавливаем текстовые колонки и заполняем их из профилей,
            // затем убираем ссылку и таблицу профилей.
            migrationBuilder.AddColumn<string>(
                name: "first_name", table: "users", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "last_name", table: "users", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "email", table: "users", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "avatar_color", table: "users", type: "text", nullable: false,
                defaultValue: "#007AFF");

            migrationBuilder.Sql(@"
                update users u
                set first_name = p.first_name,
                    last_name  = p.last_name,
                    email      = p.email,
                    avatar_color = coalesce(p.avatar_color, '#007AFF')
                from user_profiles p where p.user_id = u.id;
            ");

            migrationBuilder.DropColumn(name: "profile_id", table: "users");
            migrationBuilder.DropTable(name: "user_profiles");
        }
    }
}