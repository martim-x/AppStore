using System.Collections.Generic;
using System.Linq;

namespace AppStore.Core.Data
{
    /// <summary>Общий порядок выдачи каталога и отфильтрованных результатов.</summary>
    public static class AppOrdering
    {
        public static IEnumerable<App> Apply(IEnumerable<App> apps, string sortBy) => sortBy switch
        {
            "name" => apps.OrderBy(app => app.ShortName),
            "price" or "price_asc" => apps
                .OrderBy(app => app.FinalPrice)
                .ThenByDescending(app => app.Rating),
            "price_desc" => apps
                .OrderByDescending(app => app.FinalPrice)
                .ThenByDescending(app => app.Rating),
            "rating" => apps.OrderByDescending(app => app.Rating),
            "newest" => apps.OrderByDescending(app => app.ReleaseDate),
            "downloads" => apps
                .OrderByDescending(app => app.DownloadCount)
                .ThenByDescending(app => app.Rating),
            "featured" => apps
                .OrderByDescending(app => app.IsFeatured)
                .ThenByDescending(app => app.Rating),
            _ => apps.OrderByDescending(app => app.IsFeatured).ThenByDescending(app => app.Rating),
        };
    }
}