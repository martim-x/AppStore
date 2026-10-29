using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppStore.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "age_rating_id",
                table: "apps",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                table: "apps",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "country_id",
                table: "apps",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "developer_id",
                table: "apps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "age_ratings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_age_ratings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    iso_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "developers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    country_id = table.Column<Guid>(type: "uuid", nullable: true),
                    website = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    founded_year = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_developers", x => x.id);
                    table.ForeignKey(
                        name: "FK_developers_countries_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "app_tags",
                columns: table => new
                {
                    app_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_tags", x => new { x.app_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_app_tags_apps_app_id",
                        column: x => x.app_id,
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_app_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_apps_age_rating_id",
                table: "apps",
                column: "age_rating_id");

            migrationBuilder.CreateIndex(
                name: "ix_apps_category",
                table: "apps",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_apps_country_id",
                table: "apps",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "ix_apps_developer",
                table: "apps",
                column: "developer_id");

            migrationBuilder.CreateIndex(
                name: "ux_age_ratings_label",
                table: "age_ratings",
                column: "label",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_app_tags_tag_id",
                table: "app_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ux_categories_name",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_countries_iso",
                table: "countries",
                column: "iso_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_developers_country_id",
                table: "developers",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "ux_developers_name",
                table: "developers",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_tags_name",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_apps_age_ratings_age_rating_id",
                table: "apps",
                column: "age_rating_id",
                principalTable: "age_ratings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_apps_categories_category_id",
                table: "apps",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_apps_countries_country_id",
                table: "apps",
                column: "country_id",
                principalTable: "countries",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_apps_developers_developer_id",
                table: "apps",
                column: "developer_id",
                principalTable: "developers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // ── перенос справочных значений из старых текстовых колонок ──
            migrationBuilder.Sql(@"
                insert into countries (id, iso_code, name)
                select gen_random_uuid(), upper(trim(country)), upper(trim(country))
                from apps where country is not null and trim(country) <> ''
                group by upper(trim(country));
                update apps a set country_id = c.id
                from countries c where upper(trim(a.country)) = c.iso_code;

                insert into categories (id, name)
                select gen_random_uuid(), trim(category) from apps
                where category is not null and trim(category) <> ''
                group by trim(category);
                update apps a set category_id = c.id
                from categories c where trim(a.category) = c.name;

                insert into developers (id, name)
                select gen_random_uuid(), trim(developer) from apps
                where developer is not null and trim(developer) <> ''
                group by trim(developer);
                update apps a set developer_id = d.id
                from developers d where trim(a.developer) = d.name;

                insert into age_ratings (id, label)
                select gen_random_uuid(), trim(age_rating) from apps
                where age_rating is not null and trim(age_rating) <> ''
                group by trim(age_rating);
                update apps a set age_rating_id = r.id
                from age_ratings r where trim(a.age_rating) = r.label;

                insert into tags (id, name)
                select gen_random_uuid(), trim(t.tag)
                from apps a, unnest(string_to_array(a.tags, ',')) as t(tag)
                where a.tags is not null and trim(t.tag) <> ''
                group by trim(t.tag);

                insert into app_tags (app_id, tag_id)
                select distinct a.id, tg.id
                from apps a, unnest(string_to_array(a.tags, ',')) as t(tag)
                join tags tg on tg.name = trim(t.tag)
                where a.tags is not null and trim(t.tag) <> '';
            ");

            migrationBuilder.DropColumn(name: "developer", table: "apps");
            migrationBuilder.DropColumn(name: "category", table: "apps");
            migrationBuilder.DropColumn(name: "country", table: "apps");
            migrationBuilder.DropColumn(name: "age_rating", table: "apps");
            migrationBuilder.DropColumn(name: "tags", table: "apps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Обратный переход: возвращаем текстовые колонки и заполняем их
            // из справочников, чтобы данные не терялись при откате.
            migrationBuilder.AddColumn<string>(
                name: "developer", table: "apps", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "category", table: "apps", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "country", table: "apps", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "age_rating", table: "apps", type: "text", nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "tags", table: "apps", type: "text", nullable: true);

            migrationBuilder.Sql(@"
                update apps a set developer = d.name from developers d where d.id = a.developer_id;
                update apps a set category  = c.name from categories c where c.id = a.category_id;
                update apps a set country   = n.iso_code from countries n where n.id = a.country_id;
                update apps a set age_rating= r.label from age_ratings r where r.id = a.age_rating_id;
                update apps a set tags = coalesce((
                    select string_agg(t.name, ',' order by t.name)
                    from app_tags x join tags t on t.id = x.tag_id
                    where x.app_id = a.id), '');
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_apps_age_ratings_age_rating_id",
                table: "apps");

            migrationBuilder.DropForeignKey(
                name: "FK_apps_categories_category_id",
                table: "apps");

            migrationBuilder.DropForeignKey(
                name: "FK_apps_countries_country_id",
                table: "apps");

            migrationBuilder.DropForeignKey(
                name: "FK_apps_developers_developer_id",
                table: "apps");

            migrationBuilder.DropTable(
                name: "age_ratings");

            migrationBuilder.DropTable(
                name: "app_tags");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "developers");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropIndex(
                name: "IX_apps_age_rating_id",
                table: "apps");

            migrationBuilder.DropIndex(
                name: "ix_apps_category",
                table: "apps");

            migrationBuilder.DropIndex(
                name: "IX_apps_country_id",
                table: "apps");

            migrationBuilder.DropIndex(
                name: "ix_apps_developer",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "age_rating_id",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "country_id",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "developer_id",
                table: "apps");

        }
    }
}
