using Origins.NPCs;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.Social.Steam;

namespace Origins {
	public class SortOfRequireMusic : ILoadable {
		public void Load(Mod mod) {
			try {
				MonoModHooks.Add(typeof(WorkshopSocialModule).GetMethod("TryCalculateWorkshopDeps", BindingFlags.NonPublic | BindingFlags.Static), TryCalculateWorkshopDeps);
			} catch (Exception e) {
#if DEBUG
				throw;
#endif
#pragma warning disable CS0162 // Unreachable code detected
				mod.Logger.Error("Error while sort of requiring the music mod: ", e);
#pragma warning restore CS0162 // Unreachable code detected
			}
		}
		public void Unload() { }
		delegate bool orig_TryCalculateWorkshopDeps(ref NameValueCollection buildData);
		static bool TryCalculateWorkshopDeps(orig_TryCalculateWorkshopDeps orig, ref NameValueCollection buildData) {
			string orig_modreferences = buildData["modreferences"];
			if (buildData["name"] == "Origins") {
				string extraRequirements = ", OriginsMusic";
				if (!TryGetMusicModRequirements(out string[] musicRequirements)) return false;
				for (int i = 0; i < musicRequirements.Length; i++) {
					if (!orig_modreferences.Contains(musicRequirements[i])) extraRequirements += ", " + musicRequirements[i];
				}
				buildData["modreferences"] = orig_modreferences + extraRequirements;
			}

			bool value = orig(ref buildData);
			buildData["modreferences"] = orig_modreferences;
			return value;
		}
		static bool TryGetMusicModRequirements(out string[] requirements) {
			string path = Path.Combine(Program.SavePathShared, "ModSources", "OriginsMusic", "build.txt");
			static bool ParseLine(string line, out string[] reqs) {
				reqs = [];
				if (string.IsNullOrWhiteSpace(line)) {
					return false;
				}
				int split = line.IndexOf('=');
				if (split < 0)
					return false; // lines without an '=' are ignored
				string property = line[..split].Trim();
				string value = line[(split + 1)..].Trim();
				if (value.Length == 0) {
					return false;
				}
				if (property == "modReferences") {
					reqs = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(l => l.Split('@')[0]).ToArray();
					return true;
				}
				return false;
			}
			if (File.Exists(path)) {
				foreach (string line in File.ReadAllLines(path)) {
					if (ParseLine(line, out string[] reqs)) {
						requirements = reqs;
						return true;
					}
				}
			}
			if (ModLoader.TryGetMod("OriginsMusic", out Mod musicMod)) {
				foreach (ReadOnlySpan<char> line in Encoding.UTF8.GetString(musicMod.GetFileBytes("data/points.txt")).Split('\n')) {
					if (ParseLine(line.ToString(), out string[] reqs)) {
						requirements = reqs;
						return true;
					}
				}
			}
			requirements = null;
			return false;
		}
	}
}
