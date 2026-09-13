using FluentMigrator;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Catalog;
using Nop.Services.Media;

namespace Nop.Web.Framework.Migrations.UpgradeTo500;

/// <summary>
/// The brand logos were supplied as wordmarks centred on a 600x248 canvas, with the
/// ink covering about a third of the width and a fifth of the height. On the home
/// page strip - now that the tile is the logo and nothing else - that whitespace was
/// the tile, and the logo read as a stamp in the middle of it. The sample files are
/// trimmed to their ink; this replaces the copies already uploaded.
///
/// ponytail: re-upload the trimmed file rather than crop pixels in C# - the trimming
/// is a one-off on eight assets, and the sample images are the source of truth for a
/// fresh install anyway. A brand whose logo has since been replaced from admin has a
/// picture that no longer matches its seeded file, so it is left alone.
/// </summary>
[NopUpdateMigration("2026-09-12 00:00:06", "5.00", UpdateMigrationType.Data)]
public class BrandLogoTrimMigration : MigrationBase
{
    //manufacturer name -> the sample file it was seeded from (SampleData.json)
    protected static readonly Dictionary<string, string> _seededLogos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["General Plus"] = "brand_30.png",
        ["GL-General"] = "brand_28.png",
        ["Starway"] = "brand_27.png",
        ["General Tech"] = "brand_26.png",
        ["Hisense"] = "brand_25.png",
        ["Smart Electric"] = "brand_23.png",
        ["General goldin"] = "brand_22.png",
        ["Starvision"] = "brand_15.png"
    };

    /// <summary>Collect the UP migration expressions</summary>
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        //do not use DI, because it produces exception on the installation process
        var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
        var manufacturerService = EngineContext.Current.Resolve<IManufacturerService>();
        var pictureService = EngineContext.Current.Resolve<IPictureService>();

        foreach (var manufacturer in manufacturerService.GetAllManufacturersAsync(showHidden: true).Result)
        {
            if (manufacturer.PictureId == 0 || !_seededLogos.TryGetValue(manufacturer.Name, out var fileName))
                continue;

            var path = fileProvider.GetAbsolutePath("images", "samples", fileName);
            if (!fileProvider.FileExists(path))
                continue;

            pictureService.UpdatePictureAsync(
                    manufacturer.PictureId,
                    fileProvider.ReadAllBytesAsync(path).Result,
                    MimeTypes.ImagePng,
                    //a new SEO filename, because the thumb file is named after it and
                    //nothing else: replacing the binary under the same name leaves every
                    //browser that has the old logo cached showing the old logo, with no
                    //URL change to tell it otherwise
                    pictureService.GetPictureSeNameAsync($"{manufacturer.Name} logo").Result)
                .Wait();
        }
    }

    public override void Down()
    {
        //add the downgrade logic if necessary
    }
}
