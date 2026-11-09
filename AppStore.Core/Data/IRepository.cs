using System;
using System.Collections.Generic;

namespace AppStore.Core.Data
{
    /// <summary>
    /// Интерфейс репозитория для доступа к данным приложений и пользователей.
    /// Реализует паттерн Repository для абстракции доступа к данным.
    /// </summary>
    public interface IRepository
    {
        /// <summary>Получить все приложения.</summary>
        List<App> GetAllApps();

        /// <summary>Получить приложение по ID.</summary>
        App? GetAppById(Guid id);

        /// <summary>Добавить новое приложение.</summary>
        void AddApp(App app);

        /// <summary>Обновить существующее приложение.</summary>
        void UpdateApp(App app);

        /// <summary>Удалить приложение по ID.</summary>
        void DeleteApp(Guid id);

        /// <summary>Установить (скачать) приложение.</summary>
        void DownloadApp(Guid id);

        /// <summary>Удалить установленное приложение (uninstall).</summary>
        void UninstallApp(Guid id);

        /// <summary>Восстановить исходное наполнение каталога (seed data).</summary>
        void RestoreDefaults();

        /// <summary>Получить всех пользователей.</summary>
        List<User> GetAllUsers();

        /// <summary>Найти пользователя по логину.</summary>
        User? GetUserByLogin(string login);

        /// <summary>Добавить нового пользователя.</summary>
        void AddUser(User user);

        /// <summary>Обновить данные пользователя.</summary>
        void UpdateUser(User user);

        /// <summary>Оценка текущего пользователя для приложения (null — не оценивал).</summary>
        int? GetUserRating(Guid appId);

        /// <summary>Поставить или изменить оценку (1–5) и пересчитать агрегат приложения.</summary>
        void RateApp(Guid appId, int value);
    }
}
