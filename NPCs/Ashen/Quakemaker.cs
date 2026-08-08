using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Gores;
using Origins.Items.Accessories;
using Origins.World.BiomeData;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.NPCs.Ashen {
	public class Quakemaker_Head : WormHead {
		public static AutoLoadingTexture altHeadTexture = typeof(Quakemaker_Head).GetDefaultTMLName() + "_Alt";
		public override int BodyType => ModContent.NPCType<Quakemaker_Body>();
		public override int TailType => ModContent.NPCType<Quakemaker_Tail>();
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.NPCs.Quakemaker.DisplayName");
		public override void Load() => this.AddBanner();
		public override bool SharesDebuffs => true;
		public bool UseAltHead = false;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 2;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				CustomTexturePath = "Origins/UI/Quakemaker", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(25f, 12f),
				PortraitPositionXOverride = 4,
				PortraitPositionYOverride = 0f
			};
			ModContent.GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 40;
			NPC.lifeMax = 150;
			NPC.defense = 12;
			NPC.damage = 23;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			//NPC.scale = 0.9f;
			NPC.value = 300;
			SpawnModBiomes = [
				ModContent.GetInstance<Underground_Ashen_Biome>().Type,
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.CustomBestiaryName(Type, this.GetLocalizationKey("FullName").Replace("_Head", ""));
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerSafe) return 0;
			if (spawnInfo.SpawnTileY < Main.rockLayer) return 0;
			return Ashen_Biome.SpawnRates.Quakemaker;
		}

		public override void Init() {
			MinSegmentLength = 10;
			MaxSegmentLength = 10;
			MoveSpeed = 8.5f;
			Acceleration = 0.085f;
		}
		protected override void HeadAI_Movement_PlayDigSounds(float distance) {
			if (NPC.soundDelay == 0 && DigSound.HasValue) {
				// Play sounds quicker the closer the NPC is to the target location
				float delay = distance / 40f;

				if (delay < 10)
					delay = 10f;

				if (delay > 20)
					delay = 20f;

				NPC.soundDelay = (int)delay;

				SoundEngine.PlaySound(DigSound, NPC.Center);
				Rectangle hitbox = NPC.Hitbox.Add((NPC.rotation - MathHelper.PiOver2).ToRotationVector2() * 16);
				hitbox.Inflate(-12, -12);
				if (hitbox.OverlapsAnyTiles()) {
					Main.instance.CameraModifiers.Add(new CameraShakeModifier(
						NPC.Center, 2f, 1f, 10, 750f, -1f, "Quakemaker"
					));
				}
			}
		}
		protected internal override void HeadAI() {
			bool isAlreadyAttacking = false;
			float totalSegs = 0;
			float groundedSegs = 0;
			foreach (NPC segment in IterateWorm()) {
				if (segment != NPC && (segment.ai[2] != 0 || segment.localAI[3] != 0)) isAlreadyAttacking = true;
				totalSegs++;
				if (segment.Hitbox.OverlapsAnyTiles(false)) groundedSegs++;
			}
			if (NPC.velocity != default) NPC.GravityMultiplier *= float.Pow(1 - groundedSegs / totalSegs, 1);

			Vector2 target = NPC.GetTargetData().Center;
			float attackRot = (target - NPC.Center).ToRotation();
			float diff = GeometryUtils.AngleDif(attackRot, NPC.rotation - MathHelper.PiOver2, out int dir);
			if (diff < 0.7f && target.WithinRange(NPC.Center, 16 * 30)) {
				ForcedTargetPosition = target;
			} else {
				ForcedTargetPosition = target + (attackRot + dir * MathHelper.PiOver2).ToRotationVector2() * (16 * 10 + target.Distance(NPC.Center) * 0.1f);
			}
			if (!isAlreadyAttacking && NPC.ai[2].CycleDown(90)) {
				if (diff < 1) {
					NPC.ai[2] = 5;
					goto skipAttack;
				}
				if (NPC.Hitbox.OverlapsAnyTiles() || !CollisionExt.CanHitRay(NPC.Center, target)) {
					NPC.ai[2] = 10;
					goto skipAttack;
				}
				(FollowerNPC.ai[2], FollowerNPC.localAI[3]) = NPC.Center;
				FollowerNPC.localAI[2] = attackRot;
			}
			skipAttack:
			base.HeadAI();
		}
		public override void HitEffect(NPC.HitInfo hit) {
			TryDeathEffect();
		}
		public void TryDeathEffect() {
			if (NPC.life > 0 || NPC.aiAction == 1) return;
			NPC.aiAction = 1;
			NPC current = NPC;
			Vector2 velocity = NPC.velocity * 1.25f;
			float speed = velocity.Length();
			HashSet<int> indecies = [];
			int tailType = TailType;
			while (current.ai[0] != 0) {
				if (!indecies.Add(current.whoAmI)) break;
				for (int i = 0; i < 4; i++) {
					Gore.NewGorePerfect(
						current.GetSource_Death(),
						current.position,
						(velocity + current.velocity) * 0.5f + Main.rand.NextVector2Circular(2, 2),
						GoreCache.Ashen_Generic
					);
				}
				if (current.type == tailType) break;
				NPC next = Main.npc[(int)current.ai[0]];
				velocity = next.DirectionTo(current.Center) * speed;
				current = next;
			}
		}
		public override void OnSpawn(IEntitySource source) {
			UseAltHead = Main.rand.NextBool();
		}
		public override void SendWormAI(BinaryWriter writer) {
			writer.Write(UseAltHead);
		}
		public override void ReceiveWormAI(BinaryReader reader) {
			UseAltHead = reader.ReadBoolean();
		}
		public override void FindFrame(int frameHeight) {
			NPC.DoFrames(4);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (UseAltHead) {
				Main.EntitySpriteDraw(
					altHeadTexture.Value,
					NPC.Center - screenPos,
					NPC.frame,
					drawColor,
					NPC.rotation,
					NPC.frame.Size() * 0.5f,
					NPC.scale,
					SpriteEffects.None);
			}
			return !UseAltHead;
		}
	}
	public class Quakemaker_Body : WormBody {
		public static AutoLoadingTexture GlowTexture = typeof(Quakemaker_Body).GetDefaultTMLName() + "_Glow";
		public override bool SharesImmunityFrames => true;
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.NPCs.Quakemaker.DisplayName");
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			OriginsSets.NPCs.HideDebuffIndicators[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 40;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			//NPC.scale = 0.9f;
		}
		public override void AI() {
			if (!Main.npc.IndexInRange(NPC.realLife)) return;
			NPC.color = HeadSegment.color;
			NPC.alpha = HeadSegment.alpha;
			NPC.GivenName = HeadSegment.GivenName;
			if (NPC.ai[2] != 0 || NPC.localAI[3] != 0) {
				Vector2 pos = new(NPC.ai[2], NPC.localAI[3]);
				if (NPC.Hitbox.Contains(pos)) {
					NPC.SpawnProjectile(
						NPC.GetSource_FromAI(),
						pos,
						NPC.localAI[2].ToRotationVector2() * 8,
						ProjectileID.BulletSnowman,
						1,
						0
					);
					if (FollowerNPC is not null) {
						Rectangle region = NPC.Hitbox;
						region.Inflate(-region.Width / 4, -region.Height / 4);
						(FollowerNPC.ai[2], FollowerNPC.localAI[3]) = pos.Clamp(region);
						FollowerNPC.localAI[2] = NPC.localAI[2];
					}
					NPC.ai[2] = 0;
					NPC.localAI[3] = 0;
				} else if (!NPC.Hitbox.IsWithin(pos, 16 * 5)) {
					NPC.ai[2] = 0;
					NPC.localAI[3] = 0;
				}
			}
			if (NPC.TryGetGlobalNPC(out Blind_Debuff_Global blindGlobal)) blindGlobal.blindable = true;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			(HeadSegment.ModNPC as Quakemaker_Head)?.TryDeathEffect();
		}
		public override void Init() {
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
		public override void UpdateLifeRegen(ref int damage) {
			damage = ushort.MaxValue;
			NPC.lifeRegen = 0;
			NPC.lifeRegenCount = 0;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Glowing_Mod_NPC.DrawGlow(spriteBatch, screenPos, GlowTexture, NPC, NPC.GetTintColor(Color.White));
		}
	}
	internal class Quakemaker_Tail : WormTail {
		public override bool SharesImmunityFrames => true;
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.NPCs.Quakemaker.DisplayName");
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			OriginsSets.NPCs.HideDebuffIndicators[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 48;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			//NPC.scale = 0.9f;
		}
		public override void AI() {
			if (!Main.npc.IndexInRange(NPC.realLife)) return;
			NPC.color = HeadSegment.color;
			NPC.alpha = HeadSegment.alpha;
			NPC.GivenName = HeadSegment.GivenName;
			NPC.ai[2] = 0;
			NPC.localAI[3] = 0;
			if (NPC.TryGetGlobalNPC(out Blind_Debuff_Global blindGlobal)) blindGlobal.blindable = true;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			(HeadSegment.ModNPC as Quakemaker_Head)?.TryDeathEffect();
		}
		public override void Init() {
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
		public override void UpdateLifeRegen(ref int damage) {
			damage = ushort.MaxValue;
			NPC.lifeRegen = 0;
			NPC.lifeRegenCount = 0;
		}
	}
}