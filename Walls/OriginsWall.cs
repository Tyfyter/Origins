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
		public virtual bool CanBeReplacedByWallSpread => true;
		public virtual int TileItem => -1;
		public Dictionary<WallVersion, OriginsWall> Versions { get; private set; }
		public int GetWallID(WallVersion version) => Versions[version].Type;
		public static int GetWallID<T>(WallVersion version) where T : OriginsWall => ContentInstance<T>.Instances[0].GetWallID(version);
		public override void SetStaticDefaults() {
			AddMapEntry(MapColor);
			if (!CanBeReplacedByWallSpread) WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;
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
			return true;
		}
		public OriginsWall() : base() {}
	}
	[Autoload(false)]
	public class OriginsWallItem(OriginsWall wall) : ModItem() {
		public OriginsWall Wall { get; private set; } = wall;
		public override string Texture => Wall.Texture + "_Item";
		public override string Name => Wall.Name + "_Item";
		protected override bool CloneNewInstances => true;
		public override ModItem Clone(Item newEntity) {
			OriginsWallItem clone = (OriginsWallItem)base.Clone(newEntity);
			clone.Wall = Wall;
			return clone;
		}

		public override void SetStaticDefaults() {
			if (Wall.WallVersion == WallVersion.Placed_Unsafe) {
				ItemID.Sets.DrawUnsafeIndicator[Type] = true;
				if (Wall.WallVersions.HasFlag(WallVersion.Safe)) {
					ItemID.Sets.ShimmerTransformToItem[Type - 1] = Type;
				}
			}
			Item.ResearchUnlockCount = 400;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(Wall.Type);
		}
		public override void AddRecipes() {
			if (Wall.WallVersion == WallVersion.Safe && Wall.TileItem >= 0) {
				Recipe.Create(Type, 4)
				.AddIngredient(Wall.TileItem)
				.AddTile(TileID.WorkBenches)
				.Register();

				Recipe.Create(Wall.TileItem)
				.AddIngredient(Type, 4)
				.AddTile(TileID.WorkBenches)
				.Register();
			}
		}
	}
}
