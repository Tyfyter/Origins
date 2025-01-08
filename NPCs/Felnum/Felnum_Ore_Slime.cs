using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Buffs;
using Origins.Reflection;
using Origins.Tiles.Other;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Felnum {
	public class Felnum_Ore_Slime : ModNPC {
		//public override string Texture => "Terraria/Images/NPC_" + NPCID.BlueSlime;
		public override void SetStaticDefaults() {
			NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.ShimmerSlime;//maybe undead viking instead?
			Main.npcFrameCount[NPC.type] = 2;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BlueSlime);
			NPC.lifeMax = 45;
			NPC.defense = 28;
			NPC.damage = 14;
			NPC.color = new(255, 255, 255, 200);
			AIType = NPCID.BlueSlime;
			AnimationType = NPCID.BlueSlime;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.Player.ZoneSkyHeight && NPC.downedBoss3) 				return 0.085f;
			return 0;
		}
		public override void OnSpawn(IEntitySource source) {
			NPC.ai[1] = ModContent.ItemType<Felnum_Ore_Item>();
		}
		Vector2 oldVelocity;
		public override void AI() {
			if (oldVelocity.Y < 0 && NPC.velocity.Y >= 0) {
				const float max_dist = 16 * 50;
				float distance = Math.Min(Math.Min(
					CollisionExt.Raymarch(NPC.BottomLeft, Vector2.UnitY, max_dist),
					CollisionExt.Raymarch(NPC.Bottom, Vector2.UnitY, max_dist)
					),
					CollisionExt.Raymarch(NPC.BottomRight, Vector2.UnitY, max_dist)
				);
				if (distance > 16 * 3 && distance < max_dist) {
					NPC.Bottom = NPC.Bottom + Vector2.UnitY * distance;
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						Projectile.NewProjectile(
							NPC.GetSource_FromThis(),
							NPC.Bottom - Vector2.UnitY * 24,
							Vector2.Zero,
							ModContent.ProjectileType<Felnum_Ore_Slime_Zap>(),
							NPC.damage,
							6,
							ai0: distance
						);
					}
					SoundEngine.PlaySound(new SoundStyle(Main.rand.NextBool() ? $"Origins/Sounds/Custom/ThunderShot" : $"Terraria/Sounds/Thunder_0", SoundType.Sound).WithPitchRange(-0.1f, 0.1f).WithVolume(0.75f), NPC.Center);
					for (int i = 0; i < distance; i += Main.rand.Next(8, 12)) {
						Dust.NewDust(NPC.Bottom - Vector2.UnitY * i, 0, 0, DustID.Electric, 0f, 0f, 0, Color.White, 0.5f);
					}
					Collision.HitTiles(NPC.position, Vector2.UnitY, NPC.width, NPC.height);
				}
			}
			oldVelocity = NPC.velocity;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Felnum_Ore_Item>(), 1, 7, 14));
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			MainReflection.DrawNPC_SlimeItem(NPC, NPC.type, drawColor, 0);
			return true;
		}
		public override void HitEffect(NPC.HitInfo hit) {
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.AddBuff(ModContent.BuffType<Static_Shock_Debuff>(), 240);
		}
	}
	public class Felnum_Ore_Slime_Zap : ModProjectile {
		public override string Texture => typeof(Felnum_Ore_Slime_Zap).GetDefaultTMLName() + "_Placeholder";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
		}
		public override void SetDefaults() {
			Projectile.width = 64;
			Projectile.height = 48;
			Projectile.timeLeft = 9;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Collision.CheckAABBvLineCollision2(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center - Vector2.UnitY * Projectile.ai[0])) 				return true;
			return null;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.AddBuff(ModContent.BuffType<Static_Shock_Debuff>(), 300);
		}
		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
			miscShaderData.UseSaturation(-1f);
			miscShaderData.UseOpacity(4);
			miscShaderData.Apply();
			int maxLength = 16;
			float[] oldRot = new float[maxLength];
			Vector2[] oldPos = new Vector2[maxLength];
			Vector2 start = Projectile.Center, end = Projectile.Center - Vector2.UnitY * Projectile.ai[0];
			for (int i = 0; i < maxLength; i++) {
				oldPos[i] = Vector2.Lerp(start, end, i / (float)maxLength);
				oldRot[i] = MathHelper.PiOver2;
			}
			_vertexStrip.PrepareStrip(oldPos, oldRot, _ => new Color(80, 204, 219, 0), _ => 12, -Main.screenPosition, maxLength, includeBacksides: false);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: (9 - Projectile.timeLeft) / 3);
			Main.EntitySpriteDraw(
				texture,
				Projectile.Bottom - Main.screenPosition,
				frame,
				new Color(255, 255, 255, 0),
				0,
				new Vector2(42, 72),
				1,
				SpriteEffects.None
			);
			return false;
		}
	}
}
