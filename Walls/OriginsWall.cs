using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Walls {
	[Flags]
	public enum WallVersion : byte {
		None = 0b0000,
		Natural = 0b0001,
		Safe = 0b0010,
		Placed_Unsafe = 0b0100
	}
	public abstract class OriginsWall : ModWall {
		public abstract WallVersion WallVersions { get; }
		public abstract Color MapColor { get; }
		public virtual int TileItem => -1;
		public Dictionary<WallVersion, OriginsWall> Versions { get; private set; }
		public int GetWallID(WallVersion version) => Versions[version].Type;
		public static int GetWallID<T>(WallVersion version) where T : OriginsWall => ModContent.GetInstance<T>().GetWallID(version);
		public override void SetStaticDefaults() {
			AddMapEntry(MapColor);
			if (WallVersion == WallVersion.Safe) {
				Main.wallHouse[Type] = true;
			}
			base.DustType = DustType;
			base.HitSound = HitSound;
			base.VanillaFallbackOnModDeletion = VanillaFallbackOnModDeletion;
		}
		public override string Name => $"{base.Name}_{WallVersion}";
		public override string Texture => (GetType().Namespace + "." + GetType().Name).Replace('.', '/');
		//
		// Summary:
		//     The default type of dust made when this tile/wall is hit. Defaults to 0.
		public virtual new int DustType => 0;
		//
		// Summary:
		//     The default style of sound made when this tile/wall is hit.
		//     Defaults to SoundID.Dig, which is the sound used for tiles such as dirt and sand.
		public virtual new SoundStyle? HitSound => SoundID.Dig;
		//
		// Summary:
		//     The vanilla ID of what should replace the instance when a user unloads and subsequently
		//     deletes data from your mod in their save file. Defaults to 0.
		public virtual new ushort VanillaFallbackOnModDeletion => 0;

		public WallVersion WallVersion { get; private set; } = WallVersion.None;
		public override bool IsLoadingEnabled(Mod mod) {
			if (WallVersion == WallVersion.None) {
				ConstructorInfo ctor = GetType().GetConstructor(Array.Empty<Type>());
				Dictionary<WallVersion, OriginsWall> versions = new();
				bool first = true;
				foreach (WallVersion version in WallVersions.GetFlags()) {
					OriginsWall newWall;
					if (!first) {
						newWall = (OriginsWall)ctor.Invoke(Array.Empty<object>());
						newWall.WallVersion = version;
						mod.AddContent(newWall);
					} else {
						WallVersion = version;
						newWall = this;
						first = false;
					}
					newWall.Versions = versions;
					versions.Add(version, newWall);
					switch (version) {
						case WallVersion.Safe:
						case WallVersion.Placed_Unsafe:
						mod.AddContent(new OriginsWallItem(newWall));
						break;
					}
				}
				return true;
			}
			return false;
		}
		public OriginsWall() : base() {}
	}
	[Autoload(false)]
	public class OriginsWallItem : ModItem {
		public OriginsWall wall { get; private set; }
		public override string Texture => wall.Texture + "_Item";
		public override string Name => wall.Name + "_Item";
		protected override bool CloneNewInstances => true;
		public override ModItem Clone(Item newEntity) {
			OriginsWallItem clone = (OriginsWallItem)base.Clone(newEntity);
			clone.wall = wall;
			return clone;
		}
		public OriginsWallItem(OriginsWall wall) : base() {
			this.wall = wall;
		}
		public override void SetStaticDefaults() {
			if (wall.WallVersion == WallVersion.Placed_Unsafe) {
				ItemID.Sets.DrawUnsafeIndicator[Type] = true;
				if (wall.WallVersions.HasFlag(WallVersion.Safe)) {
					ItemID.Sets.ShimmerTransformToItem[Type - 1] = Type;
				}
			}
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = wall.Type;
		}
		public override void AddRecipes() {
			if (wall.WallVersion == WallVersion.Safe && wall.TileItem >= 0) {
				Recipe.Create(Type, 4)
				.AddIngredient(wall.TileItem)
				.AddTile(TileID.WorkBenches)
				.Register();

				Recipe.Create(wall.TileItem)
				.AddIngredient(Type, 4)
				.AddTile(TileID.WorkBenches)
				.Register();
			}
		}
	}
}
