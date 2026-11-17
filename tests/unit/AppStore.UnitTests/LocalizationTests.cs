using System;
using System.IO;
using System.Linq;
using AppStore.Core.Data;
using Newtonsoft.Json;
using Xunit;
using FluentAssertions;

namespace AppStore.UnitTests.Models
{
    /// <summary>
    /// Локализация описаний приложений RU/EN:
    /// модель App хранит Description (русский, legacy/canonical) и DescriptionEn
    /// (английский, NULL/пусто = fallback на Description для старых данных).
    /// </summary>
    public class DescriptionLocalizationTests
    {
        // ── Выбор описания по языку ──────────────────────────────────────────

        [Fact]
        public void GetDescriptionByLang_EnWithEnText_ReturnsEnglish()
        {
            var app = new App
            {
                Description = "Русское описание",
                DescriptionEn = "English description",
            };

            app.GetDescriptionByLang("en").Should().Be("English description");
        }

        [Fact]
        public void GetDescriptionByLang_Ru_ReturnsRussianDescription()
        {
            var app = new App
            {
                Description = "Русское описание",
                DescriptionEn = "English description",
            };

            app.GetDescriptionByLang("ru").Should().Be("Русское описание");
        }

        [Fact]
        public void GetDescriptionByLang_EnWithNullEn_FallsBackToDescription()
        {
            var app = new App
            {
                Description = "Русское описание",
                DescriptionEn = null,
            };

            app.GetDescriptionByLang("en").Should().Be("Русское описание");
        }

        [Fact]
        public void GetDescriptionByLang_EnWithEmptyEn_FallsBackToDescription()
        {
            var app = new App
            {
                Description = "Русское описание",
                DescriptionEn = string.Empty,
            };

            app.GetDescriptionByLang("en").Should().Be("Русское описание");
        }

        [Fact]
        public void GetDescriptionByLang_EnWithWhitespaceEn_FallsBackToDescription()
        {
            var app = new App
            {
                Description = "Русское описание",
                DescriptionEn = "   ",
            };

            app.GetDescriptionByLang("en").Should().Be("Русское описание");
        }

        [Fact]
        public void GetDescriptionByLang_UnknownLang_FallsBackToDescription()
        {
            var app = new App
            {
                Description = "Русское описание",
                DescriptionEn = "English description",
            };

            app.GetDescriptionByLang("de").Should().Be("Русское описание");
            app.GetDescriptionByLang(null).Should().Be("Русское описание");
        }

        // ── Seed-данные ───────────────────────────────────────────────────────

        [Fact]
        public void SeedApps_AllTwelveApps_HaveBothLanguageDescriptions()
        {
            var apps = SeedViaJsonRep();

            apps.Should().HaveCount(12);

            foreach (var app in apps)
            {
                app.Description.Should().NotBeNullOrWhiteSpace(
                    $"seed-приложение {app.ShortName} должно иметь русский текст");
                app.DescriptionEn.Should().NotBeNullOrWhiteSpace(
                    $"seed-приложение {app.ShortName} должно иметь английский текст");
            }
        }

        [Fact]
        public void SeedApps_Descriptions_DoNotContainEmDash()
        {
            var apps = SeedViaJsonRep();

            apps.Should().HaveCount(12);
            foreach (var app in apps)
            {
                app.Description.Should().NotContain("—", $"Russian description for {app.ShortName}");
                app.DescriptionEn.Should().NotContain("—", $"English description for {app.ShortName}");
            }
        }

        // ── JSON persistence ─────────────────────────────────────────────────

        [Fact]
        public void JsonRep_RoundTrip_PreservesBothDescriptions()
        {
            var dir = TempDir("roundtrip");
            var app = new App
            {
                ShortName = "Test",
                FullName = "Test App",
                Description = "Русское описание",
                DescriptionEn = "English description",
            };

            var rep = NewRep(dir);
            rep.AddApp(app);
            rep.UpdateApp(app);

            var reloaded = NewRep(dir).GetAppById(app.Id);
            reloaded.Should().NotBeNull();
            reloaded!.Description.Should().Be("Русское описание");
            reloaded.DescriptionEn.Should().Be("English description");
        }

        [Fact]
        public void JsonRep_OldJsonWithoutDescriptionEn_LoadsSafelyWithFallback()
        {
            var dir = TempDir("legacy");
            var file = Path.Combine(dir, "apps.json");

            // Старый формат: массив без поля descriptionEn вообще.
            File.WriteAllText(
                file,
                """
                [
                  {
                    "Id": "11111111-2222-3333-4444-555555555555",
                    "ShortName": "Legacy",
                    "FullName": "Legacy App",
                    "Description": "Только русский текст",
                    "Rating": 4.0
                  }
                ]
                """
            );

            var rep = NewRep(dir);
            var app = rep.GetAppById(Guid.Parse("11111111-2222-3333-4444-555555555555"));

            app.Should().NotBeNull();
            app!.DescriptionEn.Should().BeNull();
            app.GetDescriptionByLang("ru").Should().Be("Только русский текст");
            app.GetDescriptionByLang("en").Should().Be("Только русский текст");
        }


        // ── Хелперы ───────────────────────────────────────────────────────────

        private static string TempDir(string name)
        {
            var dir = Path.Combine(
                Path.GetTempPath(),
                "appstore-unit-tests",
                $"{name}-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static JsonRep NewRep(string dir) => new(
            appsFilePath: Path.Combine(dir, "apps.json"),
            usersFilePath: Path.Combine(dir, "users.json"),
            usersAppsFilePath: Path.Combine(dir, "users_apps.json"),
            ratingsFilePath: Path.Combine(dir, "app_ratings.json"));

        /// <summary>SeedApps защищён (protected static) — читаем через JsonRep: пустой файл → seed.</summary>
        private static List<App> SeedViaJsonRep()
        {
            var dir = TempDir("seed");
            return NewRep(dir).GetAllApps();
        }
    }
}
