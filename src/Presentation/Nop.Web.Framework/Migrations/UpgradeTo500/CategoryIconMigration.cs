using FluentMigrator;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Services.Seo;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// Five of the six home page categories had no image on the live site, so they were
/// seeded with generated cards - the brand gradient, the logo and the category name
/// set in white - while "غسالة ملابس" carried a real line-art icon. Six tiles in two
/// rows showed five blue cards around one drawing.
///
/// The cards are replaced with line art drawn to match that icon: 512x512, black on
/// transparent, the same 16px stroke. "شاشة تلفزيون" is in here too - it was line art
/// already, but thinner, opaque-white behind, and drawn to a different scale.
///
/// Four more cards were missed the first time round - "مكيفات", "فريزر", "برادة ماء"
/// and "كاوية" are not on the home page, so they only showed up once you opened the
/// category list. Same treatment, hence the later version: a store that already ran
/// this migration re-uploads all of them, which costs one write per category.
///
/// ponytail: re-upload the sample file, the same way BrandLogoTrimMigration does. The
/// sample images are the source of truth for a fresh install, so the migration only
/// has to carry the stores that were installed before them. A category whose picture
/// has since been replaced from admin no longer matches its seeded file and is left
/// alone.
/// </summary>
[NopUpdateMigration("2026-09-14 12:00:00", "5.00", UpdateMigrationType.Data)]
public class CategoryIconMigration : MigrationBase
{
    //category name -> the sample file it is seeded from (SampleData.json)
    protected static readonly Dictionary<string, string> _seededIcons = new(StringComparer.OrdinalIgnoreCase)
    {
        ["بيلت ان"] = "category_gen_3.png",
        ["ثلاجة"] = "category_gen_8.png",
        ["افران"] = "category_gen_4.png",
        ["أجهزة مطبخ"] = "category_gen_9.png",
        ["شاشة تلفزيون"] = "category_gen_11.png",
        ["مكيفات"] = "category_gen_1.png",
        ["فريزر"] = "category_gen_5.png",
        ["برادة ماء"] = "category_gen_6.png",
        ["كاوية"] = "category_gen_7.png",
        //two more cards were left behind inside the category pages: "اسبليت" sat among
        //six line-art siblings on /مكيفات and "ميكرويف" among four on /أجهزة-مطبخ.
        //The microwave borrows the drawing the built-in one already ships - same
        //appliance, and the two never appear on the same page.
        ["اسبليت"] = "category_gen_2.png",
        ["ميكرويف"] = "category_36.png"
    };

    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
        var categoryService = EngineContext.Current.Resolve<ICategoryService>();
        var pictureService = EngineContext.Current.Resolve<IPictureService>();
        var urlRecordService = EngineContext.Current.Resolve<IUrlRecordService>();

        foreach (var category in categoryService.GetAllCategoriesAsync(showHidden: true).Result)
        {
            if (category.PictureId == 0 || !_seededIcons.TryGetValue(category.Name, out var fileName))
                continue;

            var path = fileProvider.GetAbsolutePath("images", "samples", fileName);
            if (!fileProvider.FileExists(path))
                continue;

            //a new SEO filename, because the thumb file is named after it: replacing the
            //binary under the same name leaves every browser holding the old card in
            //cache with no URL change to tell it otherwise.
            //The name comes from the category's own slug, not GetPictureSeNameAsync,
            //which strips non-western characters - and for these categories that is the
            //entire name, leaving five pictures all called "-icon".
            var slug = urlRecordService.GetSeNameAsync(category).Result;

            pictureService.UpdatePictureAsync(
                    category.PictureId,
                    fileProvider.ReadAllBytesAsync(path).Result,
                    MimeTypes.ImagePng,
                    $"{slug}-icon")
                .Wait();
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
