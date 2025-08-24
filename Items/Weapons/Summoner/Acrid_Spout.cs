using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Materials;
using Origins.NPCs;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Acrid_Spout : ModItem {
		public override void SetStaticDefaults() {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [Toxic_Shock_Debuff.ID];
		}
		public override void SetDefaults() {
			Item.DefaultToWhip(ModContent.ProjectileType<Acrid_Spout_P>(), 34, 5, 4, 28);
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.value = Item.sellPrice(gold: 4, silver: 60);
			Item.rare = ItemRarityID.LightRed;
		}
		public override bool MeleePrefix() => true;
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI).scale *= player.GetAdjustedItemScale(Item);
			return false;
		}
	}
	public class Acrid_Spout_P : ModProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.WhipSettings.Segments = 20;
			Projectile.WhipSettings.RangeMultiplier = 2;
		}

		private float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Projectile.WhipSettings.RangeMultiplier = 2f * Projectile.scale;
			Player owner = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

			Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;

			Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

			float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;

			if (Timer >= swingTime || owner.ItemAnimationEndingOrEnded) {
				Projectile.Kill();
				return;
			}

			owner.heldProj = Projectile.whoAmI;

			// These two lines ensure that the timing of the owner's use animation is correct.
			owner.itemAnimation = owner.itemAnimationMax - (int)(Timer / Projectile.MaxUpdates);
			owner.itemTime = owner.itemAnimation;

			List<Vector2> points = Projectile.WhipPointsForCollision;
			points.Clear();
			Projectile.FillWhipControlPoints(Projectile, points);
			int time = (int)Timer;
			int delay = (5 * Projectile.MaxUpdates);
			if (time % delay == 0 && Timer > swingTime * 0.35f && Timer < swingTime * 0.95f) {
				int skip = (points.Count * 6) / delay;
				for (int i = (time / delay) % delay + Main.rand.Next(skip); i < points.Count; i += skip) {
					if (i == 0) continue;
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						points[i],
						(points[i] - points[i - 1]).SafeNormalize(default) * 4 + Projectile.velocity * 0.5f,
						ModContent.ProjectileType<Acrid_Spout_Droplet>(),
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner
					);
				}
			}
			if (Timer == swingTime / 2) {
				// Plays a whipcrack sound at the tip of the whip.
				SoundEngine.PlaySound(SoundID.Item153, points[^1]);
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Acrid_Spout_Buff.ID, 240);
		}

		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = [];
			Projectile.FillWhipControlPoints(Projectile, list);

			MiscShaderData miscShaderData = GameShaders.Misc["Origins:Identity"];
			miscShaderData.UseImage0(TextureAssets.MagicPixel);
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(0, 0, 0, 1));
			miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(0, 0, 1, 1));
			float[] oldRot;
			oldRot = new float[list.Count];
			for (int i = 0; i < list.Count; i++) {
				oldRot[i] = i == 0 ? 0 : (list[i] - list[i - 1]).ToRotation();
			}
			oldRot[0] = oldRot[1];
			miscShaderData.Apply();
			_vertexStrip.PrepareStrip(list.ToArray(), oldRot, (GetLightColor) => new(80, 225, 120), _ => 1, -Main.screenPosition, list.Count, includeBacksides: true);
			_vertexStrip.DrawTrail();

			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.instance.LoadProjectile(Type);
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 pos = list[0];

			for (int i = 0; i < list.Count; i++) {
				// These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
				// You can change them if they don't!
				Rectangle frame = new Rectangle(0, 0, 48, 28);
				Vector2 origin = new Vector2(24, 14);
				Vector2 scale = new Vector2(0.85f) * Projectile.scale;

				if (i == list.Count - 1) {
					frame.Y = 112;
				} else if (i > 10) {
					frame.Y = 84;
				} else if (i > 5) {
					frame.Y = 56;
				} else if (i > 0) {
					frame.Y = 28;
				}

				Vector2 element = list[i];
				Vector2 diff;
				if (i == list.Count - 1) {
					diff = element - list[i - 1];
				} else {
					diff = list[i + 1] - element;
				}

				float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
				Color color = Lighting.GetColor(element.ToTileCoordinates());

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

				pos += diff;
			}
			return false;
		}
	}
	public class Acrid_Spout_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().acridSpoutDebuff = true;
		}
	}
	public class Acrid_Spout_Droplet : ModProjectile, IElementalProjectile {
		public ushort Element => Elements.Acid;
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.DamageType = DamageClass.SummonMeleeSpeed;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
			Projectile.width = Projectile.height = 10;
			Projectile.light = 0;
			Projectile.timeLeft = 180;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 5;
			Projectile.ArmorPenetration += 26;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, 0, 0.75f * Projectile.scale, 0.3f * Projectile.scale);
			if (Projectile.timeLeft % 3 == 0) {
				Dust dust = Dust.NewDustPerfect(Projectile.Center, 226, Projectile.velocity * -0.25f, 100, new Color(0, 255, 0), Projectile.scale * 0.85f);
				dust.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.AcidDye);
				dust.noGravity = false;
				dust.noLight = true;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.ai[1] > 0) {
				Projectile.velocity -= oldVelocity - Projectile.velocity;
				return false;
			}
			return true;
		}
		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 3; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, 10, 10, DustID.Electric, 0, 0, 100, new Color(0, 255, 0), 1.25f * Projectile.scale * 0.85f);
				dust.shader = GameShaders.Armor.GetShaderFromItemId(ItemID.AcidDye);
				dust.noGravity = true;
				dust.noLight = true;
			}
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, 30);
		}
		public override bool PreDraw(ref Color lightColor) {
			return false;
		}
	}
}
