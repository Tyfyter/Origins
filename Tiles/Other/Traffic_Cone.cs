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
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<Traffic_Cone_TE>().Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			ID = Type;
			TileEntity._UpdateEnd += Traffic_Cone_TE.ResetLocations;
		}
		public override void Unload() {
			TileEntity._UpdateEnd -= Traffic_Cone_TE.ResetLocations;
		}
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			if (!Main.tile[i, j].TileIsType(ID)) {
				ModContent.GetInstance<Traffic_Cone_TE>().Kill(i, j);
			}
		}
	}
	public class Traffic_Cone_TE : ModTileEntity {
		public static new int ID { get; private set; } = -1;
		public override bool IsTileValidForEntity(int x, int y) => Main.tile[x, y].TileIsType(Traffic_Cone.ID);
		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
			return Place(i, j);
		}
		public static HashSet<Point16> coneLocations;
		public override void Update() {
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			if (!IsTileValidForEntity(Position.X, Position.Y)) {
				return;
			}
			if (ID == -1) ID = ModContent.TileEntityType<Traffic_Cone_TE>();
			coneLocations ??= new();
			if (!coneLocations.Contains(Position)) {
				Projectile.NewProjectile(
					Entity.GetSource_None(),
					Position.ToWorldCoordinates(0, 0),
					default,
					Traffic_Cone_Projectile.ID,
					0,
					0
				);
			}
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
				if (npc.CanBeChasedBy(Projectile) && npc.DistanceSQ(Projectile.position) < range * range) {
					int index = npc.FindBuffIndex(Slow_Debuff.ID);
					if (index >= 0) {
						npc.buffTime[index] = 10;
					} else {
						npc.AddBuff(Slow_Debuff.ID, 10);
					}
				}
			}
			Projectile.timeLeft = 60;
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			Point16 tilePos = Projectile.position.ToTileCoordinates16();
			if (Main.tile[tilePos.X, tilePos.Y].TileIsType(Traffic_Cone.ID)) {
				Traffic_Cone_TE.coneLocations ??= new();
				Traffic_Cone_TE.coneLocations.Add(tilePos);
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
