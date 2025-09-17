using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Materials;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
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
			ID = Type;
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			ModContent.GetInstance<Traffic_Cone_TE_System>().AddTileEntity(new(i, j));
		}
	}
	public class Traffic_Cone_TE_System : TESystem {
		public HashSet<Point16> projLocations;
		public override void PreUpdateEntities() {
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			projLocations ??= new();
			for (int i = 0; i < tileEntityLocations.Count; i++) {
				Point16 pos = tileEntityLocations[i];
				if (Main.tile[pos.X, pos.Y].TileIsType(Traffic_Cone.ID)) {
					if (!projLocations.Contains(pos)) {
						Projectile.NewProjectile(
							Entity.GetSource_None(),
							new Vector2(pos.X + 0.5f, pos.Y + 0.5f) * 16,
							default,
							Traffic_Cone_Projectile.ID,
							0,
							0,
							Owner: Main.maxPlayers
						);
					}
				} else {
					tileEntityLocations.RemoveAt(i);
					i--;
					continue;
				}
			}
			projLocations.Clear();
		}
		internal static bool RegisterProjPosition(Point16 pos) {
			Traffic_Cone_TE_System instance = ModContent.GetInstance<Traffic_Cone_TE_System>();
			instance.projLocations ??= new();
			return instance.projLocations.Add(pos);
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
			Projectile.hide = true;
		}
		public override void AI() {
			const float range = 12 * 16; // 12 tiles
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (npc.CanBeChasedBy(Projectile)) {
					if (Projectile.position.DistanceSQ(Projectile.position.Clamp(npc.Hitbox)) < range * range) {
						int index = npc.FindBuffIndex(Slow_Debuff.ID);
						if (index >= 0) {
							if (npc.buffTime[index] < 10) npc.buffTime[index] = 10;
						} else {
							npc.AddBuff(Slow_Debuff.ID, 10);
						}
					}
				}
			}
			foreach (Player player in Main.ActivePlayers) {
				if (Projectile.position.DistanceSQ(Projectile.position.Clamp(player.Hitbox)) < range * range) {
					player.OriginPlayer().nearTrafficCone = 30;
				}
			}
			Projectile.timeLeft = 5;
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			Point16 tilePos = Projectile.position.ToTileCoordinates16();
			if (Main.tile[tilePos.X, tilePos.Y].TileIsType(Traffic_Cone.ID)) {
				if (!Traffic_Cone_TE_System.RegisterProjPosition(tilePos)) {
					Projectile.Kill();
				}
			} else {
				Projectile.Kill();
			}
		}
	}
	public class Traffic_Cone_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Traffic_Cone>());
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Rubber>(), 6)
			.AddTile(TileID.HeavyWorkBench)
			.Register();
		}
	}
}
