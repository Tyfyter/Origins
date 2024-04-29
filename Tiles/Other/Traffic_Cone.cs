using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
    public class Traffic_Cone : ModTile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(200, 80, 0), Language.GetOrRegister(this.GetLocalizationKey("DisplayName")));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.addTile(Type);
			ID = Type;
		}
		public static HashSet<Point16> coneLocations;
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			if (Main.tile[i, j].TileFrameY != 0) return true;
			coneLocations ??= new();
			if (!coneLocations.Contains(new(i, j))) {
				Projectile.NewProjectile(
					Entity.GetSource_None(),
					new Vector2(i + 0.5f, j + 0.5f) * 16,
					default,
					Traffic_Cone_Projectile.ID,
					0,
					0,
					Owner: Main.maxPlayers
				);
			}
			return true;
		}
		internal static void ResetLocations() {
			coneLocations ??= new();
			coneLocations.Clear();
		}
	}
	public class Traffic_Cone_Projectile : ModProjectile {
		public override string Texture => typeof(Traffic_Cone_Item).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.tileCollide = false;
			Projectile.netImportant = true;
			//Projectile.hide = true;
		}
		public override void AI() {
			Dust.NewDust(Projectile.position, 0, 0, DustID.Torch);
			const float range = 12 * 16; // 12 tiles
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (npc.CanBeChasedBy(Projectile)) {
					if (Projectile.position.DistanceSQ(Projectile.position.Clamp(npc.Hitbox)) < range * range) {
						int index = npc.FindBuffIndex(Slow_Debuff.ID);
						if (index >= 0) {
							npc.buffTime[index] = 10;
						} else {
							npc.AddBuff(Slow_Debuff.ID, 10);
						}
					}
				}
			}
			Projectile.timeLeft = 5;
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			Point16 tilePos = Projectile.position.ToTileCoordinates16();
			if (Main.tile[tilePos.X, tilePos.Y].TileIsType(Traffic_Cone.ID)) {
				Traffic_Cone.coneLocations ??= new();
				if (!Traffic_Cone.coneLocations.Add(tilePos)) {
					Projectile.Kill();
				}
			} else {
				Projectile.Kill();
			}
		}
	}
	public class Traffic_Cone_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LampPost);
			Item.createTile = ModContent.TileType<Traffic_Cone>();
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 6);
			recipe.AddTile(TileID.HeavyWorkBench);
			recipe.Register();
		}
	}
}
