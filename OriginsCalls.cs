using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils;
using Origins.Items.Tools.Wiring;
using Origins.Tiles.Other;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins {
	public partial class Origins : Mod {
		public enum CallType {
			GetExplosiveClassesDict,
			AddBasicColorDyeShaderPass,
			AddWireMode,
			AddChambersiteOre,
			GetChambersiteOre
		}
		public enum CallTypeExtension {
			Tile,
			Wall
		}
		public override object Call(params object[] args) {
			if (!Enum.TryParse(args[0].ToString().Replace("_", ""), true, out CallType callType)) return null;
			void CallExtension(out CallTypeExtension callTypeExtension) {
				if (!Enum.TryParse(args[1].ToString().Replace("_", ""), true, out callTypeExtension)) throw new Exception($"Incorrect call extension type \"{callTypeExtension}\"");
			}
			switch (callType) {
				case CallType.GetExplosiveClassesDict:
				return DamageClasses.ExplosiveVersion;

				case CallType.AddBasicColorDyeShaderPass:
				try {
					return OriginsSets.Misc.BasicColorDyeShaderPasses.Add(((Effect)args[1], (string)args[2]));
				} catch (NullReferenceException) {
					throw new Exception("Cannot add Basic Color Dye Shader Pass after AddRecipes");
				}
				case CallType.AddWireMode:
				return WireModeLoader.AddCallWireMode(
					(Mod)args[1],
					((Delegate)args[2]).CastDelegate<Func<int, int, bool>>(),
					((Delegate)args[3]).CastDelegate<Action<int, int, bool>>(),
					(string)args[4],
					(Color)args[5],
					(Color?)args[6],
					(IEnumerable<string>)args.GetIfInRange(7) ?? [],
					(IEnumerable<string>)args.GetIfInRange(8) ?? [],
					(args.GetIfInRange(9, null) as ModItem, args.GetIfInRange(9, null) is int type ? type : ItemID.Wire),
					(bool)args.GetIfInRange(10, false),
					(string)args.GetIfInRange(11)
				);
				case CallType.AddChambersiteOre: {
					CallExtension(out CallTypeExtension callTypeExtension);
					try {
						switch (callTypeExtension) {
							case CallTypeExtension.Tile: {
								return Chambersite_Ore.Create(
									(ModTile)args[1],
									(ModItem)args[2],
									((Delegate)args[3]).CastDelegate<Func<int>>(),
									args.GetIfInRange(4) as string,
									args.GetIfInRange(5) as string,
									((Delegate)args.GetIfInRange(6))?.CastDelegate<Func<SoundStyle>>()
								);
							}
							case CallTypeExtension.Wall: {
								return null; // change when modular chambersite walls exists
							}
							default: throw new Exception($"Incorrect call extension \"{callTypeExtension}\". \"{callType}\" only accepts the extensions \"{nameof(CallTypeExtension.Tile)}\" and \"{nameof(CallTypeExtension.Wall)}\"");
						}
					} catch (NullReferenceException) {
						throw new Exception("Cannot add Chambersite Ore Pass after Load"); // idk what stage it required to allow this call to add the tile/wall
					}
				}
				case CallType.GetChambersiteOre: {
					CallExtension(out CallTypeExtension callTypeExtension);
					switch (callTypeExtension) {
						case CallTypeExtension.Tile: {
							if ((string)args[3] == "Item") return Chambersite_Ore.GetOre((int)args[2]).Item.Type;
							return Chambersite_Ore.GetOreID((int)args[2]);
						}
						case CallTypeExtension.Wall: return null; // change when modular chambersite walls exists
						default: throw new Exception($"Incorrect call extension \"{callTypeExtension}\". \"{callType}\" only accepts the extensions \"{nameof(CallTypeExtension.Tile)}\" and \"{nameof(CallTypeExtension.Wall)}\"");
					}
				}
			}
			return null;
		}
	}
}
