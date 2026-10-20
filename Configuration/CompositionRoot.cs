using System;
using Microsoft.Extensions.DependencyInjection;
using Project.Configuration;
using AppStore.Core.Configuration;
using AppStore.Core.Data;
using Project.Services;
using Project.ViewModels;

namespace Project.Configuration
{
    /// <summary>
    /// Composition root — registers all services and repositories.
    /// Single place for dependency injection configuration.
    /// </summary>
    public static class CompositionRoot
    {
        public static IServiceProvider Build()
        {
            var services = new ServiceCollection();

            // ── Configuration ──────────────────────────────────────────────────
            // Settings is a singleton, accessed via Settings.Instance
            services.AddSingleton(_ => Settings.Instance);

            // ── Password Hashing ───────────────────────────────────────────────
            // Default to BCrypt for new passwords, SHA-256 available for compat
            services.AddSingleton<IPasswordHasher, BCryptHasher>();
            services.AddSingleton<SHA256Hasher>();

            // ── Database / Repository ──────────────────────────────────────────
            services.AddScoped<IRepository, PostgreSQLRepEF>();
            services.AddScoped<PostgreSQLRepEF>();

            // ── Services ───────────────────────────────────────────────────────
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<ILocalizationService, LocalizationService>();
            services.AddSingleton<IUndoRedoService, UndoRedoService>();
            services.AddScoped<IAuthService, AuthService>();

            // ── ViewModels ─────────────────────────────────────────────────────
            services.AddTransient<MainViewModel>();
            services.AddTransient<AddEditAppViewModel>();
            services.AddTransient<ProfileViewModel>();

            // ── Views (Windows) ────────────────────────────────────────────────
            services.AddTransient<Views.LoginWindow>();
            services.AddTransient<Views.MainWindow>();
            services.AddTransient<Views.AddEditWindow>();
            services.AddTransient<Views.AppDetailView>();
            services.AddTransient<Views.ProfilePopup>();

            var provider = services.BuildServiceProvider();
            return provider;
        }
    }
}