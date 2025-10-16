using Microsoft.Xna.Framework;
using Origins.Dev;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Spray_N_Pray : ModItem, ICustomWikiStat {
		public static byte lastPaint;
		public static byte lastCoating;
		public string[] Categories => [
			WikiCategories.Gun
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToRangedWeapon(ModContent.ProjectileType<Spray_N_Pray_P>(), ItemID.RedPaint, 12, 10);
			Item.damage = 20;
			Item.crit = -4;
			Item.useAnimation = 12;
			Item.useTime = 4;
			Item.width = 86;
			Item.height = 22;
			Item.rare = ItemRarityID.LightPurple;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item5;
			Item.reuseDelay = 4;
		}
		public override bool? CanChooseAmmo(Item ammo, Player player) {
			if (ammo.PaintOrCoating) {
				lastPaint = ammo.paint;
				lastCoating = ammo.paintCoating;
				return true;
			}
			return false;
		}
		public override bool CanConsumeAmmo(Item ammo, Player player) => Main.rand.NextBool(3, 5);
		public override Vector2? HoldoutOffset() => Vector2.Zero;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = 0; i < 2; i++) {
				Projectile.NewProjectile(
					source,
					position,
					velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.9f, 1.1f),
					type,
					damage + 1,
					knockback
				);
			}
			SoundEngine.PlaySound(Item.UseSound, position);
			return false;
		}
	}
	public class Spray_N_Pray_P : ModProjectile {
		public byte paint;
		public byte coating;
		public bool hit = false;
		Vector2 splatterVelocity;
		int splatterEntity = -1;
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.aiStyle = 1;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = 2;
		}
		public override void OnSpawn(IEntitySource source) {
			paint = Spray_N_Pray.lastPaint;
			coating = Spray_N_Pray.lastCoating;
		}
		public override void AI() {
			if (hit) return;
			Color dustColor = GetPaintColor(out bool isGlow);
			if (dustColor != Color.Transparent) {
				Dust dust = Dust.NewDustDirect(Projectile.position, 8, 8, isGlow ? 261 : DustID.Paint, Projectile.velocity.X * 4, Projectile.velocity.Y * 4, 50, dustColor);
				if (Main.rand.NextBool(2)) {
					dust.noGravity = true;
					dust.scale *= 1.2f;
				} else {
					dust.scale *= 0.5f;
				}
				dust.velocity /= 4;
				dust.noLight = isGlow;
			}
		}
		public override Color? GetAlpha(Color lightColor) => GetPaintColor(out bool isGlow).MultiplyRGBA(isGlow ? Color.White : lightColor);
		public Color GetPaintColor(out bool isGlow) {
			isGlow = false;
			switch (coating) {
				case PaintCoatingID.Glow:
				isGlow = true;
				return Color.White;

				case PaintCoatingID.Echo:
				return Main.ShouldShowInvisibleWalls() ? Color.White : Color.Transparent;

				default:
				return WorldGen.paintColor(paint);
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(paint);
			writer.Write(coating);
			writer.Write(hit);
			if (hit) {
				writer.Write(splatterVelocity.X);
				writer.Write(splatterVelocity.Y);
				writer.Write(splatterEntity);
			}
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			paint = reader.ReadByte();
			coating = reader.ReadByte();
			if (reader.ReadBoolean()) {
				Vector2 vel = reader.ReadVector2();
				int target = reader.ReadInt32();
				Entity entity = null;
				if (target >= 300) {
					entity = Main.npc[target - 300];
				} else if (target >= 0) {
					entity = Main.player[target];
				}
				Splatter(vel, entity);
				Projectile.Kill();
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			SetSplatter(Projectile.Center - Rectangle.Intersect(Projectile.Hitbox, target.Hitbox).Center(), target);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			SetSplatter(Projectile.Center - Rectangle.Intersect(Projectile.Hitbox, target.Hitbox).Center(), target);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Vector2 newVelocity = Projectile.velocity;
			Projectile.velocity = oldVelocity;
			SetSplatter(newVelocity);
			return false;
		}
		void SetSplatter(Vector2 newVelocity, Entity collisionEntity = null) {
			hit = true;
			Projectile.velocity = Vector2.Zero;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.aiStyle = 0;
			Projectile.netSpam = 0;
			Projectile.netUpdate = true;
			Projectile.timeLeft = 2;
			splatterVelocity = newVelocity;
			if (collisionEntity is NPC npc) {
				splatterEntity = npc.whoAmI - 300;
			} else if (collisionEntity is Player player) {
				splatterEntity = player.whoAmI;
			}
			Splatter(newVelocity, collisionEntity);
		}
		public void Splatter(Vector2 newVelocity, Entity collisionEntity = null) {
			Color paintColor = GetPaintColor(out bool isGlow);
			Vector2 dustSpawnPosition = Projectile.position + newVelocity + Projectile.velocity.SafeNormalize(default) * 8;
			PaintStickData dustCustomData = new(true, Coating: coating);
			if (collisionEntity is Player player) {
				dustCustomData = new(true, player, (dustSpawnPosition - player.Center) * new Vector2(player.direction, 1), 0, coating);
			} else if (collisionEntity is NPC npc) {
				dustCustomData = new(true, npc, (npc.Center - dustSpawnPosition) * new Vector2(npc.direction, npc.directionY), npc.rotation, coating);
			}
			for (int i = Main.rand.Next(3, 6); i-- > 0;) {
				Dust dust = Dust.NewDustPerfect(
					dustSpawnPosition,
					Main.rand.Next(Paint_Coating_Gore.IDs),
					Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0f, 1f) * 0.5f,
					newColor: paintColor
				);
				dust.customData = dustCustomData;
				dust.noLight = isGlow;
			}
			if (Projectile.velocity.X != newVelocity.X) {
				Projectile.velocity.X *= -0.5f;
			}
			if (Projectile.velocity.Y != newVelocity.Y) {
				Projectile.velocity.Y *= -0.5f;
			}
			//gore.velocity *= 1.5f;
			if (paintColor != Color.Transparent) {
				for (int i = Main.rand.Next(6, 12); i-- > 0;) {
					Dust.NewDustPerfect(Projectile.position, isGlow ? 261 : DustID.Paint, Projectile.velocity.RotatedByRandom(1.5f) * Main.rand.NextFloat(0f, 1f), newColor: paintColor, Scale: 0.5f).noLight = isGlow;
				}
			}
			SoundEngine.PlaySound(SoundID.NPCDeath1.WithVolumeScale(0.5f), Projectile.position);
		}
	}
	public record struct PaintStickData(bool StuckAtAll, Entity StickEntity = default, Vector2 StickOffset = default, float StickRotation = default, byte Coating = 0);
	public class Paint_Coating_Gore : ModDust {
		public static int[] IDs { get; private set; }
		public override string Texture => "Origins/Gores/" + GetType().Name;
		public override void OnSpawn(Dust dust) {
			dust.frame = new Rectangle(0, 0, 10, 10);
		}
		public override bool Update(Dust dust) {
			if (dust.customData is PaintStickData stickData && stickData.StuckAtAll) {
				if (stickData.StickEntity is not null) {
					if (stickData.StickEntity is NPC npc) {
						if (npc.direction != npc.oldDirection) {
							stickData.StickOffset *= new Vector2(-1, 1);
							dust.customData = stickData with { StickOffset = stickData.StickOffset };
						}
						if (npc.rotation != stickData.StickRotation) {
							stickData.StickOffset = stickData.StickOffset.RotatedBy((npc.rotation - stickData.StickRotation) * npc.direction);
							dust.customData = stickData with { StickOffset = stickData.StickOffset, StickRotation = npc.rotation };
						}
						dust.position = npc.Center - stickData.StickOffset * new Vector2(npc.direction, npc.directionY);
						dust.alpha += (int)(Math.Min(npc.velocity.LengthSquared() / 64, 1.5f) + 0.5f);
						if (!npc.active || !npc.Hitbox.Intersects(new Rectangle((int)dust.position.X, (int)dust.position.Y, 8, 8))) {
							dust.velocity = npc.velocity;
							dust.customData = stickData with { StuckAtAll = false };
						}
					} else {
						dust.position = stickData.StickEntity.Center - stickData.StickOffset * new Vector2(stickData.StickEntity.direction, 1);
						dust.alpha += (int)(Math.Min(stickData.StickEntity.velocity.LengthSquared() / 64, 2.5f) + 0.5f);
						if (!stickData.StickEntity.active || (stickData.StickEntity is Player player && player.dead) || !stickData.StickEntity.Hitbox.Intersects(new Rectangle((int)dust.position.X, (int)dust.position.Y, 8, 8))) {
							dust.velocity = stickData.StickEntity.velocity;
							dust.customData = stickData with { StuckAtAll = false };
						}
					}
				} else {
					dust.position += dust.velocity;
					dust.velocity *= 0.75f;
				}
			} else {
				dust.position += dust.velocity;
				dust.velocity.X *= 0.98f;
				dust.velocity.Y += 0.08f;
			}
			if (Main.rand.NextBool(2)) dust.alpha++;
			return false;
		}
		public override void SetStaticDefaults() {
			IDs = [
				ModContent.DustType<Paint_Coating_Gore>(),
				ModContent.DustType<Paint_Coating_Gore2>(),
				ModContent.DustType<Paint_Coating_Gore3>()
			];
		}
		public override bool PreDraw(Dust dust) {
			return true;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) {
			if (dust.customData is PaintStickData stickData) {
				switch (stickData.Coating) {
					case PaintCoatingID.Glow:
					return Color.White * 0.9f;

					case PaintCoatingID.Echo:
					if (!Main.ShouldShowInvisibleWalls()) return Color.Transparent;
					break;
				}
			}
			return lightColor * 0.9f;
		}
	}
	public class Paint_Coating_Gore2 : Paint_Coating_Gore {
		public override void OnSpawn(Dust dust) {
			dust.frame = new Rectangle(0, 0, 10, 12);
		}
	}
	public class Paint_Coating_Gore3 : Paint_Coating_Gore {
		public override void OnSpawn(Dust dust) {
			dust.frame = new Rectangle(0, 0, 12, 12);
		}
	}
}
