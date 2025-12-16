using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using Terraria.Audio;
namespace Origins.Items.Weapons.Melee {
	public class Riverang : ModItem {
		internal static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThornChakram);
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 23;
			Item.width = 20;
			Item.height = 22;
			Item.useTime = 13;
			Item.useAnimation = 13;
			Item.shoot = ModContent.ProjectileType<Riverang_P>();
			Item.shootSpeed = 10.75f;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 9)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}
	}
	public class Riverang_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Riverang";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.penetrate = -1;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.ignoreWater = true;
			AIType = ProjectileID.FruitcakeChakram;
		}
		public override bool PreAI() {
			Projectile.aiStyle = ProjAIStyleID.Boomerang;
			return true;
		}
		public override void PostAI() {
			bool wet = false;
			if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height) && !Collision.honey) {
				wet = true;
			}
			Vector2 targetPos = Projectile.Center;
			bool foundTarget = false;
			Vector2 testPos;
			float targetDist = wet ? 80 : 40;
			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
				if (!wet) goto skip;
			}
			if (Projectile.localAI[0] > 0) {
				Projectile.localAI[0]--;
				goto skip;
			}
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC target = Main.npc[i];
				if (target.CanBeChasedBy()) {
					testPos = Projectile.Center.Clamp(target.Hitbox);
					Vector2 difference = testPos - Projectile.Center;
					float distance = difference.Length();
					bool closest = Vector2.Distance(Projectile.Center, targetPos) > distance;
					bool inRange = distance < targetDist && Vector2.Dot(difference.SafeNormalize(Vector2.Zero), Projectile.velocity.SafeNormalize(Vector2.Zero)) > 0.2f;
					if ((!foundTarget || closest) && inRange) {
						targetPos = testPos;
						foundTarget = true;
						targetDist = distance;
					}
				}
			}
			skip:
			if (foundTarget) {
				Projectile.velocity = (targetPos - Projectile.Center).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
				Projectile.localAI[1] = 10;
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height) && !Collision.honey) {
				Projectile.aiStyle = 0;
			}
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.localAI[1] > 0) Projectile.localAI[0] = 20;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 27;
			height = 27;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height) && !Collision.honey) {
				if (Projectile.velocity.X != oldVelocity.X) {
					Projectile.velocity.X = -oldVelocity.X;
				}
				if (Projectile.velocity.Y != oldVelocity.Y) {
					Projectile.velocity.Y = -oldVelocity.Y;
				}
				Vector2 targetPos = Projectile.Center;
				bool foundTarget = false;
				Vector2 testPos;
				float targetDist = 120;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC target = Main.npc[i];
					if (target.CanBeChasedBy()) {
						testPos = Projectile.Center.Clamp(target.Hitbox);
						Vector2 difference = testPos - Projectile.Center;
						float distance = difference.Length();
						bool closest = Vector2.Distance(Projectile.Center, targetPos) > distance;
						bool inRange = distance < targetDist && Vector2.Dot(difference.SafeNormalize(Vector2.Zero), Projectile.velocity.SafeNormalize(Vector2.Zero)) > -0.1f;
						if ((!foundTarget || closest) && inRange) {
							targetPos = testPos;
							foundTarget = true;
							targetDist = distance;
						}
					}
				}
				if (foundTarget) {
					Projectile.velocity = (targetPos - Projectile.Center).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
					Projectile.localAI[1] = 10;
				}
				SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
				return false;
			}
			return true;
		}
		public override void PostDraw(Color lightColor) {
			Vector2 origin = new(
				11,
				11
			);
			Color color = Riven_Hive.GetGlowAlpha(lightColor);
			color.A = 255;
			Main.EntitySpriteDraw(
				TextureAssets.GlowMask[Riverang.glowmask].Value,
				Projectile.Center,
				null,
				color,
				Projectile.rotation,
				origin,
				Projectile.scale,
				Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
		}
	}
}
