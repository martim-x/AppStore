using System;
using System.Collections.Generic;
using System.Globalization;
using AppStore.Core.Data;
using Xunit;
using FluentAssertions;

namespace AppStore.UnitTests.Models
{
    public class AppTests
    {
        [Fact]
        public void FinalPrice_NoDiscount_ReturnsPrice()
        {
            var app = new App { Price = 100.0, DiscountPercent = null };
            app.FinalPrice.Should().Be(100.0);
        }

        [Fact]
        public void FinalPrice_WithDiscount_CalculatesCorrectly()
        {
            var app = new App { Price = 100.0, DiscountPercent = 20.0 };
            app.FinalPrice.Should().Be(80.0);
        }

        [Fact]
        public void FinalPrice_With100PercentDiscount_ReturnsZero()
        {
            var app = new App { Price = 100.0, DiscountPercent = 100.0 };
            app.FinalPrice.Should().Be(0.0);
        }

        private static void WithInvariantCulture(Action action)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                action();
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void ButtonLabel_NotDownloaded_FreeApp_ReturnsInstallAction()
        {
            WithInvariantCulture(() =>
            {
                var app = new App { Price = 0, IsDownloaded = false };
                app.ButtonLabel.Should().Be("Установить");
            });
        }

        [Fact]
        public void ButtonLabel_NotDownloaded_PaidApp_ReturnsPrice()
        {
            WithInvariantCulture(() =>
            {
                var app = new App { Price = 99.99, IsDownloaded = false, DiscountPercent = null };
                app.ButtonLabel.Should().Be("$99.99");
            });
        }

        [Fact]
        public void ButtonLabel_Downloaded_ReturnsDeleteAction()
        {
            WithInvariantCulture(() =>
            {
                var app = new App { Price = 99.99, IsDownloaded = true };
                app.ButtonLabel.Should().Be("Удалить");
            });
        }

        [Fact]
        public void ButtonLabel_Discounted_ShowsFinalPrice()
        {
            WithInvariantCulture(() =>
            {
                var app = new App { Price = 100.0, DiscountPercent = 50.0, IsDownloaded = false };
                app.ButtonLabel.Should().Be("$50.00");
            });
        }

        [Theory]
        [InlineData("Design")]
        [InlineData("Productivity")]
        [InlineData("Games")]
        [InlineData("Development")]
        public void Category_CanBeSet(string category)
        {
            var app = new App { Category = category };
            app.Category.Should().Be(category);
        }

        [Fact]
        public void Tags_DefaultIsEmptyList()
        {
            var app = new App();
            app.Tags.Should().NotBeNull();
            app.Tags.Should().BeEmpty();
        }

        [Fact]
        public void Tags_CanAddMultiple()
        {
            var app = new App { Tags = new List<string> { "editor", "design", "ui" } };
            app.Tags.Should().HaveCount(3);
            app.Tags.Should().Contain("editor");
        }
    }

    public class UserTests
    {
        [Fact]
        public void AvatarLetter_FromLogin_ReturnsUppercaseFirstChar()
        {
            var user = new User { Login = "testuser" };
            user.AvatarLetter.Should().Be("T");
        }

        [Fact]
        public void AvatarLetter_EmptyLogin_ReturnsQuestionMark()
        {
            var user = new User { Login = "" };
            user.AvatarLetter.Should().Be("?");
        }

        [Fact]
        public void AvatarLetter_NullLogin_ReturnsQuestionMark()
        {
            var user = new User { Login = null! };
            user.AvatarLetter.Should().Be("?");
        }

        [Fact]
        public void Role_DefaultIsUser()
        {
            var user = new User();
            user.Role.Should().Be(UserRole.User);
        }

        [Fact]
        public void Role_CanBeSetToAdmin()
        {
            var user = new User { Role = UserRole.Admin };
            user.Role.Should().Be(UserRole.Admin);
        }
    }

    public class UsersAppsTests
    {
        [Fact]
        public void Constructor_SetsInstalledAtToNow()
        {
            var before = DateTime.Now.AddSeconds(-1);
            var link = new UsersApps { UserId = Guid.NewGuid(), AppId = Guid.NewGuid() };
            var after = DateTime.Now.AddSeconds(1);

            link.InstalledAt.Should().BeOnOrAfter(before);
            link.InstalledAt.Should().BeOnOrBefore(after);
        }
    }
}
