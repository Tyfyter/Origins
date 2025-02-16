using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine {
	public class Brine_Serpent_Head : WormHead, IBrinePoolNPC, ICustomWikiStat {
		public override int BodyType => ModContent.NPCType<Brine_Serpent_Body>();
		public override int TailType => ModContent.NPCType<Brine_Serpent_Tail>();
		public int PathfindingTime { get; set; }
		public bool TargetIsRipple { get; set; }
		public bool CanSeeTarget { get; set; }
		public Vector2 TargetPos { get; set; }
		public HashSet<int> TargetNPCTypes => TargetNPCTypes;
		public static HashSet<int> TargetTypes { get; private set; } = [];
		public static HashSet<int> SegmentTypes { get; private set; } = [];
		public override bool SharesDebuffs => true;
		public override bool HasCustomBodySegments => true;
		//public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 9;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Position = new Vector2(-28f, 8f),
				PortraitPositionXOverride = -96f
			};
			NPCID.Sets.UsesNewTargetting[Type] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<Toxic_Shock_Debuff>()] = true;
			ModContent.GetInstance<Brine_Pool.SpawnRates>().AddSpawn(Type, SpawnChance);
			SegmentTypes = [
				Type,
				ModContent.NPCType<Brine_Serpent_Body>(),
				ModContent.NPCType<Brine_Serpent_Body2>(),
				ModContent.NPCType<Brine_Serpent_Tail>()
			];
		}
		public override void Unload() {
			TargetTypes = null;
			SegmentTypes = null;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 24;
			NPC.lifeMax = 380;
			NPC.defense = 27;
			NPC.damage = 26;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath23;
			//NPC.scale = 0.9f;
			NPC.value = 450;
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
		}
		public override void PostAI() {
			base.PostAI();
			Brine_Pool_NPC.HitOtherNPCs(NPC);
		}
		public override int SpawnBodySegments(int segmentCount) {
			int latestNPC = NPC.whoAmI;
			IEntitySource source = NPC.GetSource_FromAI();
			while (segmentCount > 0) {
				latestNPC = SpawnSegment(source, BodyType, latestNPC);
				segmentCount--;
			}
			latestNPC = SpawnSegment(source, ModContent.NPCType<Brine_Serpent_Body2>(), latestNPC);
			return latestNPC;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Brine_Pool.SpawnRates.Snek;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(new LeadingConditionRule(DropConditions.PlayerInteraction).WithOnSuccess(
				ItemDropRule.ByCondition(new Conditions.IsHardmode(), ModContent.ItemType<Alkaliphiliac_Tissue>(), 1, 5, 8)
			));
		}

		public override void Init() {
			MinSegmentLength = MaxSegmentLength = 11;
			MoveSpeed = 4f;
			Acceleration = 0.1f;
			DigSound = null;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			target.AddBuff(BuffID.Poisoned, 240);
		}
		public override bool CanHitNPC(NPC target) => TargetTypes.Contains(target.type) || (target.ModNPC is not IBrinePoolNPC && !SegmentTypes.Contains(target.type));
		public virtual bool CanTargetNPC(NPC other) {
			if (other.type == NPCID.TargetDummy) return false;
			return other.wet && CanHitNPC(other);
		}
		public virtual bool CheckTargetLOS(Vector2 target) => CollisionExt.CanHitRay(NPC.Center, target);
		private static VertexStrip _vertexStrip = new();
		Vector2[] oldPos;
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			float[] oldRot;
			int count;
			if (NPC.IsABestiaryIconDummy) {
				oldRot = new float[5];
				oldPos = new Vector2[5];
				Vector2 pos = NPC.Center;
				count = 5;
				for (int i = 0; i < count; i++) {
					pos.X += 36;
					oldPos[i] = pos;
					oldRot[i] = 0;
				}
				drawColor = Color.White;
			} else {
				NPC current = NPC;
				HashSet<int> indecies = [];
				int tailType = TailType;
				List<NPC> NPCs = [];
				while (current.ai[0] != 0) {
					if (!indecies.Add(current.whoAmI) || (current.realLife != -1 && current.realLife != NPC.whoAmI)) break;
					NPCs.Add(current);
					if (current.type == tailType) break;
					NPC next = Main.npc[(int)current.ai[0]];
					current = next;
				}
				oldRot = new float[NPCs.Count];
				oldPos = new Vector2[NPCs.Count];
				for (int i = 0; i < NPCs.Count - 1; i++) {
					Vector2 diff = NPCs[i + 1].Center - NPCs[i].Center;
					oldPos[i] = NPCs[i].Center + diff * 0.5f;
					oldRot[i] = diff.ToRotation();
				}
				oldRot[^1] = oldRot[^2];
				oldPos[^1] = oldPos[^2] + GeometryUtils.Vec2FromPolar(36, oldRot[^1]);
				count = NPCs.Count;
			}
			SpriteEffects effects = SpriteEffects.None;
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:Identity"];
			miscShaderData.UseImage0(TextureAssets.Npc[BodyType]);
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(0, 0, 0, 1));
			if (Math.Sin(NPC.rotation) > 0) {
				miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(0, 0, 1, 1));
				effects = SpriteEffects.FlipVertically;
			} else {
				miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(0, 1, 1, -1));
			}
			miscShaderData.Apply();
			_vertexStrip.PrepareStrip(oldPos, oldRot, GetLightColor, _ => 14, -screenPos, count, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			NPCLoader.DrawEffects(NPC, ref drawColor);
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				oldPos[0] - screenPos,
				NPC.frame,
				NPC.GetNPCColorTintedByBuffs(drawColor),
				oldRot[0],
				new Vector2(38, 13).Apply(effects, new(38, 36)),
				1,
				effects,
			0);
			return false;
		}
		Color GetLightColor(float progress) {
			if (NPC.IsABestiaryIconDummy) return Color.White;
			progress = MathHelper.Clamp(progress * oldPos.Length, 0, oldPos.Length);
			int p = (int)progress;
			Color npcColor = new(Lighting.GetSubLight(progress < 1 ? Vector2.Lerp(oldPos[p], oldPos[p + 1], progress - p) : oldPos[^1]));
			NPCLoader.DrawEffects(NPC, ref npcColor);
			return NPC.GetNPCColorTintedByBuffs(npcColor);
		}
	}

	internal class Brine_Serpent_Body : WormBody {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<Toxic_Shock_Debuff>()] = true;
		}
		public override bool SharesImmunityFrames => true;
		public override float SegmentSeparation => 36;
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 24;
		}
		public override void Init() {
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
		public override void PostAI() {
			base.PostAI();
			Brine_Pool_NPC.HitOtherNPCs(NPC);
		}
		public override bool CanHitNPC(NPC target) => Brine_Serpent_Head.TargetTypes.Contains(target.type) || (target.ModNPC is not IBrinePoolNPC && !Brine_Serpent_Head.SegmentTypes.Contains(target.type));
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;
	}

	internal class Brine_Serpent_Body2 : WormBody {
		public override string Texture => typeof(Brine_Serpent_Head).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<Toxic_Shock_Debuff>()] = true;
		}
		public override bool SharesImmunityFrames => true;
		public override float SegmentSeparation => 36;
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 24;
		}
		public override void Init() {
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
		public override void PostAI() {
			base.PostAI();
			Brine_Pool_NPC.HitOtherNPCs(NPC);
		}
		public override bool CanHitNPC(NPC target) => Brine_Serpent_Head.TargetTypes.Contains(target.type) || (target.ModNPC is not IBrinePoolNPC && !Brine_Serpent_Head.SegmentTypes.Contains(target.type));
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;
	}

	internal class Brine_Serpent_Tail : WormTail {
		public override string Texture => typeof(Brine_Serpent_Head).GetDefaultTMLName();
		public override float SegmentSeparation => 36;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<Toxic_Shock_Debuff>()] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.width = NPC.height = 24;
		}
		public override void Init() {
			MoveSpeed = 5.5f;
			Acceleration = 0.045f;
		}
		public override void PostAI() {
			base.PostAI();
			Brine_Pool_NPC.HitOtherNPCs(NPC);
		}
		public override bool CanHitNPC(NPC target) => Brine_Serpent_Head.TargetTypes.Contains(target.type) || (target.ModNPC is not IBrinePoolNPC && !Brine_Serpent_Head.SegmentTypes.Contains(target.type));
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;
	}
}
