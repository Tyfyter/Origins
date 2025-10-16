using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Abrasion_Blaster : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Launcher
		];
		public override void SetDefaults() {
			Item.DefaultToLauncher(18, 45, 48, 22, false);
			Item.crit = 4;
			Item.useTime = 1;
			Item.shootSpeed = 15;
			Item.shoot = ModContent.ProjectileType<Abrasion_Blaster_P>();// just in case anything expects weapons to directly shoot what they shoot
			Item.useAmmo = ModContent.ItemType<Scrap>();
			Item.reuseDelay = 15;
			Item.channel = true;
			Item.UseSound = null;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Biocomponent10>(), 35)
			.AddIngredient(ModContent.ItemType<Scrap>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		protected internal static bool consumeFromProjectile = false;
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return consumeFromProjectile || player.ItemUsesThisAnimation == 1;
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Item132.WithVolume(0.5f).WithPitch(0.5f /** projectile.ai[0]*/), player.itemLocation);
			return null;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Abrasion_Blaster_Charge_P.ID;
			//SoundEngine.PlaySound(SoundID.Item36.WithVolume(0.75f), position);
			//Item.UseSound = Origins.Sounds.EnergyRipple;
			int heldProjectile = player.GetModPlayer<OriginPlayer>().heldProjectile;
			if (heldProjectile > -1) {
				Projectile projectile = Main.projectile[heldProjectile];
				if (projectile.active && projectile.type == Abrasion_Blaster_Charge_P.ID && projectile.ai[0] > 4) {
					velocity = velocity.RotatedByRandom(0.1f);
				}
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int heldProjectile = player.GetModPlayer<OriginPlayer>().heldProjectile;
			if (heldProjectile > -1) {
				if (player.controlUseItem) {
					player.itemAnimation += player.itemTimeMax;
				} else {
					player.itemAnimation = player.itemTimeMax;
				}
				Projectile projectile = Main.projectile[heldProjectile];
				if (projectile.active && projectile.type == Abrasion_Blaster_Charge_P.ID) {
					if (projectile.velocity != velocity) {
						projectile.netUpdate = true;
					}
					projectile.velocity = velocity;
					return false;
				}
			}
			if (player.ItemUsesThisAnimation == 1) {
				// initial sound here
				return true;
			}
			return false;
		}
	}
	public class Abrasion_Blaster_Charge_P : Abrasion_Blaster_P {
		public override string Texture => typeof(Abrasion_Blaster).GetDefaultTMLName();
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.friendly = false;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0, 0, 255, new Color(255, 150, 30));
			dust.position = Projectile.Center;
			dust.noGravity = true;
			dust.velocity *= 3f;

			Player player = Main.player[Projectile.owner];
			player.GetModPlayer<OriginPlayer>().heldProjectile = Projectile.whoAmI;
			if (!player.channel) {
				if (Main.myPlayer == Projectile.owner) {
					if (Projectile.ai[2] != 1) {
						Projectile.NewProjectile(
							player.GetSource_ItemUse_WithPotentialAmmo(player.HeldItem, player.HeldItem.useAmmo),
							Projectile.Center,
							Projectile.velocity,
							Abrasion_Blaster_P.ID,
							Projectile.damage,
							Projectile.knockBack,
							Projectile.owner,
							Projectile.ai[0]
						);
					} else {
						Projectile.NewProjectile(
							player.GetSource_ItemUse_WithPotentialAmmo(player.HeldItem, player.HeldItem.useAmmo),
							Projectile.Center,
							Projectile.velocity,
							ModContent.ProjectileType<Abrasion_Blaster_Explosion>(),
							Projectile.damage,
							Projectile.knockBack,
							Projectile.owner,
							ai1: Projectile.ai[0]
						);
					}
				}
				Projectile.Kill();
			} else {
				Projectile.Center = player.RotatedRelativePoint(player.MountedCenter);
				Vector2 offset = Projectile.velocity.SafeNormalize(default) * 36;
				bool breakChannel = false;
				if (Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, Projectile.position + offset, Projectile.width, Projectile.height)) {
					Projectile.ai[2] = 0;
				} else {
					Projectile.ai[2] = 1;
				}
				Projectile.position += offset;
				if (++Projectile.ai[1] >= player.itemAnimationMax) {
					try {
						Abrasion_Blaster.consumeFromProjectile = true;
						if (player.PickAmmo(player.HeldItem, out _, out _, out _, out _, out _)) {
							Projectile.ai[1] = 0;
							Projectile.ai[0]++;
							Projectile.netUpdate = true;
							breakChannel = Projectile.ai[0] > 7;
						} else {
							breakChannel = true;
						}
					} finally {
						Abrasion_Blaster.consumeFromProjectile = false;
					}
				}
				if (breakChannel) {
					player.itemAnimation = player.itemTimeMax;
					player.reuseDelay *= 3;
					player.channel = false;
				}
			}
		}
		public override void OnKill(int timeLeft) { }
	}
	public class Abrasion_Blaster_P : ModProjectile {
		public static AutoLoadingAsset<Texture2D> texture0 = typeof(Abrasion_Blaster).GetDefaultTMLName() + "_Charge1";
		public static AutoLoadingAsset<Texture2D> texture1 = typeof(Abrasion_Blaster).GetDefaultTMLName() + "_Charge2";
		public static AutoLoadingAsset<Texture2D> texture2 = typeof(Abrasion_Blaster).GetDefaultTMLName() + "_Charge3";
		public override string Texture => typeof(Abrasion_Blaster).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.hide = true;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			if (Projectile.ai[0] > 5) {
				Projectile.extraUpdates = 3;
			} else if (Projectile.ai[0] > 2) {
				Projectile.extraUpdates = 2;
			}
			Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0, 0, 255, new Color(255, 150, 30));
			dust.position = Projectile.Center;
			dust.noGravity = true;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 1 + Projectile.ai[0] * 0.65f;
		}
		public override void OnKill(int timeLeft) {
			if (Main.myPlayer == Projectile.owner) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<Abrasion_Blaster_Explosion>(),
					Projectile.damage,
					Projectile.knockBack,
					ai1: Projectile.ai[0]
				);
			}
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture;
			Rectangle frame = default;
			int frameMax;
			if (Projectile.ai[0] > 5) {
				texture = texture2;
				frame.Width = 18;
				frame.Height = 18;
				frameMax = 2;
			} else if (Projectile.ai[0] > 2) {
				texture = texture1;
				frame.Width = 16;
				frame.Height = 16;
				frameMax = 3;
			} else {
				texture = texture0;
				frame.Width = 10;
				frame.Height = 10;
				frameMax = 4;
			}
			if (++Projectile.frameCounter > frameMax) {
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) & 0b11;
			}
			switch (Projectile.frame) {
				case 1:
				frame.Y += frame.Height + 2;
				break;

				case 3:
				frame.Y += (frame.Height + 2) * 2;
				break;
			}
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				frame,
				new Color(1f, 1f, 1f, 0.8f),
				Projectile.rotation,
				frame.Size() * 0.5f,
				Projectile.scale,
				0
			);
			return false;
		}
	}
	public class Abrasion_Blaster_Explosion : ExplosionProjectile {
		public override DamageClass DamageType => DamageClasses.ExplosiveVersion[DamageClass.Ranged];
		public override int Size => 72;
		public override bool DealsSelfDamage => Projectile.ai[1] > 2;
		public override void AI() {
			if (Projectile.ai[0] == 0 && Projectile.ai[1] > 5) {
				const int rad = 4;
				Vector2 center = Projectile.Center;
				int i = (int)(center.X / 16);
				int j = (int)(center.Y / 16);
				Projectile.ExplodeTiles(
					center,
					rad,
					i - rad,
					i + rad,
					j - rad,
					j + rad,
					Projectile.ShouldWallExplode(center, rad, i - rad, i + rad, j - rad, j + rad)
				);
			}
			base.AI();
		}
	}
}
