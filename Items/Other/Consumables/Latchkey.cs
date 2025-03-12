using CalamityMod.NPCs.TownNPCs;
using Origins.Dusts;
using Origins.Items.Accessories;
using Origins.Tiles;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Empowerments;

namespace Origins.Items.Other.Consumables {
	public class Latchkey : ModItem {
        public string[] Categories => [
            "ExpendableTool"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.rare = ItemRarityID.Green;
			Item.width = 18;
			Item.height = 20;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.UseSound = SoundID.Item1;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.shootSpeed = 4;
			Item.shoot = ModContent.ProjectileType<Latchkey_P>();
			Item.value = Item.sellPrice(copper: 20);
		}
		public override void AddRecipes() {
			/*Recipe.Create(Type, 10)
			.AddIngredient(ItemID.BottledWater, 10)
			.AddIngredient(ModContent.ItemType<Riven_Grass_Seeds>())
            .AddIngredient(ModContent.ItemType<Silica_Item>())
            .Register();*/
		}
	}
	public class Latchkey_P : ModProjectile {
		public const int max_updates = 4;
		public override string Texture => typeof(Refactoring_Pieces).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.DD2SquireSonicBoom);
			Projectile.width = 20;
			Projectile.height = 42;
			Projectile.alpha = 0;
			Projectile.MaxUpdates = max_updates;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 10 * max_updates;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			Player owner = Main.player[Projectile.owner];

			if (Projectile.ai[0] > 0) {
				if (--Projectile.ai[0] <= 0) Projectile.Kill();
			} else {
				Vector2 pos = Projectile.position;
				Rectangle hitbox = Projectile.Hitbox;
				for (int i = 0; i < 11 * 4; i++) {
					pos += Projectile.velocity;
					hitbox.X = (int)pos.X;
					hitbox.Y = (int)pos.Y;
					bool shouldBreak = false;
					switch (OverlapsAnyTiles(hitbox)) {
						case 0:
						if (i == 0 || Projectile.ai[0] != 0) {
							shouldBreak = true;
						} else {
							Projectile.ai[0] = i;
						}
						break;
						case 2:
						Projectile.Kill();
						owner.velocity = Vector2.Zero;
						return;
					}
					if (shouldBreak) break;
				}
			}

			owner.Center = Projectile.Center;
			owner.velocity = Projectile.velocity;
			if (++Projectile.frameCounter >= 14) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
			}
		}
		public override void OnKill(int timeLeft) {
			// dusts and/or gores here
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			//hitbox.Inflate(4, 12);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Player owner = Main.player[Projectile.owner];
			owner.Center = Projectile.Center;
			owner.velocity = Projectile.velocity;

			return false;
		}
		public override bool PreDraw(ref Color lightColor) {

			return false;
		}
		static int OverlapsAnyTiles(Rectangle area) {
			Rectangle checkArea = area;
			Point topLeft = area.TopLeft().ToTileCoordinates();
			Point bottomRight = area.BottomRight().ToTileCoordinates();
			int minX = Utils.Clamp(topLeft.X, 0, Main.maxTilesX - 1);
			int minY = Utils.Clamp(topLeft.Y, 0, Main.maxTilesY - 1);
			int maxX = Utils.Clamp(bottomRight.X, 0, Main.maxTilesX - 1) - minX;
			int maxY = Utils.Clamp(bottomRight.Y, 0, Main.maxTilesY - 1) - minY;
			int cornerX = area.X - topLeft.X * 16;
			int cornerY = area.Y - topLeft.Y * 16;
			int highestValue = 0;
			for (int i = 0; i <= maxX; i++) {
				for (int j = 0; j <= maxY; j++) {
					Tile tile = Main.tile[i + minX, j + minY];
					if (tile != null && tile.HasSolidTile()) {
						checkArea.X = i * -16 + cornerX;
						checkArea.Y = j * -16 + cornerY;
						int value = TileLoader.GetTile(tile.TileType) is IDefiledTile ? 1 : 2;
						if (highestValue >= value) continue;
						if (tile.Slope != SlopeType.Solid) {
							if (CollisionExtensions.TileTriangles[(int)tile.Slope - 1].Intersects(checkArea)) highestValue = value;
						} else {
							if (CollisionExtensions.TileRectangles[(int)tile.BlockType].Intersects(checkArea)) highestValue = value;
						}
					}
				}
			}
			return highestValue;
		}
	}
}
