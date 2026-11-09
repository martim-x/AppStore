using System;

namespace Project.Services
{
    /// <summary>
    /// Агрегация оценок приложения (APP-003).
    /// apps.rating/rating_count хранят накопленный агрегат: новая оценка
    /// добавляется к сумме, повторная — заменяет прежнюю оценку пользователя.
    /// </summary>
    public static class RatingAggregator
    {
        /// <summary>Допустимый диапазон оценки.</summary>
        public const int MinValue = 1;
        public const int MaxValue = 5;

        /// <summary>
        /// Пересчитать агрегат при добавлении или замене оценки.
        /// </summary>
        /// <param name="rating">Текущая средняя оценка.</param>
        /// <param name="count">Текущее количество оценок.</param>
        /// <param name="oldValue">Прежняя оценка пользователя (null — оценка первая).</param>
        /// <param name="newValue">Новая оценка пользователя.</param>
        /// <returns>Новая средняя оценка (1 знак) и количество оценок.</returns>
        public static (double rating, int count) Apply(
            double rating,
            int count,
            int? oldValue,
            int newValue)
        {
            if (newValue < MinValue || newValue > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(newValue), "Оценка должна быть от 1 до 5.");

            if (oldValue.HasValue && (oldValue.Value < MinValue || oldValue.Value > MaxValue))
                throw new ArgumentOutOfRangeException(nameof(oldValue), "Прежняя оценка должна быть от 1 до 5.");

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (oldValue.HasValue && count == 0)
            {
                // Агрегат пуст, но оценка пользователя существует (рассинхронизация):
                // считаем её единственной.
                return (newValue, 1);
            }

            if (oldValue.HasValue)
            {
                // Замена: количество не меняется, сумма корректируется.
                var sum = rating * count - oldValue.Value + newValue;
                return (Round(sum / count), count);
            }

            // Новая оценка: добавляется к накопленной сумме.
            var newSum = rating * count + newValue;
            var newCount = count + 1;
            return (Round(newSum / newCount), newCount);
        }

        private static double Round(double value) =>
            Math.Round(value, 1, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Символ звезды для блока «Ваша оценка»: залитая звезда, если
        /// оценка пользователя не меньше номера звезды.
        /// </summary>
        /// <param name="rating">Оценка пользователя (null — ещё не оценивал).</param>
        /// <param name="index">Номер звезды от 1 до 5.</param>
        public static string StarFor(int? rating, int index)
        {
            if (index < MinValue || index > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(index));

            return (rating ?? 0) >= index ? "★" : "☆";
        }
    }
}
