using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.CrossMod;
using Origins.Items.Materials;
using PegasusLib;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Maelstrom_Incantation : ModItem, ICustomDrawItem {
		private Asset<Texture2D> _smolTexture;
		public Texture2D SmolTexture => (_smolTexture ??= this.GetSmallTexture())?.Value;
		public override void SetStaticDefaults() {
			Origins.DamageBonusScale[Type] = 1.5f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
		}
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.width = 26;
			Item.height = 28;
			Item.value = Item.sellPrice(0, 8);
			Item.autoReuse = true;
			Item.damage = 19;
			Item.DamageType = DamageClasses.Incantation;
			Item.noMelee = true;
			Item.knockBack = 4;
			Item.shoot = ModContent.ProjectileType<Maelstrom_Incantation_P>();
			Item.noUseGraphic = true;
			Item.shootSpeed = 16f;
			Item.UseSound = null;
			Item.mana = 16;
			Item.useTime = 26;
			Item.useAnimation = 26;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 60);
			Item.holdStyle = ItemHoldStyleID.HoldLamp;
			Item.UseSound = SoundID.Item8;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Book, 5)
			.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 10)
			.AddTile(TileID.Bookcases)
			.Register();
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Item117.WithPitchRange(0.0f, 0.2f), player.itemLocation);
			return null;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, player.itemTime);
			return false;
		}
		public override void UseItemFrame(Player player) => Incantations.HoldItemFrame(player);
		public override void HoldItemFrame(Player player) => Incantations.HoldItemFrame(player);
		public bool BackHand => true;
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Incantations.DrawInHand(
				SmolTexture,
				ref drawInfo,
				lightColor
			);
		}
	}
	public class Maelstrom_Incantation_P : ModProjectile {
		const int lifespan = 1800;
		static AutoLoadingAsset<Texture2D> largeTexture = "Origins/Items/Weapons/Summoner/Maelstrom_Incantation_Large_P";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.AmberBolt);
			Projectile.DamageType = DamageClasses.Incantation;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 16;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 1;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.timeLeft = lifespan;
			Projectile.penetrate = -1;
			Projectile.scale = 0.85f;
			Projectile.alpha = 255;
		}
		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (Projectile.ai[0] > 0) {
				int age = lifespan - Projectile.timeLeft;
				if (age <= Projectile.ai[0]) {
					Projectile.position = owner.MountedCenter;
					float ageFactor = age / Projectile.ai[0];

					Projectile.alpha = (int)(255 * (1 - ageFactor));
					Projectile.scale = 0.4f + 0.45f * ageFactor;

					owner.heldProj = Projectile.whoAmI;
					if (Projectile.owner == Main.myPlayer) {
						Vector2 direction = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitY);
						Vector2 oldVel = Projectile.velocity;
						Projectile.velocity = direction * Projectile.velocity.Length();
						if (oldVel != Projectile.velocity) Projectile.netUpdate = true;
					}
					owner.direction = Math.Sign(Projectile.velocity.X);
					owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - MathHelper.PiOver2);
					if (age == (int)Projectile.ai[0] && Projectile.owner == Main.myPlayer) {
						Projectile.NewProjectile(
							Projectile.GetSource_FromThis(),
							Projectile.position,
							Projectile.velocity,
							ModContent.ProjectileType<Maelstrom_Incantation_P_Trail>(),
							(Projectile.damage * 3) / 4,
							Projectile.knockBack,
							Projectile.owner,
							ai1: Projectile.whoAmI
						);
					}
				} else {
					Projectile.alpha = 0;
					Projectile.scale = 0.85f;
					Projectile.friendly = true;
				}
				Projectile.frameCounter++;
				if (Projectile.frameCounter >= 6) {
					Projectile.frameCounter = 0;
					Projectile.frame++;
					if (Projectile.frame >= 4) {
						Projectile.frame = 0;
					}
				}
			} else {
				if (--Projectile.ai[0] <= -16) {
					if (Projectile.ai[0] > -24) {
						if (Projectile.ai[0] == -16) {
							SoundEngine.PlaySound(SoundID.Item122.WithPitchRange(0.9f, 1.1f).WithVolume(2), Projectile.position);
							SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithPitchRange(0.0f, 0.2f).WithVolume(0.75f), Projectile.position);
							Projectile.damage += Projectile.damage / 2;
							Projectile.knockBack += Projectile.knockBack;
						}
						//Projectile.scale += 0.5f;
					} else {
						if (Projectile.ai[0] < -32) {
							Projectile.Kill();
						}
					}
					if ((int)Projectile.ai[0] % 2 == 0) {
						float angle = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
						Vector2 targetEnd = OriginExtensions.Vec2FromPolar(angle, Main.rand.NextFloat(18, 24)) + Projectile.position;
						Vector2 targetStart = OriginExtensions.Vec2FromPolar(angle, Main.rand.NextFloat(2)) + Projectile.position;
						if (Projectile.owner == Main.myPlayer) {
							Projectile.NewProjectile(
								Projectile.GetSource_FromThis(),
								targetEnd,
								default,
								Projectiles.Misc.Felnum_Shock_Arc.ID,
								0,
								0,
								Owner: Main.myPlayer,
								ai0: targetStart.X,
								ai1: targetStart.Y
							);
						}
					}
					Projectile.frameCounter++;
					if (Projectile.frameCounter >= 5) {
						Projectile.frameCounter = 0;
						Projectile.frame++;
						if (Projectile.frame >= 4) {
							Projectile.frame = 0;
						}
					}
				} else {
					Projectile.scale = 2.5f;
				}
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.ai[0] > 0) {
				if (lifespan - Projectile.timeLeft > Projectile.ai[0]) return null;
			} else {
				if (Projectile.ai[0] < -16) return null;
			}
			return false;
		}
		private void startExplosion() {
			if (Projectile.ai[0] > 0) {
				Projectile.ai[2] = Projectile.velocity.X;
				Projectile.velocity = Vector2.Zero;
				Projectile.ai[0] = 0;
				Projectile.netUpdate = true;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (Projectile.velocity == Vector2.Zero) modifiers.HitDirectionOverride = Math.Sign(Projectile.ai[2]);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			startExplosion();
			target.AddBuff(Maelstrom_Buff_Damage.ID, 240);
			target.AddBuff(Maelstrom_Buff_Zap.ID, 240);
		}
		public override Color? GetAlpha(Color lightColor) {
			int val = 255 - Projectile.alpha;
			return new Color(val, val, val, val);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			startExplosion();
			return false;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 6;
			height = 6;
			return true;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			int size = (int)(20 * 0.5f * Projectile.scale * 1.1764705882352941176470588235294f);
			hitbox.Inflate(size, size);
		}
		public override bool PreDraw(ref Color lightColor) {
			const int large_frames = 3;
			bool isLarge = Projectile.ai[0] < 0;
			Texture2D texture = isLarge ? largeTexture : TextureAssets.Projectile[Type].Value;
			int frame = isLarge ? (int)(Projectile.ai[0] * large_frames / -32f) : Projectile.frame;
			int frameCount = isLarge ? large_frames : Main.projFrames[Type];
			float scale = isLarge ? 1 : Projectile.scale;

			int width = texture.Width;
			int frameHeight = texture.Height / frameCount;
			int frameY = frameHeight * frame;
			Main.EntitySpriteDraw(
				texture,
				Projectile.position - Main.screenPosition,
				new Rectangle(0, frameY, width, frameHeight),
				Projectile.GetAlpha(lightColor),
				Projectile.rotation,
				new Vector2(width * 0.5f, frameHeight * 0.5f),
				scale,
				0,
				0
			);
			return false;
		}
	}
	public class Maelstrom_Incantation_P_Trail : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PiercingStarlight;
		Triangle Hitbox {
			get {
				Vector2 direction = Vector2.Normalize(Projectile.velocity) * -64;
				Vector2 side = new Vector2(direction.Y, -direction.X) * (float)Math.Pow(Projectile.ai[0], 0.35f) * 0.65f;

				Vector2 basePos = Projectile.position + direction * Projectile.ai[0];
				return new Triangle(
					Projectile.position,
					basePos + side,
					basePos - side
				);
			}
		}
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CultistBossLightningOrbArc);
			Projectile.DamageType = DamageClass.Summon;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 1;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.timeLeft *= 3;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			if (Projectile.ai[1] < 0 || Projectile.ai[1] > Main.maxProjectiles) {
				Projectile.Kill();
				return;
			}
			Projectile owner = Main.projectile[(int)Projectile.ai[1]];
			if (!owner.active) {
				Projectile.Kill();
				return;
			} else {
				Projectile.timeLeft = 2;
			}
			if (owner.velocity != default) Projectile.velocity = owner.velocity;
			Projectile.position = owner.position - Projectile.velocity;
			if (owner.ai[0] > 0) {
				if (Projectile.ai[0] < 1f) {
					Projectile.ai[0] += 0.05f;
				}
			} else {
				Projectile.friendly = false;
				if (Projectile.ai[0] > 0.1f) {
					Projectile.ai[0] -= 0.1f;
					if (Projectile.ai[0] < 0.005f) {
						Projectile.Kill();
					}
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			target.AddBuff(Maelstrom_Buff_Damage.ID, 240);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Triangle hitbox = Hitbox;
			if (!hitbox.HasNaNs() && hitbox.Intersects(targetHitbox)) {
				return true;
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Triangle hitbox = Hitbox;
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 scale = (new Vector2(1.85f, 0.25f) / new Vector2(36, 72)) * (hitbox.b - hitbox.a).Length();
			Rectangle frame = new Rectangle(0, 0, 36, 72);
			Color color = Color.Lerp(new Color(255, 255, 255, 0), lightColor, 0.75f);
			Main.EntitySpriteDraw(
				texture,
				hitbox.a - Main.screenPosition,
				frame,
				color,
				(hitbox.b - hitbox.a).ToRotation() + MathHelper.Pi,
				new Vector2(36, 36),
				scale,
				SpriteEffects.None,
			0);
			Main.EntitySpriteDraw(
				texture,
				hitbox.a - Main.screenPosition,
				frame,
				color,
				(hitbox.c - hitbox.a).ToRotation() + MathHelper.Pi,
				new Vector2(36, 36),
				scale,
				SpriteEffects.None,
			0);
			return false;
		}
	}
}
