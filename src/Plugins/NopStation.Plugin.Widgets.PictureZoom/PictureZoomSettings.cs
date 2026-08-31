using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.PictureZoom;

public class PictureZoomSettings : ISettings
{
	public bool EnablePictureZoom { get; set; }

	public double ZoomWidth { get; set; } = 1.0;

	public double ZoomHeight { get; set; } = 1.0;

	public int LtrPositionTypeId { get; set; } = 10;

	public int RtlPositionTypeId { get; set; } = 10;

	public bool Tint { get; set; }

	public double TintOpacity { get; set; } = 0.5;

	public double LensOpacity { get; set; } = 0.5;

	public bool SoftFocus { get; set; }

	public int SmoothMove { get; set; } = 3;

	public bool ShowTitle { get; set; } = true;

	public double TitleOpacity { get; set; } = 0.5;

	public int AdjustX { get; set; }

	public int AdjustY { get; set; }

	public int ImageSize { get; set; } = 500;

	public int FullSizeImage { get; set; } = 1000;
}
