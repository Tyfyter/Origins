using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Journal;
using Origins.Tiles.Brine;
using PegasusLib;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Torpedo_Tube : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Torpedo_Tube_Entry).Name;
		public class Torpedo_Tube_Entry : JournalEntry {
			public override string TextKey => "Torpedo_Tube";
			public override JournalSortIndex SortIndex => new("Brine_Pool_And_Lost_Diver", 2);
		}
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Torpedo_Tube_P>(54, 44, 8f, 44, 18);
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, -8f);
		}
	}
	public class Torpedo_Tube_Alt : Torpedo_Tube {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Torpedo_Tube>()] = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.shoot = ModContent.ProjectileType<Torpedo_Tube_Alt_P>();
		}
		public override void AddRecipes() {
			Recipe.Create(ModContent.ItemType<Torpedo_Tube>())
			.AddIngredient(Type)
			.AddIngredient<Mildew_Item>(4)
			.Register();
		}
	}
	public class Torpedo_Tube_P : ModProjectile, ICanisterProjectile {
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Canister_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Canister_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public virtual float Gravity => 0.1f;
		public virtual float HomingThreshold => 5.5f;
		public virtual (float @base, float speedFactor) HomingFactor => (0.02f, 0.006f);
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			Origins.MagicTripwireRange[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.extraUpdates = 1;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = 420;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
			Projectile.alpha = 255;
			Projectile.ignoreWater = true;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
			if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height)) {
				float targetWeight = HomingThreshold;
				Vector2 targetDiff = default;
				bool foundTarget = Main.player[Projectile.owner].DoHoming((target) => {
					if (!target.wet) return false;
					Vector2 currentDiff = target.Center - Projectile.Center;
					float dist = currentDiff.Length();
					currentDiff /= dist;
					float weight = Vector2.Dot(Projectile.velocity, currentDiff) * (500f / (dist + 100));
					if (weight > targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
						targetWeight = weight;
						targetDiff = currentDiff;
						return true;
					}
					return false;
				});

				if (foundTarget) {
					PolarVec2 velocity = (PolarVec2)Projectile.velocity;
					OriginExtensions.AngularSmoothing(
						ref velocity.Theta,
						targetDiff.ToRotation(),
						HomingFactor.@base + velocity.R * HomingFactor.speedFactor * Origins.HomingEffectivenessMultiplier[Projectile.type]
					);
					Projectile.velocity = (Vector2)velocity;
				}
			} else {
				this.DoGravity(Gravity);
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
			if (++Projectile.frameCounter >= 4) {
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.wet) target.AddBuff(Cavitation_Debuff.ID, 120);
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.alpha < 200) {
				return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, (255 - Projectile.alpha) / 2);
			}
			return Color.Transparent;
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			Vector2 origin = new(50, 8);
			Vector2 canisterOrigin = new(8, 0);
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				InnerTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.InnerColor,
				projectile.rotation + MathHelper.PiOver2,
				canisterOrigin,
				projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				OuterTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation + MathHelper.PiOver2,
				canisterOrigin,
				projectile.scale,
				spriteEffects
			);
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				projectile.Center - Main.screenPosition,
				texture.Frame(verticalFrames: 3, frameY: Projectile.frame),
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
		}
	}
	public class Torpedo_Tube_Alt_P : Torpedo_Tube_P {
		public override string Texture => typeof(Torpedo_Tube_P).GetDefaultTMLName();
		public override float Gravity => 0.06f;
		public override (float @base, float speedFactor) HomingFactor => (0.01f, 0.001f);
		public override float HomingThreshold => 6f;
		public override LocalizedText DisplayName => Mod.GetLocalization($"{LocalizationCategory}.Torpedo_Tube_P.DisplayName");
	}
}
