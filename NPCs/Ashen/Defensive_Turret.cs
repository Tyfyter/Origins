using MagicStorage.CrossMod;
using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Origins.NPCs.Ashen.Boss.Fire_Cannons_State;
using static Terraria.ModLoader.ModContent;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen {
	public class Defensive_Turret : ModNPC, IAshenEnemy, Repairboy.IReparable {
		static AutoLoadingTexture glowTexture = typeof(Defensive_Turret).GetDefaultTMLName("_Glow");
		static AutoLoadingTexture armTexture = typeof(Defensive_Turret).GetDefaultTMLName("_Arm");
		static AutoLoadingTexture gunTexture = typeof(Defensive_Turret).GetDefaultTMLName("_Gun");
		static AutoLoadingTexture gunGlowTexture = typeof(Defensive_Turret).GetDefaultTMLName("_Gun_Glow");
		public override void Load() => this.AddBanner();
		public bool IsDeactivated {
			get => NPC.ai[2] != 0;
			set => NPC.ai[2] = value.ToInt();
		}
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		Vector2 GunOrigin => NPC.Top - new Vector2(NPC.spriteDirection * 10, 4);
		public override void SetDefaults() {
			NPC.lifeMax = 80;
			NPC.defense = 22;
			NPC.damage = 10;
			NPC.width = 68;
			NPC.height = 68;
			NPC.value = Item.buyPrice(0, 0, 2);
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.knockBackResist = 0f;
			SpawnModBiomes = [
				GetInstance<Ashen_Biome>().Type,
			];
		}
		public override void AI() {
			NPC.velocity = default;
			NPC.dontTakeDamage = IsDeactivated;
			NPC.damage = NPC.dontTakeDamage ? 0 : NPC.defDamage;
			if (NPC.dontTakeDamage) return;
			UpdateTarget();

			Vector2 diff = NPC.targetRect.Center() - NPC.Top;
			float angle = diff.ToRotation();
			GeometryUtils.AngularSmoothing(ref NPC.rotation, angle, 0.05f);
			NPC.spriteDirection = diff.X < 0 ? -1 : 1;
			if (NPC.HasValidTarget) {
				NPC.ai[0]++;
				if (GeometryUtils.AngleDif(NPC.rotation, angle, out _) < 0.5f) {
					if (NPC.ai[0] > 60) {
						NPC.ai[0] = 0;
						SoundEngine.PlaySound(Origins.Sounds.HeavyCannon.WithPitch(-0.5f), NPC.Top);
						Vector2 direction = NPC.rotation.ToRotationVector2();
						NPC.SpawnProjectile(null,
							GunOrigin + direction * 20,
							direction * ShotVelocity,
							ModContent.ProjectileType<Trenchmaker_Cannon_P>(),
							ShotDamage,
							1
						);
					}
				} else {
					Min(ref NPC.ai[0], 30);
				}
			} else {
				NPC.ai[0] = 0;
			}
		}
		public void DoTargeting() {
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.Players);
			if (searchResults.FoundTarget) {
				NPC.target = searchResults.NearestTargetIndex;
				NPC.targetRect = searchResults.NearestTargetHitbox;
			} else {
				NPC.target = -1;
				NPC.targetRect = NPC.Hitbox.Add(128 * NPC.direction * Vector2.UnitX);
			}
		}
		public void UpdateTarget() {
			if (!NPC.HasValidTarget) {
				DoTargeting();
				return;
			}
			NPC.targetRect = NPC.GetTargetData().Hitbox;
		}
		public override bool CheckDead() {
			Max(ref NPC.life, 1);
			IsDeactivated = true;
			return false;
		}
		public override bool NeedSaving() => true;
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore1");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore2");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore3");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore4");
				for (int i = 0; i < 7; i++) {
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
				}
			} else if (Main.rand.NextBool(5)) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (!IsDeactivated) {
				SpriteEffects effects = SpriteEffects.None;
				float rotation = NPC.rotation + MathHelper.Pi;
				if (NPC.spriteDirection == 1) {
					effects = SpriteEffects.FlipHorizontally;
					rotation -= MathHelper.Pi;
				}
				spriteBatch.DrawGlowingNPCPart(
					gunTexture,
					gunGlowTexture,
					GunOrigin - screenPos,
					gunTexture.Frame(verticalFrames: 4, frameY: NPC.frame.Y),
					drawColor,
					Color.White,
					rotation,
					new Vector2(80, 23).Apply(effects, gunTexture.Value.Size()),
					NPC.scale,
					effects
				);
				spriteBatch.Draw(
					armTexture,
					NPC.Bottom - screenPos,
					null,
					drawColor,
					0,
					new Vector2(-2, 93).Apply(effects, armTexture.Value.Size()),
					NPC.scale,
					effects,
				0);
			}
			spriteBatch.DrawGlowingNPCPart(
				TextureAssets.Npc[Type].Value,
				glowTexture,
				NPC.Bottom - screenPos,
				null,
				drawColor,
				Color.White,
				0,
				glowTexture.Value.Size() * new Vector2(0.5f, 1),
				NPC.scale,
				SpriteEffects.None
			);
			return false;
		}
		public override void SaveData(TagCompound tag) {
			tag[nameof(IsDeactivated)] = NPC.ai[0];
		}
		public override void LoadData(TagCompound tag) {
			tag.TryGet(nameof(IsDeactivated), out NPC.ai[0]);
			if (IsDeactivated) NPC.life = 1;
		}
		public bool? NeedsRepair(NPC repairboy, ref float cost, ref Rectangle hitbox) => IsDeactivated;
		public bool Repair(int repairAmount) {
			NPC.life += Main.rand.RandomRound(repairAmount * 0.05f);
			if (NPC.life >= NPC.lifeMax) {
				NPC.life = NPC.lifeMax;
				IsDeactivated = false;
			}
			return true;
		}
	}
}
