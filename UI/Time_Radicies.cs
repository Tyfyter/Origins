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
			Regex regex = new("(?:\\$(\\d+)(!?):(.*?)\\$)|([^$]+)");
			Match[] matches = regex.Matches(text).ToArray();
			Time_Radix[] radices = new Time_Radix[matches.Length];
			for (int i = 0; i < matches.Length; i++) {
				if (matches[i].Groups[4].Success) {
					string plainText = matches[i].Groups[4].Value;
					radices[i] = new(0, plainText, plainText.Length > 1 && plainText.StartsWith('!'));
				} else {
					radices[i] = new(int.Parse(matches[i].Groups[1].Value), matches[i].Groups[3].Value, !string.IsNullOrWhiteSpace(matches[i].Groups[2].Value));
				}
			}
			return radices;
		}
		public static string FormatTime(int time, Time_Radix[] radices, int increment = 1, int indefiniteThreshold = 0, string indefiniteText = null) {
			if (indefiniteText is not null && time < indefiniteThreshold * increment) return indefiniteText;
			StringBuilder builder = new();
			float textTime = time / 60f;
			for (int i = 1; i <= radices.Length; i++) {
				(int radix, string format, bool alwaysShow) = radices[^i];
				if (radix == 0) {
					builder.Insert(0, format);
					continue;
				}
				if (textTime == 0 && !alwaysShow) break;
				float useTime = i == radices.Length ? textTime : textTime % radix;
				builder.Insert(0, string.Format(format, useTime));
				textTime = (int)(textTime / radix);
			}
			return builder.ToString();
		}
	}
	public sealed class Time_Radices : IDisposable {
		readonly LocalizedText key;
		Time_Radix[] radices;
		public Time_Radices(string key) : this(Language.GetOrRegister(key)) { }
		public Time_Radices(LocalizedText key) {
			this.key = key;
			Regenerate();
			RefreshHandler.radices.Add(new(this));
		}
		void Regenerate() => radices = Time_Radix.ParseRadices(key.Value);
		public static Time_Radices BuffTime { get; private set; }
		class RefreshHandler : ModSystem {
			public static List<WeakReference> radices = [];
			public override void Load() {
				BuffTime = new("Mods.Origins.Generic.BuffTime");
			}
			public override void OnLocalizationsLoaded() {
				radices.RemoveAll(r => !r.IsAlive);
				for (int i = 0; i < radices.Count; i++) ((Time_Radices)radices[i].Target).Regenerate();
			}
			public override void Unload() {
				BuffTime = null;
			}
		}
		public string FormatTime(int time) => radices.FormatTime(time);
		void IDisposable.Dispose() {
			for (int i = RefreshHandler.radices.Count - 1; i >= 0; i--) {
				if (ReferenceEquals(RefreshHandler.radices[i].Target, this)) {
					RefreshHandler.radices.RemoveAt(i);
					break;
				}
			}
			radices = null;
		}
	}
	public static class Time_Radix_Ext {
		public static string FormatTime(this Time_Radix[] radices, int time) {
			return Time_Radix.FormatTime(time, radices, int.MinValue);
		}
	}
}
