using FluentAssertions;
using Project.Services;
using Xunit;

namespace AppStore.UnitTests.Services
{
    /// <summary>
    /// APP-003: агрегат оценок приложения.
    /// Инвариант: средняя оценка = сумма всех оценок / количество оценок;
    /// повторная оценка тем же пользователем заменяет прежнюю, а не добавляет новую.
    /// </summary>
    public class RatingAggregatorTests
    {
        [Fact]
        public void Apply_FirstRating_AppendsValue()
        {
            var (rating, count) = RatingAggregator.Apply(4.0, 1, oldValue: null, newValue: 5);

            rating.Should().Be(4.5);
            count.Should().Be(2);
        }

        [Fact]
        public void Apply_ReplaceRating_KeepsCountAndRecomputesAverage()
        {
            // 4.5 при двух оценках = 9 баллов; 5 заменяем на 4 → 8/2 = 4.0
            var (rating, count) = RatingAggregator.Apply(4.5, 2, oldValue: 5, newValue: 4);

            rating.Should().Be(4.0);
            count.Should().Be(2);
        }

        [Fact]
        public void Apply_ExampleFromBacklog_AveragesToFourPointFour()
        {
            // 4, 4, 5, 5, 4 → 4.4: накапливаем последовательно
            var state = (rating: 0.0, count: 0);
            foreach (var value in new[] { 4, 4, 5, 5, 4 })
                state = RatingAggregator.Apply(state.rating, state.count, null, value);

            state.rating.Should().Be(4.4);
            state.count.Should().Be(5);
        }

        [Fact]
        public void Apply_RoundsToOneDecimal()
        {
            // 5 и 4 → 4.5; затем 5 → 14/3 = 4.666… → 4.7
            var (rating, _) = RatingAggregator.Apply(4.5, 2, null, 5);

            rating.Should().Be(4.7);
        }

        [Fact]
        public void Apply_EmptyAggregateWithReplace_KeepsValue()
        {
            var (rating, count) = RatingAggregator.Apply(0, 0, oldValue: 5, newValue: 3);

            rating.Should().Be(3.0);
            count.Should().Be(1);
        }

        [Fact]
        public void StarFor_NoUserRating_ReturnsEmptyStars()
        {
            RatingAggregator.StarFor(null, 1).Should().Be("☆");
            RatingAggregator.StarFor(null, 5).Should().Be("☆");
        }

        [Fact]
        public void StarFor_ThreePoints_FillsThreeStars()
        {
            var stars = Enumerable.Range(1, 5).Select(i => RatingAggregator.StarFor(3, i));

            stars.Should().Equal("★", "★", "★", "☆", "☆");
        }

        [Fact]
        public void StarFor_ChangedRating_UpdatesImmediately()
        {
            var before = Enumerable.Range(1, 5).Select(i => RatingAggregator.StarFor(5, i));
            var after = Enumerable.Range(1, 5).Select(i => RatingAggregator.StarFor(1, i));

            before.Should().OnlyContain(s => s == "★");
            after.Should().Equal("★", "☆", "☆", "☆", "☆");
        }

        [Fact]
        public void StarFor_OutOfRangeIndex_Throws()
        {
            Action act = () => RatingAggregator.StarFor(3, 6);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
