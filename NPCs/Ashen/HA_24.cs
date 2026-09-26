using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Gores;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Ashen;
public class HA_24 : Glowing_Mod_NPC, IAshenEnemy {
	public static int TransformTime => 32;
	public static int ID { get; private set; }
	AutoLoadingTexture transformTexture;
	AutoLoadingTexture transformGlowTexture;
	public float patrolMin;
	public float patrolMax;
	public override void SetStaticDefaults() {
		this.SetIDProp();
		NPCID.Sets.UsesNewTargetting[Type] = true;
		if (Type != ID) return;
		Main.npcFrameCount[NPC.type] = 10;
		NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() {
			Scale = 0.85f,
			PortraitScale = 1,
			Rotation = MathHelper.PiOver4 * 3,
			Position = new Vector2(-3, 8),
			PortraitPositionXOverride = 0,
			PortraitPositionYOverride = 0
		};
	}
	public override void SetDefaults() {
		NPC.aiStyle = NPCAIStyleID.ActuallyNone;
		NPC.lifeMax = 1750;
		NPC.defense = 16;
		NPC.damage = 35;
		NPC.width = 74;
		NPC.height = 152;
		NPC.knockBackResist = 0f;
		NPC.value = 20000;
		NPC.noGravity = false;
		NPC.noTileCollide = false;
		NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-2f);
		SpawnModBiomes = [
			ModContent.GetInstance<Ashen_Biome>().Type
		];
		transformTexture = Texture + "_Mode_Switch";
		transformGlowTexture = Texture + "_Mode_Switch_Glow";
	}
	public override void OnSpawn(IEntitySource source) {
		if (NPC.ai[0] == 0) {
			patrolMin = NPC.Center.X - 16 * 100;
			patrolMax = NPC.Center.X + 16 * 100;
		} else {
			patrolMin = NPC.ai[0];
			patrolMax = NPC.ai[1];
		}
		NPC.ai[0] = 0;
		NPC.ai[1] = 0;
	}
	public override void ModifyNPCLoot(NPCLoot npcLoot) {
	}
	public override void AI() {
		NPC.noGravity = false;
		switch (NPC.aiAction) {
			case -1: {
				NPC.noGravity = true;
				NPC.velocity *= 0.93f;
				if (NPC.localAI[0].CycleUp(TransformTime)) Transform(HA_24_Flying.ID);
				break;
			}
			case 0: {
				if (NPC.ai[0].Warmup(60)) {
					NPC.aiAction = -1;
					NPC.velocity.Y = -7;
					NPC.netUpdate = true;
				}
				break;
			}
		}
	}
	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
		bestiaryEntry.AddTags(
			this.GetBestiaryFlavorText()
		);
	}
	public override void FindFrame(int frameHeight) {
		NPC.DoFrames(6);
	}
	public override void HitEffect(NPC.HitInfo hit) {
		if (NPC.life <= 0) {
			OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[0]);
			OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[1]);
			OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[2]);
			OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[3]);
			for (int i = 0; i < 7; i++) {
				OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic);
			}
		} else if (Main.rand.NextBool(5)) {
			OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic);
		}
	}
	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		if (NPC.localAI[0] > 0) {
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (NPC.spriteDirection == 1) spriteEffects = SpriteEffects.FlipHorizontally;
			Rectangle frame = transformTexture.Frame(verticalFrames: 5, frameY: (int)(NPC.localAI[0] * 5f / TransformTime));
			spriteBatch.DrawGlowingNPCPart(
				transformTexture,
				transformGlowTexture,
				NPC.Center - screenPos,
				frame,
				drawColor,
				GetGlowColor(drawColor),
				NPC.rotation,
				frame.Size() * 0.5f,
				NPC.scale,
				spriteEffects
			);
			return false;
		}
		return base.PreDraw(spriteBatch, screenPos, drawColor);
	}
	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		if (NPC.localAI[0] > 0) return;
		using ScopedOverride<bool> _ = NPC.oiled.ScopedOverride(false);
		Texture2D glowTexture = GlowTexture;
		SpriteEffects spriteEffects = SpriteEffects.None;
		if (NPC.spriteDirection == 1) spriteEffects = SpriteEffects.FlipHorizontally;
		Vector2 halfSize = new(glowTexture.Width / 2, glowTexture.Height / Main.npcFrameCount[NPC.type] / 2);
		spriteBatch.Draw(
			glowTexture,
			new Vector2(NPC.position.X - screenPos.X + (NPC.width / 2) - glowTexture.Width * NPC.scale / 2f + halfSize.X * NPC.scale, NPC.position.Y - screenPos.Y + NPC.height - glowTexture.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4f + halfSize.Y * NPC.scale + Main.NPCAddHeight(NPC) + NPC.gfxOffY),
			NPC.frame,
			GetGlowColor(drawColor),
			NPC.rotation,
			halfSize,
			NPC.scale,
			spriteEffects,
		0);
	}
	public override void SendExtraAI(BinaryWriter writer) {
		writer.Write(NPC.aiAction);
	}
	public override void ReceiveExtraAI(BinaryReader reader) {
		NPC.aiAction = reader.ReadInt32();
	}
	protected void Transform(int targetType) {
		Vector2 center = NPC.Center;
		NPC.aiAction = 0;
		NPC.Transform(targetType);
		NPC.Center = center;
		if (NPC.ModNPC is HA_24 ha24) {
			ha24.patrolMin = patrolMin;
			ha24.patrolMax = patrolMax;
		}
	}
}
public class HA_24_Flying : HA_24, IPlatformNPC {
	public static new int ID { get; private set; }
	public Vector2 PlatformOffset => new(26 + 12 * NPC.direction, 0);
	public float PlatformWidth => 172;
	Vector2 IPlatformNPC.OldPlatformPosition { get; set; }
	public override void SetStaticDefaults() {
		base.SetStaticDefaults();
		Main.npcFrameCount[NPC.type] = 4;
		NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
	}
	readonly float[] dists = new float[8];
	Vector2 LaserStartPos => NPC.Center + new Vector2(NPC.direction * 66 - 4, 0);
	public override void SetDefaults() {
		base.SetDefaults();
		NPC.defense = 0;
		NPC.damage = 35;
		NPC.width = 224;
		NPC.height = 38;
		NPC.noGravity = true;
	}
	public override void ModifyNPCLoot(NPCLoot npcLoot) {
	}
	public override void AI() {
		Blind_Debuff_Global blindGlobal = NPC.GetGlobalNPC<Blind_Debuff_Global>();
		blindGlobal.blindable = true;
		if (NPC.direction == 0) NPC.direction = -1;
		switch (NPC.aiAction) {
			case -1: {
				NPC.velocity *= 0.93f;
				if (NPC.localAI[0].CycleUp(TransformTime)) Transform(HA_24.ID);
				break;
			}
			case 0: {
				NPC.velocity.X += NPC.direction * 0.25f;
				NPC.velocity *= 0.97f;
				Vector2 originalPos = LaserStartPos;
				Vector2 laserPos = originalPos;
				Rectangle basicCheck = new((int)laserPos.X, (int)laserPos.Y, dists.Length, 0);
				for (int i = 0; i < dists.Length; i++) {
					dists[i] = CollisionExt.Raymarch(laserPos, Vector2.UnitY, 16 * 256);
					Max(ref basicCheck.Height, (int)dists[i]);
					laserPos.X++;
				}
				if (blindGlobal.blinded) break;
				int newTarget = -1;
				foreach (Player player in Main.ActivePlayers) {
					if (player.dead || player.invis || player.stealth <= 0) continue;
					Rectangle hitbox = player.Hitbox;
					if (!hitbox.Intersects(basicCheck)) continue;
					int maxIndex = dists.Length - (int)float.Max(laserPos.X - hitbox.Right, 0);
					for (int i = (int)float.Max(originalPos.X - hitbox.X, 0); i < maxIndex; i++) {
						if (hitbox.Y <= originalPos.Y + dists[i]) {
							newTarget = player.whoAmI;
							goto foundTarget;
						}
					}
				}
				foreach (NPC other in Main.ActiveNPCs) {
					if (!other.chaseable || other.ModNPC is IAshenEnemy || OriginsSets.NPCs.TargetDummies[other.type]) continue;
					Rectangle hitbox = other.Hitbox;
					if (!hitbox.Intersects(basicCheck)) continue;
					int maxIndex = dists.Length - (int)float.Max(laserPos.X - hitbox.Right, 0);
					for (int i = (int)float.Max(originalPos.X - hitbox.X, 0); i < maxIndex; i++) {
						if (hitbox.Y <= originalPos.Y + dists[i]) {
							newTarget = other.WhoAmIToTargettingIndex;
							goto foundTarget;
						}
					}
				}
				foundTarget:
				if (newTarget != -1) {
					NPC.target = newTarget;
					NPC.aiAction = 1;
					NPC.netUpdate = true;
					if (!NPC.GetGlobalNPC<OriginGlobalNPC>().silencedDebuff) {
						int packhunter = ModContent.NPCType<Packhunter>();
						Rectangle target = NPC.GetTargetData().Hitbox;
						foreach (NPC npc in Main.ActiveNPCs) {
							if (npc.type == packhunter && npc.aiAction is 0 or 3 && !npc.confused && npc.IsWithin(NPC.targetRect, 112 * 16)) {
								npc.direction = (target.Center.X > npc.Center.X).ToDirectionInt();
								npc.targetRect = target;
								npc.aiAction = 5;
								npc.ai[0] = 0;
								npc.netUpdate = true;
							}
						}
					}
				}
				if (patrolMin != 0 && NPC.direction < 0 && LaserStartPos.X < patrolMin) NPC.direction *= -1;
				else if (patrolMax != 0 && NPC.direction > 0 && LaserStartPos.X > patrolMax) NPC.direction *= -1;
				else {
					Vector2 checkPos = NPC.Center + new Vector2(NPC.direction * NPC.width * 0.5f, 0);
					if ((NPC.collideX && Math.Sign(NPC.oldVelocity.X) == NPC.direction) || !CollisionExt.CanHitRay(checkPos, checkPos + new Vector2(NPC.direction * 16 * 10, 0))) {
						if (LaserStartPos.X < patrolMin) patrolMin = 0;
						if (LaserStartPos.X > patrolMax) patrolMax = 0;
						NPC.direction *= -1;
					}
				}
				break;
			}
			case 1: {
				NPCAimedTarget target = NPC.GetTargetData();
				if (target.Invalid) {
					NPC.aiAction = 0;
					NPC.netUpdate = true;
					break;
				}
				Vector2 dir = (target.Center - NPC.Center).Normalized(out float dist);
				NPC.velocity += dir * 0.25f;
				NPC.velocity *= 0.97f;
				if (dist < 16 * 30) {
					NPC.aiAction = -1;
					NPC.netUpdate = true;
				}
				break;
			}
		}
		NPC.spriteDirection = NPC.direction;
	}
	public override bool CheckActive() => false;
	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) { }
	public override void FindFrame(int frameHeight) {
		NPC.DoFrames(4);
	}
	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		if (!base.PreDraw(spriteBatch, screenPos, drawColor)) return false;
		switch (NPC.aiAction) {
			case 0:
			Vector2 laserPos = LaserStartPos + Main.screenPosition - screenPos;
			laserPos.X += 2;
			for (int i = 2; i < dists.Length - 2; i++) {
				spriteBatch.DrawLine(
					Color.OrangeRed * 0.8f,
					laserPos,
					laserPos + new Vector2(0, dists[i]),
					1
				);
				laserPos.X++;
			}
			break;
		}
		return base.PreDraw(spriteBatch, screenPos, drawColor);
	}
	public override void HitEffect(NPC.HitInfo hit) {
		if (NPC.life <= 0) {
			OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[0]);
			OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[1]);
			OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[2]);
			OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[3]);
			for (int i = 0; i < 7; i++) {
				OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic);
			}
		} else if (Main.rand.NextBool(5)) {
			OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic);
		}
	}
}
