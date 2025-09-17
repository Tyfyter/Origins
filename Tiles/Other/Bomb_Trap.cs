using Microsoft.Xna.Framework;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
	public class Bomb_Trap : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.DontDrawTileSliced[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;
			TileID.Sets.IgnoresNearbyHalfbricksWhenDrawn[Type] = true;
			TileID.Sets.IsAMechanism[Type] = true;
			DustType = DustID.Stone;
			HitSound = SoundID.Dig;
			AddMapEntry(new Color(148, 144, 144), Language.GetText("MapObject.Trap"));
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			if (Main.LocalPlayer.direction == 1) {
				Main.tile[i, j].TileFrameX += 18;
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					NetMessage.SendTileSquare(-1, i, j);
				}
			}
		}
		public override void HitWire(int i, int j) {
			Tile tile = Main.tile[i, j];
			if (Wiring.CheckMech(i, j, 300)) {
				int dirX = 0;//(tile.TileFrameX == 0) ? (-1) : ((tile.TileFrameX == 18) ? 1 : 0);
				int dirY = 0;//(tile.TileFrameX >= 36) ? ((tile.TileFrameX >= 72) ? 1 : (-1)) : 0;
				switch (tile.TileFrameX / 18) {
					case 0:
					dirX = -1;
					break;
					case 1:
					dirX = 1;
					break;
					case 2 or 3:
					dirY = -1;
					break;
					case 4 or 5:
					dirY = 1;
					break;
				}
				int type = ProjectileType<Bomb_Trap_Grenade>();
				int damage = 50;
				float speed = 6f;
				if (Main.getGoodWorld) {
					type = ProjectileType<Bomb_Trap_Dynamite>();
					damage = 120;
					speed = 4.5f;
				} else if (Main.rand.NextBool(5)) {
					type = ProjectileType<Bomb_Trap_Bomb>();
					damage = 90;
					speed = 5f;
				}
				Vector2 position = new(i * 16 + 8 + 10 * dirX, j * 16 + 8 + 10 * dirY);
				Projectile.NewProjectile(
					Wiring.GetProjectileSource(i, j),
					position,
					new Vector2(dirX, dirY) * speed,
					type,
					damage,
					2f,
					Main.myPlayer
				);
			}
		}
		public override bool Slope(int i, int j) {
			ref short frameX = ref Main.tile[i, j].TileFrameX;
			int frame = 0;
			switch (frameX / 18) {
				case 0:
				frame = 2;
				break;
				case 1:
				frame = 3;
				break;
				case 2:
				frame = 4;
				break;
				case 3:
				frame = 5;
				break;
				case 4:
				frame = 1;
				break;
				case 5:
				frame = 0;
				break;
			}
			frameX = (short)(frame * 18);
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileSquare(-1, i, j);
			}
			return false;
		}
		public override bool IsTileDangerous(int i, int j, Player player) => true;
	}
	public class Bomb_Trap_Grenade : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Grenade;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.Explosive[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.friendly = false;
			Projectile.trap = true;
			Projectile.timeLeft = 60;
			AIType = ProjectileID.Grenade;
		}
		public override void AI() {
			if (Projectile.timeLeft <= 3) return;
			foreach (Player player in Main.ActivePlayers) {
				if (player.Hitbox.Intersects(Projectile.Hitbox)) {
					Projectile.timeLeft = 3;
					return;
				}
			}
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.Hitbox.Intersects(Projectile.Hitbox)) {
					Projectile.timeLeft = 3;
					return;
				}
			}
		}
		public override void PrepareBombToBlow() {
			Projectile.friendly = true;
			Projectile.hostile = true;
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item14);
		}
	}
	public class Bomb_Trap_Bomb : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bomb;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.trap = true;
			Projectile.timeLeft = 90;
			AIType = ProjectileID.Grenade;
		}
		public override void PrepareBombToBlow() {
			Projectile.friendly = true;
			Projectile.hostile = true;
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item14);
		}
	}
	public class Bomb_Trap_Dynamite : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Dynamite;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Dynamite);
			Projectile.trap = true;
			Projectile.timeLeft = 100;
			AIType = ProjectileID.Grenade;
		}
		public override void PrepareBombToBlow() {
			Projectile.friendly = true;
			Projectile.hostile = true;
			Projectile.Resize(250, 250);
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item14);
		}
	}
	public class Bomb_Trap_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Bomb_Trap>());
			Item.value = Item.sellPrice(silver: 35);
		}
	}
}
