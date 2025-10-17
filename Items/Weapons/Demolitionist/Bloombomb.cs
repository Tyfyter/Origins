using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Microsoft.Xna.Framework.MathHelper;
using static Origins.OriginExtensions;
namespace Origins.Items.Weapons.Demolitionist {
	public class Bloombomb : ModItem, ICustomWikiStat {
        public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Bomb);
			Item.damage = 35;
			Item.shoot = ModContent.ProjectileType<Bloombomb_P>();
			Item.ammo = ItemID.Bomb;
			Item.value = Item.sellPrice(silver: 4);
			Item.rare = ItemRarityID.Green;
			Item.ArmorPenetration += 4;
        }
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ItemID.Bomb)
            .AddIngredient(ItemID.Seed, 20)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Bloombomb_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Bloombomb";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.timeLeft = 135;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			int t = Bloombomb_Seed.ID;
			int count = 14 - Main.rand.Next(3);
			float rot = TwoPi / count;
			for (int i = count; i > 0; i--) {
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					(Vec2FromPolar(rot * i, 6) + Main.rand.NextVector2Unit()) + (Projectile.velocity / 12),
					t,
					Projectile.damage / 8,
					6,
					Projectile.owner
				);
			}
		}
	}
	public class Bloombomb_Seed : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Seed;
		public static int ID { get; private set; } = 0;
		public override void SetStaticDefaults() {
			ID = Projectile.type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Seed);
			Projectile.DamageType = DamageClasses.ThrownExplosive;
            Projectile.timeLeft = 240;
            Projectile.penetrate = 3;
			Projectile.ArmorPenetration += 10;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Vector2 realPos = Projectile.oldPosition + oldVelocity;
			float halfWidth = Projectile.width;
			float halfHeight = Projectile.height;
			for (int x = -1; x < 2; x++) {
				for (int y = -1; y < 2; y++) {
					Tile tile = Framing.GetTileSafely((realPos + new Vector2(halfWidth * x, halfHeight * y)).ToTileCoordinates());
					if (tile.HasTile && (TileID.Sets.Grass[tile.TileType] || TileID.Sets.GrassSpecial[tile.TileType])) {
						if (Projectile.velocity.X != oldVelocity.X) {
							Projectile.velocity.X = -oldVelocity.X;
						}
						if (Projectile.velocity.Y != oldVelocity.Y) {
							Projectile.velocity.Y = -oldVelocity.Y;
						}
						return false;
					}
				}
			}
			Point pos = realPos.ToTileCoordinates();
			for (int x = -1; x < 2; x++) {
				for (int y = -1; y < 2; y++) {
					Tile tile = Framing.GetTileSafely(pos.X + x, pos.Y + y);
					if (tile.HasTile) {
						if (tile.TileType == TileID.Dirt) {
							tile.TileType = TileID.Grass;
							WorldGen.SquareTileFrame(pos.X + x, pos.Y + y);
						} else if (tile.TileType == TileID.Mud) {
							tile.TileType = TileID.JungleGrass;
							WorldGen.SquareTileFrame(pos.X + x, pos.Y + y);
						}
					}
				}
			}
			return true;
		}
	}
}
