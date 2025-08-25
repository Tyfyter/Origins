using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Armor.Felnum;
using Origins.Items.Vanity.Other;
using Origins.Tiles.Other;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Felnum {
	public class Cloud_Elemental : ModNPC, IWikiNPC {
		AutoLoadingAsset<Texture2D> armTexture = typeof(Cloud_Elemental).GetDefaultTMLName() + "_Fingergun";
		public Rectangle DrawRect => new(0, 0, 70, 46);
		public int AnimationFrames => 16;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		//public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.DD2DarkMageT1;
			Main.npcFrameCount[NPC.type] = 4;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][Static_Shock_Debuff.ID] = true;
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 120;
			NPC.defense = 32;
			NPC.damage = 15;
			NPC.width = 70;
			NPC.height = 46;
			NPC.knockBackResist = 0.5f;
			NPC.HitSound = SoundID.NPCHit30.WithPitchOffset(0.25f);
			NPC.DeathSound = SoundID.NPCDeath33.WithPitchOffset(0.25f);
			NPC.noGravity = true;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			if (spawnInfo.Player.ZoneSkyHeight && NPC.downedBoss3) return 0.4f;
			return 0;
		}
		public override void FindFrame(int frameHeight) {
			NPC.DoFrames(4);
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => NPC.playerInteraction[target.whoAmI] || !target.OriginPlayer().felnumEnemiesFriendly;
		Vector2 ShoulderPos => NPC.Center - new Vector2(9 * NPC.spriteDirection, 7);
		public override void AI() {
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.All,
				player => NPC.playerInteraction[player.whoAmI] || !player.OriginPlayer().felnumEnemiesFriendly,
				npc => npc.CanBeChasedBy()
			);
			NPC.target = searchResults.NearestTargetIndex;
			if (searchResults.FoundTarget) {
				if (searchResults.NearestTargetHitbox.Center().IsWithin(NPC.Center, 16 * 100)) {
					NPC.targetRect = searchResults.NearestTargetHitbox;
					if (NPC.ShouldFaceTarget(ref searchResults)) {
						NPC.FaceTarget();
					}
				} else {
					NPC.target = -1;
				}
			}
			if (NPC.HasValidTarget) {
				NPCAimedTarget target = NPC.GetTargetData();
				const int charge_time = 9 * 3;
				const int attack_time_total = 180;
				const int attack_delay = attack_time_total - charge_time;
				Vector2 vectorToTargetPosition = target.Center - ShoulderPos;
				if (NPC.confused) vectorToTargetPosition *= -1;
				NPC.spriteDirection = Math.Sign(vectorToTargetPosition.X);
				float dist = vectorToTargetPosition.Length();
				vectorToTargetPosition /= dist;

				if (++NPC.ai[0] > attack_delay) {
					float chargeFactor = (attack_time_total - NPC.ai[0]) / charge_time;
					NPC.velocity *= (chargeFactor + 299) / 300;
					if (Main.rand.NextFloat(0, 1) < Math.Pow((NPC.ai[0] - attack_delay) / charge_time, 3) * 0.5f) SoundEngine.PlaySound(Origins.Sounds.LittleZap, ShoulderPos);

					if (NPC.ai[0] > attack_time_total) {
						NPC.ai[0] = 0;
						if (Main.netMode != NetmodeID.MultiplayerClient) {
							Projectile.NewProjectile(
								NPC.GetSource_FromAI(),
								ShoulderPos,
								GeometryUtils.Vec2FromPolar(8, NPC.rotation),
								ModContent.ProjectileType<Cloud_Elemental_P>(),
								20 + (int)(10 * ContentExtensions.DifficultyDamageMultiplier),
								4
							);
						}
						SoundEngine.PlaySound(Main.rand.Next(Origins.Sounds.LightningSounds), NPC.Center);
					}
				} else {
					NPC.rotation = vectorToTargetPosition.ToRotation();
				}
			} else {
				NPC.rotation = NPC.velocity.ToRotation();
			}
			NPC.velocity *= 0.94f;
			NPC.velocity.X += (NPC.velocity.X < 0 ? -1 : 1) * 0.1f;
			NPC.spriteDirection = Math.Sign(Math.Cos(NPC.rotation));
			Vector2 nextVel = Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, true, true);
			if (nextVel.X != NPC.velocity.X) NPC.velocity.X *= -0.7f;
			if (nextVel.Y != NPC.velocity.Y) NPC.velocity.Y *= -0.7f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Felnum_Ore_Item>(), 1, 7, 14));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Felnum_Shock_Glasses>(), 20));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ancient_Felnum_Helmet>(), 66));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ancient_Felnum_Breastplate>(), 66));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ancient_Felnum_Greaves>(), 66));
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects spriteEffects = SpriteEffects.None;
			float rotation = NPC.rotation;
			if (NPC.spriteDirection != -1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			} else {
				rotation += MathHelper.Pi;
			}
			spriteBatch.Draw(
				armTexture,
				ShoulderPos - screenPos,
				null,
				drawColor,
				rotation,
				spriteEffects.ApplyToOrigin(new(28, 6), armTexture.Frame()),
				NPC.scale,
				spriteEffects,
			0);
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				NPC.Center - screenPos,
				NPC.frame,
				drawColor,
				0,
				spriteEffects.ApplyToOrigin(new(35, 23), NPC.frame),
				NPC.scale,
				spriteEffects,
			0);
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				SoundEngine.PlaySound(SoundID.NPCHit37, NPC.Center);
				Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Felnum_Enemy_Death_Dust>());

				for (int i = 0; i < 20; i++) {
					Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Smoke, 0f, 0f, 50, Color.White, 1.5f);
					dust.velocity = NPC.velocity + dust.velocity * 2f;
					dust.noGravity = true;
				}
				for (int i = 0; i < 4; i++) {
					Gore.NewGoreDirect(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, Main.rand.Next(61, 64), NPC.scale).velocity *= 0.3f;
				}
			}
		}
	}
	public class Cloud_Elemental_P: Felnum_Guardian_P {
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.friendly = false;
		}
		public override bool CanHitPlayer(Player target) => !target.OriginPlayer().felnumEnemiesFriendly;
	}
}
