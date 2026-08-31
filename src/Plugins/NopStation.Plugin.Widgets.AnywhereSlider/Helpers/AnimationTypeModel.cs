using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Helpers;

public class AnimationTypeModel
{
	public class Option
	{
		public string Text { get; set; }

		public string Value { get; set; }
	}

	public string Group { get; set; }

	public IList<Option> Options { get; set; }

	public AnimationTypeModel()
	{
		Options = new List<Option>();
	}
}
