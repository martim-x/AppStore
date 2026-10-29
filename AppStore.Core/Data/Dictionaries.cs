using System;
using System.Collections.Generic;

namespace AppStore.Core.Data
{
    /// <summary>
    /// Справочник стран происхождения приложений и разработчиков.
    /// Код страны — ISO 3166-1 alpha-2, названия дублируются для отображения.
    /// </summary>
    public class Country
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Код ISO 3166-1 alpha-2 (US, RU, BY, …).</summary>
        public string IsoCode { get; set; } = string.Empty;

        /// <summary>Название страны.</summary>
        public string Name { get; set; } = string.Empty;

        public List<Developer> Developers { get; set; } = new();
        public List<App> Apps { get; set; } = new();
    }

    /// <summary>
    /// Справочник организаций-разработчиков (издателей) приложений.
    /// Одна организация описывается один раз и переиспользуется записями каталога.
    /// </summary>
    public class Developer
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Наименование организации.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Страна регистрации организации (необязательно).</summary>
        public Guid? CountryId { get; set; }
        public Country? Country { get; set; }

        /// <summary>Официальный сайт (необязательно).</summary>
        public string? Website { get; set; }

        /// <summary>Год основания (необязательно).</summary>
        public int? FoundedYear { get; set; }

        public List<App> Apps { get; set; } = new();
    }

    /// <summary>Справочник категорий каталога (Design, Games, Productivity, …).</summary>
    public class CatalogCategory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        public List<App> Apps { get; set; } = new();
    }

    /// <summary>Справочник возрастных маркировок (4+, 9+, 12+, 17+).</summary>
    public class AgeRating
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Label { get; set; } = string.Empty;

        public List<App> Apps { get; set; } = new();
    }

    /// <summary>Справочник тегов приложения.</summary>
    public class Tag
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        public List<AppTag> Apps { get; set; } = new();
    }

    /// <summary>
    /// Связующая таблица «приложение — тег»: отношение «многие ко многим»
    /// между каталогом и справочником тегов.
    /// </summary>
    public class AppTag
    {
        public Guid AppId { get; set; }
        public App? App { get; set; }

        public Guid TagId { get; set; }
        public Tag? Tag { get; set; }
    }
}
