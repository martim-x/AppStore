using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Project.Converters
{
    // ── 1. BoolToVisibilityConverter ──────────────────────────────────────────
    /// <summary>bool: true → Visible, false → Collapsed</summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => value is Visibility v && v == Visibility.Visible;
    }

    // ── 2. InverseBoolToVisibilityConverter ───────────────────────────────────
    /// <summary>bool: true → Collapsed, false → Visible</summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => value is Visibility v && v == Visibility.Collapsed;
    }

    // ── 3. NullToVisibilityConverter ──────────────────────────────────────────
    /// <summary>null → Collapsed, не null → Visible</summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => value == null ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => DependencyProperty.UnsetValue;
    }

    // ── 4. PriceConverter ─────────────────────────────────────────────────────
    /// <summary>
    /// Форматирует цену в строку в зависимости от текущего языка.
    /// Читает "CurrentLanguage" из Application.Current.Resources и выбирает формат валюты.
    /// Примеры: en → "$9.99", ru → "9,99 ₽", de → "9,99 €".
    /// </summary>
    [ValueConversion(typeof(double), typeof(string))]
    public class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double price)
                return string.Empty;

            if (price <= 0)
                return GetString("Free") ?? "Free";

            var lang = Application.Current?.Resources["CurrentLanguage"] as string ?? "en";
            return lang switch
            {
                "ru" => $"{price:F2} ₽".Replace('.', ','),
                "de" => $"{price:F2} €".Replace('.', ','),
                "fr" => $"{price:F2} €".Replace('.', ','),
                _ => $"${price:F2}",
            };
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => DependencyProperty.UnsetValue;

        private static string? GetString(string key)
        {
            try
            {
                return Application.Current?.Resources[key] as string;
            }
            catch
            {
                return null;
            }
        }
    }

    // ── 5. DownloadCountConverter ─────────────────────────────────────────────
    /// <summary>Форматирует большие числа: 1234 → "1.2K", 1234567 → "1.2M".</summary>
    [ValueConversion(typeof(int), typeof(string))]
    public class DownloadCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long n = value switch
            {
                int i => i,
                long l => l,
                double d => (long)d,
                _ => 0,
            };

            return n switch
            {
                >= 1_000_000 => $"{n / 1_000_000.0:0.#}M",
                >= 1_000 => $"{n / 1_000.0:0.#}K",
                _ => n.ToString(),
            };
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => DependencyProperty.UnsetValue;
    }

    // ── 6. RatingToStarsConverter ─────────────────────────────────────────────
    /// <summary>Оценка 4.5 → "★★★★☆" (целые звёзды закрашенные, остаток — пустые).</summary>
    [ValueConversion(typeof(double), typeof(string))]
    public class RatingToStarsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double rating)
                return string.Empty;

            // Округляем до ближайшего целого в диапазоне [0;5]
            int full = (int)Math.Clamp(Math.Round(rating), 0, 5);
            int empty = 5 - full;

            return new string('★', full) + new string('☆', empty);
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => DependencyProperty.UnsetValue;
    }

    // ── 7. SizeConverter ──────────────────────────────────────────────────────
    /// <summary>Возвращает размер в человекочитаемом виде: "186.3 MB" или "12.5 GB".</summary>
    [ValueConversion(typeof(double), typeof(string))]
    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double sizeMB)
                return string.Empty;

            return sizeMB >= 1024 ? $"{sizeMB / 1024.0:0.#} GB" : $"{sizeMB:0.#} MB";
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => DependencyProperty.UnsetValue;
    }

    // ── 8. StringToColorBrushConverter ───────────────────────────────────────
    /// <summary>Преобразует строку с цветом в формате "#RRGGBB" в SolidColorBrush.</summary>
    [ValueConversion(typeof(string), typeof(SolidColorBrush))]
    public class StringToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string hex)
                return new SolidColorBrush(Colors.Transparent);

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                return new SolidColorBrush(color);
            }
            catch
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => DependencyProperty.UnsetValue;
    }

    // ── 9. DiscountVisibilityConverter ────────────────────────────────────────
    /// <summary>null или 0 → Collapsed; любое положительное значение → Visible.</summary>
    [ValueConversion(typeof(double?), typeof(Visibility))]
    public class DiscountVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return Visibility.Collapsed;
            if (value is double d && d > 0)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => DependencyProperty.UnsetValue;
    }

    // ── 10. RoleNameConverter ─────────────────────────────────────────────────
    /// <summary>bool IsAdmin → "Admin" / "User".</summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class RoleNameConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => value is bool b && b ? "Admin" : "User";

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => DependencyProperty.UnsetValue;
    }

    // ── 11. ButtonLabelConverter ──────────────────────────────────────────────
    /// <summary>
    /// Формирует подпись на кнопке в зависимости от:
    /// values[0] = IsDownloaded (bool) — приложение уже установлено;
    /// values[1] = FinalPrice (double/decimal/float) — цена с учётом скидки;
    /// values[2] = CurrentLanguage ("en" | "ru" и т.п.) — текущий язык, триггер пересчёта.
    /// </summary>
    public class ButtonLabelConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (values.Length < 3)
                return Application.Current.TryFindResource("BtnGet") as string ?? "Установить";

            bool isDownloaded = values[0] is bool b && b;
            double finalPrice = values[1] switch
            {
                double d => d,
                decimal dc => (double)dc,
                float f => f,
                _ => 0d,
            };
            string lang = values[2] as string ?? "en";

            // Установленное приложение можно удалить.
            if (isDownloaded)
            {
                return Application.Current.TryFindResource("BtnUninstall") as string ?? "Delete";
            }

            var getLabel = Application.Current.TryFindResource("BtnGet") as string
                ?? (lang == "ru" ? "Установить" : "Install");

            // Бесплатное приложение — короткий глагол действия.
            if (finalPrice <= 0)
                return getLabel;

            // Для платного приложения показываем только цену.
            var formattedPrice = lang == "ru"
                ? $"{finalPrice * 2.97:F0} BYN"
                : $"${finalPrice.ToString("F2", CultureInfo.GetCultureInfo("en-US"))}";
            return formattedPrice;
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture
        ) => throw new NotSupportedException();
    }

    // ── 12. LocalizedDescriptionConverter ────────────────────────────────────
    /// <summary>
    /// Описание приложения на языке интерфейса.
    /// values[0] = App — модель с Description (русский) и DescriptionEn;
    /// values[1] = CurrentLanguage ("en" | "ru") — триггер пересчёта при смене
    /// языка: карточка, уже открытая в этот момент, обновляется на лету.
    /// Логика выбора/fallback живёт в Core (App.GetDescriptionByLang).
    /// </summary>
    public class LocalizedDescriptionConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            if (values is not { Length: >= 2 } || values[0] is not AppStore.Core.Data.App app)
                return string.Empty;

            return app.GetDescriptionByLang(values[1] as string);
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture
        ) => throw new NotSupportedException();
    }

    /// <summary>
    /// Состояние звезды в блоке «Ваша оценка» (APP-003).
    /// Оценка пользователя больше или равна номеру звезды - звезда залита.
    /// </summary>
    public class RatingStarStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rating = value is int current ? current : (int?)null;
            var index = int.TryParse(parameter?.ToString(), out var parsed) ? parsed : 0;

            return Project.Services.RatingAggregator.StarFor(rating, index);
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
                        CultureInfo culture
                    ) => throw new NotSupportedException();
                }

                /// <summary>
                /// Локализация категории приложения (данные в БД хранятся английскими
                /// строками). Отображаемое название берётся из словаря локализации:
                /// ключи Cat01Development…Cat10Health; при смене языка WPF обновляет
                /// значения через Reload() (SwitchLanguage перестраивает коллекции).
                /// </summary>
                public class CategoryConverter : IValueConverter
                {
                    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
                    {
                        ["Development"] = "Cat01Development",
                        ["Design"] = "Cat02Design",
                        ["Productivity"] = "Cat03Productivity",
                        ["Games"] = "Cat04Games",
                        ["Media"] = "Cat05Media",
                        ["Social"] = "Cat06Social",
                        ["Utilities"] = "Cat07Utilities",
                        ["Education"] = "Cat08Education",
                        ["Finance"] = "Cat09Finance",
                        ["Health"] = "Cat10Health",
                    };

                    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
                    {
                        var key = value as string;
                        if (key != null && Map.TryGetValue(key, out var resourceKey)
                            && Application.Current.TryFindResource(resourceKey) is string localized)
                            return localized;

                        return key ?? string.Empty;
                    }

                    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                        => throw new NotSupportedException();
                }
            }
