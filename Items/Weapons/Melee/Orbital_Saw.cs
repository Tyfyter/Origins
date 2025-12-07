using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Orbital_Saw : ModItem {
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThornChakram);
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 16;
			Item.width = 20;
			Item.height = 22;
			Item.useTime = 13;
			Item.useAnimation = 13;
			Item.shoot = ModContent.ProjectileType<Orbital_Saw_P>();
			Item.shootSpeed = 10.75f;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 9)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}
	}
	public class Orbital_Saw_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.penetrate = -1;
			Projectile.width = 34;
			Projectile.height = 34;
			Projectile.ignoreWater = true;
			AIType = ProjectileID.FruitcakeChakram;
			DrawOriginOffsetX = 0;
			DrawOriginOffsetY = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 6;
		}
		public override bool PreAI() {
			if (Projectile.localAI[0] == 0 || Projectile.localAI[0] > 45) Projectile.aiStyle = 3;
			return true;
		}
		public override void PostAI() {
			if (Projectile.ai[0] == 1 && Projectile.localAI[0] == 0) {
				Projectile.localAI[0] = 1;
				Projectile.aiStyle = 0;
				Projectile.penetrate = -1;
			} else if (Projectile.aiStyle == 0) {
				if (++Projectile.localAI[0] > 45) {
					Projectile.aiStyle = ProjAIStyleID.Boomerang;
				} else {
					Projectile.velocity *= 0.9f;
					if (Projectile.soundDelay == 0) {
						Projectile.soundDelay = 6;
						Terraria.Audio.SoundEngine.PlaySound(in SoundID.Item7, Projectile.position);
						bool sounded = false;
						Vector2 tileCenter = Vector2.Zero;
						int tileCount = 0;
						foreach (Point tile in Collision.GetTilesIn(Projectile.position, Projectile.BottomRight)) {
							if (Framing.GetTileSafely(tile).HasFullSolidTile()) {
								WorldGen.KillTile(tile.X, tile.Y, fail: true, effectOnly: true);
								if (!sounded) {
									sounded = true;
									Terraria.Audio.SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
								}
								tileCenter += tile.ToWorldCoordinates();
								tileCount++;
							}
						}
						if (tileCount > 0) {
							tileCenter /= tileCount;
							Vector2 dir = (tileCenter - Projectile.Center).WithMaxLength(6).RotatedBy(Projectile.direction * MathHelper.PiOver2);
							for (int i = 0; i < 4; i++) {
								Dust.NewDustPerfect(
									tileCenter,
									DustID.Torch,
									dir.RotatedByRandom(1f)
								).noGravity = true;
							}
						}
					}
					Projectile.rotation += 0.533f * Projectile.direction;
				}
			}
		}
		public override bool? CanHitNPC(NPC target) {
			Projectile.aiStyle = 0;
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.ai[0] != 1 || Projectile.localAI[0] <= 45) {
				Projectile.velocity *= 0.25f;
			} else {
				Projectile.velocity *= 0.5f;
			}
			Projectile.netUpdate = true;
			Rectangle overlap = Rectangle.Intersect(Projectile.Hitbox, target.Hitbox);
			Vector2 dir = (overlap.Center.ToVector2() - Projectile.Center).WithMaxLength(6).RotatedBy(Projectile.direction * MathHelper.PiOver2);
			for (int i = 0; i < 4; i++) {
				Dust.NewDustPerfect(
					Main.rand.NextVector2FromRectangle(overlap),
					DustID.Torch,
					dir.RotatedByRandom(1f)
				).noGravity = true;
			}
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 27;
			height = 27;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = oldVelocity * 0.25f;
			float len = Projectile.velocity.Length() / 3;
			if (len > 1) Projectile.velocity /= len;
			if (Projectile.ai[0] != 1) Projectile.netUpdate = true;
			Projectile.ai[0] = 1;
			return false;
		}
	}
}
