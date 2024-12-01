using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.UI {
	public record struct Time_Radix(int Radix, string Format, bool AlwaysShow) {
		public static Time_Radix[] ParseRadices(string text) {
			Regex regex = new("\\$(\\d+)(!?):(.*?)\\$");
			Match[] matches = regex.Matches(text).ToArray();
			Time_Radix[] radices = new Time_Radix[matches.Length];
			for (int i = 0; i < matches.Length; i++) {
				radices[i] = new(int.Parse(matches[i].Groups[1].Value), matches[i].Groups[3].Value, !string.IsNullOrWhiteSpace(matches[i].Groups[2].Value));
			}
			return radices;
		}
		public static string FormatTime(int time, Time_Radix[] radices, int increment = 1, int indefiniteThreshold = 0, string indefiniteText = null) {
			if (indefiniteText is not null && time < indefiniteThreshold * increment) return indefiniteText;
			StringBuilder builder = new();
			float textTime = time / 60f;
			for (int i = 1; i <= radices.Length; i++) {
				(int radix, string format, bool alwaysShow) = radices[^i];
				if (textTime == 0 && !alwaysShow) break;
				float useTime = i == radices.Length ? textTime : textTime % radix;
				builder.Insert(0, string.Format(format, useTime));
				textTime = (int)(textTime / radix);
			}
			return builder.ToString();
		}
	}
	public class Time_Radices : ILoadable {
		public static Time_Radix[] BuffTime { get; private set; }
		internal static void Refresh() {
			BuffTime = Time_Radix.ParseRadices(Language.GetOrRegister("Mods.Origins.Generic.BuffTime").Value);
		}
		public void Load(Mod mod) {
			Refresh();
		}
		public void Unload() {
			BuffTime = null;
		}
	}
	public static class Time_Radix_Ext {
		public static string FormatTime(this Time_Radix[] radices, int time) {
			return Time_Radix.FormatTime(time, radices, int.MinValue);
		}
	}
}
