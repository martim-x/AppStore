using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStore.Core.Data
{
    /// <summary>
    /// Модель приложения в каталоге.
    /// Реализует интерфейс IApp для привязки к UI.
    /// </summary>
    public class App : IApp
    {
        /// <summary>Уникальный идентификатор приложения.</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Краткое название (для списков).</summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>Полное название приложения.</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Описание приложения.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Разработчик/издатель (наименование из справочника developers).</summary>
        [NotMapped]
        public string Developer { get; set; } = string.Empty;

        /// <summary>Ссылка на справочник организаций-разработчиков.</summary>
        public Guid? DeveloperId { get; set; }

        /// <summary>Категория (Design, Productivity, Games, Development, Media, Social, Education, Finance, Health, Utilities).</summary>
        [NotMapped]
        public string Category { get; set; } = string.Empty;

        /// <summary>Ссылка на справочник категорий.</summary>
        public Guid? CategoryId { get; set; }

        /// <summary>Рейтинг (0.0 - 5.0).</summary>
        public double Rating { get; set; }

        /// <summary>Количество оценок.</summary>
        public int RatingCount { get; set; }

        /// <summary>Цена в условных единицах.</summary>
        public double Price { get; set; }

        /// <summary>Версия приложения.</summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>Размер в МБ.</summary>
        public double SizeMB { get; set; }

        /// <summary>Страна происхождения (ISO код).</summary>
        [NotMapped]
        public string Country { get; set; } = string.Empty;

        /// <summary>Ссылка на справочник стран.</summary>
        public Guid? CountryId { get; set; }

        /// <summary>Возрастной рейтинг (4+, 9+, 12+, 17+).</summary>
        [NotMapped]
        public string AgeRating { get; set; } = "4+";

        /// <summary>Ссылка на справочник возрастных маркировок.</summary>
        public Guid? AgeRatingId { get; set; }

        /// <summary>Акцентный цвет (HEX).</summary>
        public string Color { get; set; } = "#007AFF";

        /// <summary>Помечено как рекомендуемое (Featured).</summary>
        public bool IsFeatured { get; set; }

        /// <summary>Установлено ли у текущего пользователя.</summary>
        public bool IsDownloaded { get; set; }

        /// <summary>Доступно ли для установки.</summary>
        public bool IsInStock { get; set; } = true;

        /// <summary>Счётчик скачиваний.</summary>
        public int DownloadCount { get; set; }

        /// <summary>Процент скидки (null = нет скидки).</summary>
        public double? DiscountPercent { get; set; }

        /// <summary>Дата релиза.</summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>Теги для фильтрации и поиска (из справочника tags).</summary>
        [NotMapped]
        public List<string> Tags { get; set; } = new();

        /// <summary>Ссылки на справочник тегов (связь «многие ко многим»).</summary>
        [NotMapped]
        public List<Guid> TagIds { get; set; } = new();

        /// <summary>
        /// Итоговая цена с учётом скидки.
        /// </summary>
        public double FinalPrice =>
            DiscountPercent.HasValue ? Price * (1.0 - DiscountPercent.Value / 100.0) : Price;

        /// <summary>
        /// Текст кнопки: цена / "Загрузить" / "Открыть".
        /// </summary>
        public string ButtonLabel =>
            IsDownloaded ? "Открыть"
            : Price == 0 ? "Загрузить"
            : $"${FinalPrice:F2}";
    }

    /// <summary>
    /// Модель пользователя системы.
    /// </summary>
    public class User : IUser
    {
        /// <summary>Уникальный идентификатор пользователя.</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Логин (уникальный).</summary>
        public string Login { get; set; } = string.Empty;

        /// <summary>Хеш пароля (BCrypt или SHA-256 legacy).</summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Имя пользователя.</summary>
        public string? FirstName { get; set; }

        /// <summary>Фамилия пользователя.</summary>
        public string? LastName { get; set; }

        /// <summary>Email пользователя.</summary>
        public string? Email { get; set; }

        /// <summary>Роль: User или Admin.</summary>
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>Цвет аватара (HEX).</summary>
        public string AvatarColor { get; set; } = "#007AFF";

        /// <summary>Первая буква логина для аватара.</summary>
        public string AvatarLetter =>
            string.IsNullOrEmpty(Login) ? "?" : Login[0].ToString().ToUpper();
    }

    /// <summary>
    /// Связь пользователь-приложение (установка/покупка).
    /// </summary>
    public class UsersApps : IUsersApps
    {
        /// <summary>ID пользователя.</summary>
        public Guid UserId { get; set; }

        /// <summary>ID приложения.</summary>
        public Guid AppId { get; set; }

        /// <summary>Дата установки/покупки.</summary>
        public DateTime InstalledAt { get; set; } = DateTime.Now;
    }
}
