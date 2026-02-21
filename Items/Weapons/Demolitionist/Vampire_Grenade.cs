using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Dusts;
using Origins.Projectiles;
using PegasusLib.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.NPCs;

namespace Origins.Items.Weapons.Demolitionist {
	public class Vampire_Grenade : ModItem {
		public static int ShaderID { get; private set; }
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			GameShaders.Armor.BindShader(Type, new ArmorShaderData(
				Mod.Assets.Request<Effect>("Effects/MakeRing"),
				"MakeRing"
			)).UseOpacity(2);
			ShaderID = GameShaders.Armor.GetShaderIdFromItemId(Type);
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 48;
			Item.shoot = ModContent.ProjectileType<Vampire_Grenade_P>();
			Item.shootSpeed *= 1.25f;
			Item.value *= 9;
			Item.rare = ItemRarityID.Orange;
		}
	}
	public class Vampire_Grenade_P : ModProjectile {
		public override string Texture => typeof(Vampire_Grenade).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.penetrate = 1;
			Projectile.timeLeft = 135;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.NewProjectile(
				Projectile.GetSource_Death(),
				target.Center,
				default,
				ModContent.ProjectileType<Vampire_Grenade_Heal>(),
				Projectile.damage,
				Projectile.knockBack,
				ai0: damageDone * 0.075f,
				ai2: Projectile.identity
			);
		}
		public override void OnKill(int timeLeft) {
			if (!Projectile.IsLocallyOwned()) return;
			int identity = Projectile.NewProjectile(
				Projectile.GetSource_Death(),
				Projectile.Center,
				default,
				ModContent.ProjectileType<Vampire_Grenade_Suck>(),
				Projectile.damage,
				Projectile.knockBack
			);
			int healType = ModContent.ProjectileType<Vampire_Grenade_Heal>();
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (!other.IsLocallyOwned() || other.type != healType || other.ai[2] != Projectile.identity) continue;
				other.ai[2] = identity;
				other.netUpdate = true;
			}
		}
	}
	public class Vampire_Grenade_Suck : ModProjectile, IIsExplodingProjectile, IShadedProjectile {
		public override string Texture => typeof(Vampire_Grenade).GetDefaultTMLName();
		public bool IsExploding => true;
		public int Shader => Vampire_Grenade.ShaderID;
		public static int BaseRadius => 64;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 6000;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent { Entity: Projectile parent }) {
				parent.localNPCImmunity.CopyTo(Projectile.localNPCImmunity, 0);
			}
		}
		public override void AI() {
			Projectile.rotation += 0.01f;
			Projectile.localAI[0] = Projectile.GetGlobalProjectile<ExplosiveGlobalProjectile>().ApplyBlastRadius(BaseRadius, Projectile.owner);
			if (Projectile.ai[0] == 0) {
				Projectile.ai[1] = 1 - (1 - Projectile.ai[1]) * 0.82f;
				if (MathUtils.LinearSmoothing(ref Projectile.ai[1], 1, 0.001f)) {
					Projectile.ai[0] = 1;
					Projectile.localNPCHitCooldown = 15;
					Rectangle hitbox = Projectile.Hitbox;
					for (int i = 0; i < Projectile.localNPCImmunity.Length; i++) {
						if (Projectile.localNPCImmunity[i] != 0) {
							NPC npc = Main.npc[i];
							if (!npc.active || !Projectile.Colliding(hitbox, npc.Hitbox)) Projectile.localNPCImmunity[i] = 0;
						}
					}
				}
			} else if(Collapse(ref Projectile.ai[1])) {
				 Projectile.Kill();
			}
			Projectile.scale = Projectile.ai[1] * 1.4142135624f;
		}
		public static bool Collapse(ref float size) => MathUtils.LinearSmoothing(ref size, 0, (1 - size) * 0.1f + 0.001f);
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.NewProjectile(
				Projectile.GetSource_Death(),
				Vampire_Grenade_Suck.MoveInRange(Projectile, target.Center),
				default,
				ModContent.ProjectileType<Vampire_Grenade_Heal>(),
				Projectile.damage,
				Projectile.knockBack,
				ai0: damageDone * 0.075f,
				ai2: Projectile.identity
			);
		}
		public static float GetSize(Projectile projectile) => projectile.localAI[0] * projectile.ai[1];
		public static Vector2 MoveInRange(Projectile projectile, Vector2 position) {
			Vector2 diff = (position - projectile.Center).Normalized(out float dist);
			return projectile.Center + diff * Math.Min(GetSize(projectile), dist);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Projectile.direction = Math.Sign(targetHitbox.Center().X - Projectile.Center.X) * (Projectile.ai[0] == 0).ToDirectionInt();
			float size = GetSize(Projectile);
			float margin = Projectile.localAI[0] * 0.1f;
			if (!targetHitbox.IsWithin(Projectile.Center, size + margin)) return false;
			if (Projectile.ai[0] != 0) {
				return !targetHitbox.TopLeft().IsWithin(Projectile.Center, size)
					|| !targetHitbox.TopRight().IsWithin(Projectile.Center, size)
					|| !targetHitbox.BottomLeft().IsWithin(Projectile.Center, size)
					|| !targetHitbox.BottomRight().IsWithin(Projectile.Center, size);
			}
			return true;
		}
		public override bool PreDraw(ref Color lightColor) {
			ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(Vampire_Grenade.ShaderID, Main.LocalPlayer);
			shader.UseOpacity(Projectile.localAI[0]);
			shader.UseSaturation(GetSize(Projectile));
			shader.UseImage(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
			shader.Shader.Parameters["uScale"]?.SetValue(Projectile.scale);
			shader.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 0, 0, 0));
			shader.Shader.Parameters["uAlphaMatrix1"].SetValue(new Vector4(0.85f, 0, 0, 0.15f));
			shader.Shader.Parameters["uRotation1"].SetValue(Projectile.rotation);
			Texture2D texture = TextureAssets.Extra[ExtrasID.RainbowRodTrailShape].Value;
			DrawData data = new(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				Color.Red,
				Projectile.rotation,
				texture.Size() * 0.5f,
				Projectile.scale,
				0
			);
			shader.Apply(Main.LocalPlayer, data);
			data.Draw(Main.spriteBatch);
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			return false;
		}
	}
	public class Vampire_Grenade_Heal : ModProjectile {
		public override string Texture => "Terraria/Images/NPC_0";
		public override void SetDefaults() {
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			Projectile.extraUpdates = 2;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active) {
				Projectile.Kill();
				return;
			}
			if (Projectile.GetRelatedProjectile(2) is Projectile suck) {
				if (suck.ai[0] != 0) {
					Projectile.velocity = Vampire_Grenade_Suck.MoveInRange(suck, Projectile.Center) - Projectile.Center;
					float size = suck.ai[1];
					if (Vampire_Grenade_Suck.Collapse(ref size)) {
						Projectile.ai[2] = -1;
					}
				}
			} else {
				Projectile.ai[2] = -1;
				Vector2 direction = player.Center - Projectile.Center;
				float dist = direction.LengthSquared();
				if (dist < 50f && Projectile.Hitbox.Intersects(player.Hitbox)) {
					if (Projectile.owner == Main.myPlayer && !player.moonLeech) {
						player.Heal(Main.rand.RandomRound(Projectile.ai[0]));
					}
					Projectile.Kill();
				} else {
					direction *= 4 / MathF.Sqrt(dist);
					Projectile.velocity = (Projectile.velocity * 15f + direction) / 16f;
				}
			}
			for (int i = 0; i < 3; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Solution_D>(), 0f, 0f, 20, new(110, 0, 0), 1.1f);
				dust.noGravity = true;
				dust.velocity *= 0f;
				dust.position.X -= Projectile.velocity.X * 0.334f * i;
				dust.position.Y -= Projectile.velocity.Y * -0.334f * i;
			}
		}
	}
}
