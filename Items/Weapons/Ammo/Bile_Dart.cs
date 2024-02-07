using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Graphics;
using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
    public class Bile_Dart : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CursedDart);
			Item.maxStack = 999;
			Item.damage = 9;
			Item.shoot = ModContent.ProjectileType<Bile_Dart_P>();
			Item.shootSpeed = 3f;
			Item.knockBack = 2.2f;
			Item.value = Item.sellPrice(copper: 6);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 100);
			recipe.AddIngredient(ModContent.ItemType<Black_Bile>());
			recipe.Register();
		}
	}
	public class Bile_Dart_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ammo/Bile_Dart";
		
		public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.CursedDart);
        }
		public override void AI() {
			Projectile.localAI[0] += 1f;
			if (Projectile.localAI[0] > 3f)
				Projectile.alpha = 0;

			if (Projectile.ai[0] >= 20f) {
				Projectile.ai[0] = 20f;
				Projectile.velocity.Y += 0.075f;
			}
			int auraProjIndex = (int)Projectile.ai[1] - 1;
			if (auraProjIndex < 0) {
				if (Projectile.owner == Main.myPlayer) Projectile.ai[1] = Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.position,
					default,
					Bile_Dart_Aura.ID,
					Projectile.damage / 10,
					0,
					Projectile.owner,
					Projectile.whoAmI
				) + 1;
			} else {
				Projectile auraProj = Main.projectile[auraProjIndex];
				if (auraProj.active && auraProj.type == Bile_Dart_Aura.ID) {
					auraProj.Center = Projectile.Center;
					auraProj.rotation = Projectile.rotation;
				} else {
					Projectile.ai[1] = 0;
				}
			}
		}
		public override Color? GetAlpha(Color lightColor) {
			return Projectile.alpha == 0 ? new Color(255, 255, 255, 200) : Color.Transparent;
		}
		public override void OnKill(int timeLeft) {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            SoundEngine.PlaySound(SoundID.NPCHit22.WithVolume(0.5f), Projectile.position);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), 60);
        }
	}
	public class Bile_Dart_Aura : ModProjectile {
		internal static bool anyActive;
		public static ScreenTarget AuraTarget { get; private set; }
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.hide = false;
			Projectile.width = Projectile.height = 72;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.tileCollide = false;
			Projectile.scale = 1.5f;
		}
		public override void Load() {
			if (Main.dedServ) return;
			AuraTarget = new(
				MaskAura,
				() => {
					if (anyActive) {
						anyActive = false;
						return Lighting.NotRetro;
					} else {
						return false;
					}
				},
				0
			);
			Terraria.On_Main.DrawInfernoRings += Main_DrawInfernoRings;
		}

		private void Main_DrawInfernoRings(Terraria.On_Main.orig_DrawInfernoRings orig, Main self) {
			orig(self);
			if (Main.dedServ) return;
			if (Lighting.NotRetro) DrawAura(Main.spriteBatch);
		}

		public override void Unload() {
			AuraTarget = null;
		}
		public override void AI() {
			int auraProj = (int)Projectile.ai[0];
			if (auraProj < 0) {
				Projectile.Kill();
			} else {
				Projectile ownerProj = Main.projectile[auraProj];
				if (ownerProj.active) {
					Projectile.Center = ownerProj.Center;
					Projectile.rotation = ownerProj.rotation;
				} else {
					Projectile.Kill();
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), 20);
		}
		public override bool PreDraw(ref Color lightColor) {
			anyActive = true;
			return false;
		}
		static void MaskAura(SpriteBatch spriteBatch) {
			if (Main.dedServ) return;
			Vector2 screenCenter = Main.ScreenSize.ToVector2() * 0.5f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (proj.active && proj.type == ID) {
					spriteBatch.Draw(
						TextureAssets.Projectile[ID].Value,
						(proj.Center - Main.screenPosition - screenCenter) * Main.GameViewMatrix.Zoom + screenCenter,
						null,
						new Color(
							MathHelper.Clamp(proj.velocity.X / 16 + 8, 0, 1),
							MathHelper.Clamp(proj.velocity.Y / 16 + 8, 0, 1),
						0f),
						0,
						new Vector2(36),
						proj.scale * Main.GameViewMatrix.Zoom.X,
						0,
					0);
				}
			}
		}

		static void DrawAura(SpriteBatch spriteBatch) {
			if (Main.dedServ) return;
			Main.LocalPlayer.ManageSpecialBiomeVisuals("Origins:MaskedRasterizeFilter", anyActive, Main.LocalPlayer.Center);
			if (anyActive) {
				Filters.Scene["Origins:MaskedRasterizeFilter"].GetShader().UseImage(AuraTarget.RenderTarget, 1);
			}
			//spriteBatch.Draw(AuraTarget.RenderTarget, Vector2.Zero, Color.White);
		}
	}

}
