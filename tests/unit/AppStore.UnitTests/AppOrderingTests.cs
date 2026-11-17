using System.Collections.Generic;
using System.Linq;
using AppStore.Core.Data;
using Xunit;
using FluentAssertions;

namespace AppStore.UnitTests.Models
{
    public class AppOrderingTests
    {
        [Fact]
        public void Apply_PriceAscending_SortsFilteredAppsByFinalPriceThenRating()
        {
            var apps = new[]
            {
                new App { ShortName = "Paid", Price = 20, Rating = 4.9 },
                new App { ShortName = "Free", Price = 0, Rating = 4.0 },
                new App { ShortName = "Discounted", Price = 50, DiscountPercent = 80, Rating = 4.5 },
            };

            AppOrdering.Apply(apps, "price_asc")
                .Select(app => app.ShortName)
                .Should()
                .Equal("Free", "Discounted", "Paid");
        }

        [Fact]
        public void Apply_PriceDescending_SortsFilteredAppsByFinalPriceThenRating()
        {
            var apps = new[]
            {
                new App { ShortName = "Paid", Price = 20, Rating = 4.9 },
                new App { ShortName = "Free", Price = 0, Rating = 4.0 },
                new App { ShortName = "Discounted", Price = 50, DiscountPercent = 80, Rating = 4.5 },
            };

            AppOrdering.Apply(apps, "price_desc")
                .Select(app => app.ShortName)
                .Should()
                .Equal("Paid", "Discounted", "Free");
        }
    }
}
