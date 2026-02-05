using AltLibrary.Common.AltBiomes;
using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Weapons.Ranged;
using Origins.NPCs.Defiled;
using Origins.NPCs.Riven;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Utilities.NPCUtils;

namespace Origins.Items.Materials {
	public class Sentient_Powder : ModItem {
		static short glowmask;
        public string[] Categories => [
            WikiCategories.ExpendableTool
        ];
        public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.PurificationPowder;
			Item.ResearchUnlockCount = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[ItemID.VilePowder];
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.VilePowder);
			Item.shoot = ModContent.ProjectileType<Sentient_Powder_P>();
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 5)
			.AddIngredient(ModContent.ItemType<Acetabularia_Item>())
			.AddTile(TileID.Bottles)
			.Register();

            Recipe.Create(ModContent.ItemType<Gelled_Knife>(), 50)
            .AddIngredient(ItemID.ThrowingKnife, 50)
            .AddIngredient(this)
            .Register();
        }
	}
	public class Sentient_Powder_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.VilePowder;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.VilePowder);
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			Projectile.velocity *= 0.95f;
			Projectile.ai[0] += 1f;
			if (Projectile.ai[0] == 180f) {
				Projectile.Kill();
			}
			if (Projectile.ai[1] == 0f) {
				Projectile.ai[1] = 1f;
				for (int i = 0; i < 30; i++) {
					Dust.NewDust(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						ModContent.DustType<Generic_Powder_Dust>(),
						Projectile.velocity.X,
						Projectile.velocity.Y,
						50,
						new Color(0.3f, 0.5f, 0.7f, 0.1f)
					);
				}
			}
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				Rectangle hitbox = Projectile.Hitbox;
				foreach (NPC target in Main.ActiveNPCs) {
					if (target.Hitbox.Intersects(hitbox)) {
						int targetType = -1;
						switch (target.type) {
							case NPCID.Bunny or NPCID.BunnySlimed or NPCID.BunnyXmas or NPCID.PartyBunny:
							targetType = ModContent.NPCType<Barnacle_Bunny>();
							break;
							case NPCID.Penguin or NPCID.PenguinBlack:
							targetType = ModContent.NPCType<Riven_Penguin>();
							break;
							case NPCID.Goldfish or NPCID.GoldfishWalker:
							targetType = ModContent.NPCType<Bottomfeeder>();
							break;
						}
						if (targetType != -1) {
							target.Transform(targetType);
						}
					}
				}
			}
			if (Main.myPlayer != Projectile.owner) return;
			int minX = (int)(Projectile.position.X / 16f) - 1;
			int maxX = (int)((Projectile.position.X + Projectile.width) / 16f) + 2;
			int minY = (int)(Projectile.position.Y / 16f) - 1;
			int maxY = (int)((Projectile.position.Y + Projectile.height) / 16f) + 2;
			if (minX < 0) {
				minX = 0;
			}
			if (maxX > Main.maxTilesX) {
				maxX = Main.maxTilesX;
			}
			if (minY < 0) {
				minY = 0;
			}
			if (maxY > Main.maxTilesY) {
				maxY = Main.maxTilesY;
			}
			Vector2 comparePos = default;
			for (int x = minX; x < maxX; x++) {
				for (int y = minY; y < maxY; y++) {
					comparePos.X = x * 16;
					comparePos.Y = y * 16;
					if ((Projectile.position.X + Projectile.width > comparePos.X) &&
						(Projectile.position.X < comparePos.X + 16f) &&
						(Projectile.position.Y + Projectile.height > comparePos.Y) &&
						(Projectile.position.Y < comparePos.Y + 16f) &&
						Main.tile[x, y].HasTile) {
						AltLibrary.Core.ALConvert.Convert<Riven_Hive_Alt_Biome>(x, y, 0);
					}
				}
			}
		}
		public override bool? CanCutTiles() => false;
	}
}
