#define DRAWPLATFORM //uncomment this to see where the platform is
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Core.Animation;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Melee;
using Origins.LootConditions;
using Origins.Tiles.Ashen;
using Origins.Tiles.Ashen.Hanging_Scrap;
using Origins.World.BiomeData;
using ReLogic.Content;
using ReLogic.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Core.Animation.KeyframeData;
using static Origins.NPCs.Ashen.VV13;
using static Terraria.ModLoader.ModContent;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen;
public class VV13 : ModNPC, IWikiNPC, IAshenEnemy, IPlatformNPC, ISpecialTargetingNPC {
	public Rectangle DrawRect => new(0, 0, 34, 46);
	public int AnimationFrames => 3;
	public int FrameDuration => 3;
	public Vector2 PlatformOffset => new(0, 0);
	public float PlatformWidth => 128;
	Vector2 IPlatformNPC.OldPlatformPosition { get; set; }
	static AutoLoadingTexture glowTexture = typeof(VV13).GetDefaultTMLName("_Glow");
	public VV13Animation Animation {
		get => VV13Animation.animations[NPC.aiAction];
		set => NPC.aiAction = value.Type;
	}
	public ref float AnimationTime => ref NPC.ai[0];
	public ref float CurrentToolIndex => ref NPC.ai[1];
	public VV13Tool CurrentTool {
		get => VV13Tool.tools[(int)CurrentToolIndex];
		set => CurrentToolIndex = value.Type;
	}
	public ref float AnimationVar => ref NPC.ai[3];
	public Transform RootTransform => new(NPC.Bottom + Vector2.UnitY * 2) {
		Mirror = NPC.direction == 1
	};
	public override void Load() => this.AddBanner();
	public override void SetStaticDefaults() {
		NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft with {
			Position = new Vector2(35, 0),
			PortraitPositionXOverride = 0,
			PortraitPositionYOverride = 0
		};
		GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, Ashen_Biome.SpawnRates.ScrapyardStryder);
	}
	public override void SetDefaults() {
		NPC.aiStyle = NPCAIStyleID.ActuallyNone;
		NPC.lifeMax = 475;
		NPC.defense = 24;
		NPC.damage = 48;
		NPC.width = 124;
		NPC.height = 46;
		NPC.value = 230;
		NPC.knockBackResist = 0.5f;
		NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
		NPC.DeathSound = SoundID.NPCDeath44;
		NPC.noGravity = true;
		SpawnModBiomes = [
			GetInstance<Ashen_Biome>().Type,
		];
	}
	public override void OnSpawn(IEntitySource source) {
		CurrentTool = VV13Tool.GetRandom();
		AnimationVar = 1;
	}
	public void TargetClosest(bool faceTarget = true, Vector2? checkPosition = null) {
		TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.Players, SearchFilter);
		if (searchResults.FoundTarget) {
			NPC.target = searchResults.NearestTargetIndex;
			NPC.targetRect = searchResults.NearestTargetHitbox;
			if (faceTarget && NPC.ShouldFaceTarget(ref searchResults)) NPC.FaceTarget();
		}
	}
	bool SearchFilter(Player player) => player?.OriginPlayer()?.standingOnPlatformNPC != NPC;
	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
		bestiaryEntry.CustomBestiaryName(Type, this.GetLocalizationKey("FullName"));
		bestiaryEntry.AddTags(
			this.GetBestiaryFlavorText()
		);
	}
	public override void AI() {
		if (Main.rand.NextBool(650)) {
			SoundEngine.PlaySound(Origins.Sounds.VV13Idle, NPC.Center);
		}
		NPC.TargetClosestUpgraded();
		CurrentTool.AI(this);
		Vector2 nextVel = Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, true, true);
		if (nextVel.X != NPC.velocity.X) NPC.velocity.X *= -0.9f;
		if (nextVel.Y != NPC.velocity.Y) NPC.velocity.Y *= -0.9f;
	}
	public void Approach(Vector2 target, float minRange, float maxRange) {
		float speed = 8f;
		float inertia = 128f;
		//NPC.rotation = NPC.velocity.X * 0.1f;
		Vector2 vectorToTargetPosition = (target - NPC.Center).Normalized(out float dist);
		if (NPC.confused) vectorToTargetPosition *= -1;
		if (dist < minRange) {
			speed *= -1;
		} else if (dist < maxRange) {
			speed = 0;
			if (NPC.velocity.LengthSquared() < 0.1f) {
				// If there is a case where it's not moving at all, give it a little "poke"
				NPC.velocity += Main.rand.NextVector2Circular(1, 1) * 0.05f;
			}
		}
		NPC.spriteDirection = Math.Sign(vectorToTargetPosition.X);
		vectorToTargetPosition *= speed;
		NPC.velocity = (NPC.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
	}
	public override bool? CanFallThroughPlatforms() => true;
	public override void FindFrame(int frameHeight) {
		NPC.spriteDirection = NPC.direction;
		VV13Animation animation = Animation;
		float oldTime = AnimationTime;
		animationControls.Play(ref AnimationTime, animation);
		animation.events.Update(this, oldTime, AnimationTime);
	}
	public override void ModifyNPCLoot(NPCLoot npcLoot) {
		npcLoot.Add(ItemDropRule.Common(ItemID.MeatGrinder, 200));
		npcLoot.Add(ScavengerBonus.Scrap(amountDroppedMinimum: 5, amountDroppedMaximum: 11));
		npcLoot.Add(ItemDropRule.Common(ItemType<BBQ_Skewer>(), 19));
		npcLoot.Add(ItemDropRule.Common(ItemType<Phoenum>(), 1, 1, 3));
		npcLoot.Add(ItemDropRule.Common(ItemType<The_Muffler>(), 80));
	}
	public override void HitEffect(NPC.HitInfo hit) {
		base.HitEffect(hit);
	}
	public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
		npcHitbox.Height -= 22;
		npcHitbox.Y += 22;
		return base.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
	}
	public override void SendExtraAI(BinaryWriter writer) {
		writer.Write(NPC.aiAction);
	}
	public override void ReceiveExtraAI(BinaryReader reader) {
		NPC.aiAction = reader.ReadInt32();
	}
	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		drawColor = NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(drawColor));
		using ScopedOverride<bool> o = NPC.oiled.ScopedOverride(false);
		Color glowColor = NPC.GetTintColor(Color.White);
		VV13Animation animation = Animation;
		Transform root = RootTransform;
		root.Position -= screenPos;
		Vector2 dirMult = new(NPC.direction, 1);
		if (animation.HasTool) CurrentTool.Draw(spriteBatch, this, root + animation.tool.GetCurrentValue(AnimationTime), drawColor, animation.toolOpacity.GetCurrentValue(AnimationTime));
		animation.DrawExtras(spriteBatch, this, root, drawColor);
		animation.tentacleMidFront.GetCurrentValue(AnimationTime).Draw(spriteBatch, root + VV13Animation.tentaclePosMidFront, drawColor, 0, NPC.direction);
		animation.tentacleMidBack.GetCurrentValue(AnimationTime).Draw(spriteBatch, root + VV13Animation.tentaclePosMidBack, drawColor, 0, NPC.direction);
		animation.tentacleBack.GetCurrentValue(AnimationTime).Draw(spriteBatch, root + VV13Animation.tentaclePosBack, drawColor, 0, NPC.direction);
		animation.tentacleFront.GetCurrentValue(AnimationTime).Draw(spriteBatch, root + VV13Animation.tentaclePosFront, drawColor, 0, NPC.direction);
		spriteBatch.DrawGlowingNPCPart(
			TextureAssets.Npc[Type].Value,
			glowTexture,
			root.Position,
			NPC.frame,
			drawColor,
			glowColor,
			NPC.rotation,
			NPC.frame.Size() * new Vector2(0.5f, 1),
			NPC.scale,
			NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
		);
		NPC.DrawConfused();
		Editor.vv13 = this;
		Animation_Editor_UI.DrawEditorUI = Editor.DrawUI;
		spriteBatch.DrawDebugTextAbove(CurrentTool, NPC.Top - screenPos);
		return false;
	}
#if DRAWPLATFORM
	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		Vector2 platformStart = NPC.position + PlatformOffset - screenPos;
		OriginExtensions.DrawDebugLineSprite(platformStart, platformStart + PlatformWidth * Vector2.UnitX, Color.Red);
	}
	struct DebugFlag : IDebugFlag;
#endif
	public record struct Tentacle(PolarVec2 A, PolarVec2 B, PolarVec2 C, PolarVec2 D, PolarVec2 E, PolarVec2 F, PolarVec2 G, PolarVec2 H) : IBoneChain<Tentacle> {
		static AutoLoadingTexture texture = typeof(VV13).GetDefaultTMLName("_Arm");
		public static int Length => 8;
		public PolarVec2 A = A, B = B, C = C, D = D, E = E, F = F, G = G, H = H;
		public ref PolarVec2 this[int index] {
			[UnscopedRef]
			get {
				if (index < 0 || index >= Length) throw new IndexOutOfRangeException();
				return ref Unsafe.Add(ref A, index);
			}
		}
		public readonly override string ToString() => $"{A}, {B}, {C}, {D}, {E}, {F}, {G}, {H}";
		public readonly void Draw(SpriteBatch spriteBatch, Transform transform, Color drawColor, int xFrame, int direction) {
			Rectangle frame = texture.Frame(3, 3, xFrame);
			frame.Height = 12;
			int i = 0;
			Vector2 origin = frame.Size() * new Vector2(0.5f, 1);
			Vector2 mult = new(direction, 1);
			Vector2 offset = transform.Position;
			foreach ((Vector2 position, float length, float angle) in Unsafe.AsRef(in this).Iterate()) {
				switch (++i) {
					default:
					frame.Y = 0;
					break;

					case 4:
					frame.Y = 14;
					break;

					case 8:
					frame.Y = 28;
					frame.Height = 24;
					origin.Y = 24;
					break;
				}
				spriteBatch.Draw(
					texture,
					offset + position * mult,
					frame,
					drawColor,
					(angle - MathHelper.PiOver2) * direction,
					origin,
					new Vector2(1, length / frame.Height),
					SpriteEffects.None,
				0);
			}
		}
	}
	KeyframeAnimation.PlayingState animationControls;
	static class Editor {
		public static VV13 vv13;
		public static void DrawUI(SpriteBatch spriteBatch) {
			VV13Animation animation = vv13.Animation;
			animation.gizmoBasePosition = vv13.NPC.Bottom - Main.screenPosition;
			animation.DrawEditorUI(spriteBatch, ref vv13.AnimationTime, ref vv13.animationControls);
			if (KeyframeAnimation.DrawSwapper(spriteBatch, VV13Animation.animations, out VV13Animation selected, vv13.Animation)) vv13.Animation = selected;
		}
	}
}
#region animations
public abstract class VV13Animation : KeyframeAnimation, ILoadable {
	public static List<VV13Animation> animations = [];
	static Tentacle Default => new(new(12, MathHelper.PiOver2), new(12, 0), new(12, 0), new(12, 0), new(12, 0), new(12, 0), new(12, 0), new(24, 0));
	public static readonly Vector2 tentaclePosFront = new(-40, 0);
	public static readonly Vector2 tentaclePosMidFront = new(-10, -2);
	public static readonly Vector2 tentaclePosMidBack = new(18, -4);
	public static readonly Vector2 tentaclePosBack = new(48, -8);
	//front
	public KeyframeSet<Tentacle> tentacleFront = new(Default, tentaclePosFront) {
	};
	// middle front
	public KeyframeSet<Tentacle> tentacleMidFront = new(Default, tentaclePosMidFront) {
	};
	// middle back
	public KeyframeSet<Tentacle> tentacleMidBack = new(Default, tentaclePosMidBack) {
	};
	//back
	public KeyframeSet<Tentacle> tentacleBack = new(Default, tentaclePosBack) {
	};
	[ShowIfTrue(nameof(HasTool))]
	public KeyframeSet<PosRotScale> tool = new(new()) {
	};
	[ShowIfTrue(nameof(HasTool))]
	public KeyframeSet<Percentage> toolOpacity = new(1f) {
	};
	public EventKeyframeSet<VV13> events = [
	];
	public virtual bool HasTool => true;
	public static void Drop(in VV13 vv13) {
		if (NetmodeActive.MultiplayerClient) return;
		if (vv13.CurrentTool is VV13_No_Tool) return;
		PosRotScale transform = vv13.Animation.tool.GetCurrentValue(vv13.AnimationTime);
		Vector2 pos = vv13.NPC.Bottom + Vector2.UnitY * 2 + transform.Position;
		NPC.NewNPC(
			vv13.NPC.GetSource_FromAI(),
			(int)pos.X,
			(int)pos.Y,
			NPCType<VV13_Dropped_Tool>(),
			ai0: vv13.CurrentToolIndex,
			ai1: transform.Rotation
		);
		vv13.CurrentTool = GetInstance<VV13_No_Tool>();
	}
	public static void Dev_CycleTool(in VV13 vv13) =>
		vv13.CurrentToolIndex = (vv13.CurrentToolIndex + 1) % VV13Tool.tools.Count;
	public int Type { get; private set; }
	public void Load(Mod mod) {
		Type = animations.Count;
		animations.Add(this);
	}
	public void Unload() { }
	public virtual void DrawExtras(SpriteBatch spriteBatch, VV13 vv13, Transform root, Color drawColor) { }
}
public class VV13_Idle : VV13Animation {
	public override bool HasTool => false;
	public VV13_Idle() {
		tentacleFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), (true, []), new Vector2(40f, 0f)) {
			new(1, new(new(12f, 1.5173486f), new(12f, -0.1272701f), new(12f, 0.006449938f), new(12f, 0.011137009f), new(12f, 0.11933422f), new(12f, 0.2057916f), new(12f, 0.29269636f), new(24f, 0.23381531f)), []),
			new(13, new(new(12f, 1.535102f), new(12f, -0.03470266f), new(12f, 0.013216019f), new(12f, -0.04069245f), new(12f, 0.047262907f), new(12f, -0.008957386f), new(12f, -0.05913222f), new(24f, -0.22825828f)), []),
			new(26, new(new(12f, 1.5420312f), new(12f, 0.1843797f), new(12f, 0.08090389f), new(12f, -0.23727477f), new(12f, -0.21172202f), new(12f, -0.17237091f), new(12f, -0.22384924f), new(24f, -0.059705675f)), []),
			new(39, new(new(12f, 1.6384833f), new(12f, 0.047383785f), new(12f, 0.13188076f), new(12f, 0.00953567f), new(12f, -0.007999301f), new(12f, -0.15723646f), new(12f, -0.1315198f), new(24f, -0.032798886f)), []),
			new(52, new(new(12f, 1.7015185f), new(12f, -0.01837635f), new(12f, -0.002798438f), new(12f, 0.075674176f), new(12f, 0.06986678f), new(12f, -6.1958466f), new(12f, 0.15277314f), new(24f, 0.1936078f)), []),
			new(65, new(new(12f, 1.3924373f), new(12f, -0.074828506f), new(12f, -0.0052353144f), new(12f, 0.0031436682f), new(12f, 0.11933398f), new(12f, 0.04143071f), new(12f, 0.11646974f), new(24f, 0.2775159f)), [])
		};
		tentacleMidFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), (true, []), new Vector2(10f, -2f)) {
			new(1, new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), []),
			new(13, new(new(12f, 1.4608321f), new(12f, -0.074828506f), new(12f, 0.08090031f), new(12f, 0.0031436682f), new(12f, 0.11933398f), new(12f, 0.04143071f), new(12f, 0.11646974f), new(24f, -0.02321589f)), []),
			new(26, new(new(12f, 1.7015185f), new(12f, -0.01837635f), new(12f, -0.002798438f), new(12f, -0.028679132f), new(12f, -0.06262362f), new(12f, -6.352029f), new(12f, -0.17660463f), new(24f, -0.17309988f)), []),
			new(39, new(new(12f, 1.5173486f), new(12f, -0.1272701f), new(12f, 0.006449938f), new(12f, 0.011137009f), new(12f, 0.11933422f), new(12f, 0.2057916f), new(12f, 0.29269636f), new(24f, 0.23381531f)), []),
			new(52, new(new(12f, 1.535102f), new(12f, -0.03470266f), new(12f, 0.013216019f), new(12f, -0.04069245f), new(12f, 0.047262907f), new(12f, -0.008957386f), new(12f, -0.05913222f), new(24f, -0.22825828f)), []),
			new(65, new(new(12f, 1.5420312f), new(12f, 0.1843797f), new(12f, 0.08090389f), new(12f, -0.23727477f), new(12f, -0.21172202f), new(12f, -0.17237091f), new(12f, -0.22384924f), new(24f, -0.059705675f)), [])
		};
		tentacleMidBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), (true, []), new Vector2(-18f, -4f)) {
			new(1, new(new(12f, 1.5420312f), new(12f, 0.1843797f), new(12f, 0.08090389f), new(12f, -0.23727477f), new(12f, -0.21172202f), new(12f, -0.17237091f), new(12f, -0.22384924f), new(24f, -0.059705675f)), []),
			new(13, new(new(12f, 1.535102f), new(12f, -0.03470266f), new(12f, 0.013216019f), new(12f, -0.04069245f), new(12f, 0.047262907f), new(12f, -0.008957386f), new(12f, -0.05913222f), new(24f, -0.22825828f)), []),
			new(26, new(new(12f, 1.6384833f), new(12f, 0.047383785f), new(12f, 0.13188076f), new(12f, 0.00953567f), new(12f, -0.007999301f), new(12f, -0.15723646f), new(12f, -0.1315198f), new(24f, -0.032798886f)), []),
			new(39, new(new(12f, 1.7015185f), new(12f, -0.01837635f), new(12f, -0.002798438f), new(12f, 0.075674176f), new(12f, 0.06986678f), new(12f, -6.1958466f), new(12f, 0.15277314f), new(24f, 0.1936078f)), []),
			new(52, new(new(12f, 1.5173486f), new(12f, -0.1272701f), new(12f, 0.006449938f), new(12f, 0.011137009f), new(12f, 0.11933422f), new(12f, 0.2057916f), new(12f, 0.29269636f), new(24f, 0.23381531f)), []),
			new(65, new(new(12f, 1.3924373f), new(12f, -0.074828506f), new(12f, -0.0052353144f), new(12f, 0.0031436682f), new(12f, 0.11933398f), new(12f, 0.04143071f), new(12f, 0.11646974f), new(24f, 0.2775159f)), [])
		};
		tentacleBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), (true, []), new Vector2(-48f, -8f)) {
			new(1, new(new(12f, 1.5275428f), new(12f, 0.047383785f), new(12f, 0.08821046f), new(12f, 0.052036643f), new(12f, 0.04112172f), new(12f, 0.074460745f), new(12f, 0.22564828f), new(24f, 0.40869713f)), []),
			new(13, new(new(12f, 1.6142732f), new(12f, -0.076045156f), new(12f, -0.002798438f), new(12f, 0.075674176f), new(12f, 0.06986678f), new(12f, -6.328594f), new(12f, 0.19331968f), new(24f, 0.1936078f)), []),
			new(26, new(new(12f, 1.3924373f), new(12f, -0.074828506f), new(12f, -0.0052353144f), new(12f, 0.05301273f), new(12f, 0.25247478f), new(12f, 0.25156093f), new(12f, 0.11646974f), new(24f, 0.2775159f)), []),
			new(39, new(new(12f, 1.4421482f), new(12f, -0.012227774f), new(12f, -0.04648614f), new(12f, -0.0427922f), new(12f, -0.08577466f), new(12f, -0.0084017515f), new(12f, -0.05913222f), new(24f, -0.22825828f)), []),
			new(52, new(new(12f, 1.8969669f), new(12f, -0.08978069f), new(12f, -0.19518518f), new(12f, -0.06441128f), new(12f, -0.09847784f), new(12f, -0.12766683f), new(12f, -0.05913222f), new(24f, -0.22825828f)), []),
			new(65, new(new(12f, 1.6738873f), new(12f, 0.1843797f), new(12f, 0.08090389f), new(12f, -0.13952458f), new(12f, -0.07373607f), new(12f, -0.03576207f), new(12f, -0.22384924f), new(24f, -0.059705675f)), [])
		};
		events = [];
		MinimumLength = 66f;
	}
}
public class VV13_Holding : VV13Animation {
	public VV13_Holding() {
		tentacleFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(40f, 0f)) {
			new(10, new(new(12f, 0.7912359f), new(12f, 0.4286418f), new(12f, 0.275985f), new(12f, 0.23773241f), new(12f, 0.18323064f), new(12f, 0.3792218f), new(12f, 0.19664812f), new(24f, 0.3173499f)), [])
		};
		tentacleMidFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(10f, -2f)) {
			new(10, new(new(12f, 0.30794883f), new(12f, 0.4286418f), new(12f, 0.4878528f), new(12f, 0.49038506f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), [])
		};
		tentacleMidBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(-18f, -4f)) {
			new(10, new(new(12f, 2.1406956f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.20927048f), new(12f, -0.46808028f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [])
		};
		tentacleBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(-48f, -8f)) {
			new(10, new(new(12f, 2.1481814f), new(12f, 6.0126324f), new(12f, -0.12210536f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), [])
		};
		tool = new(new()) {
			new(1, new(new(4.950001f, 99.45f), 0f, 1f), []),
			new(10, new(new(-10.5f, 79.5f), 0f, 1f), [])
		};
		toolOpacity = new(1f) {
		};
		events = [
					new(10, Dev_CycleTool)
				];
	}
}
public class VV13_Drop : VV13Animation {
	public VV13_Drop() {
		tentacleFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(40f, 0f)) {
			new(7, new(new(12f, 0.7912359f), new(12f, 0.4286418f), new(12f, 0.275985f), new(12f, 0.23773241f), new(12f, 0.18323064f), new(12f, 0.3792218f), new(12f, 0.19664812f), new(24f, 0.3173499f)), []),
			new(13, new(new(12f, 0.5916487f), new(12f, 0.30539238f), new(12f, 0.18050456f), new(12f, 0.31285703f), new(12f, 0.12805295f), new(12f, 0.26886046f), new(12f, 0.19664812f), new(24f, 0.3173499f)), [new Exponential(2f)]),
			new(19, new(new(12f, 1.2405173f), new(12f, 0.33936298f), new(12f, -0.19077718f), new(12f, 0.022064716f), new(12f, -0.15902036f), new(12f, -0.1429162f), new(12f, -0.2447167f), new(24f, -0.01968513f)), []),
			new(36, new(new(12f, 0.7912359f), new(12f, 0.4286418f), new(12f, 0.275985f), new(12f, 0.23773241f), new(12f, 0.18323064f), new(12f, 0.3792218f), new(12f, 0.19664812f), new(24f, 0.3173499f)), [new SinEaseBoth()]),
			new(59, new(new(12f, 1.6227281f), new(12f, -0.013721585f), new(12f, -0.038817883f), new(12f, 0.06344569f), new(12f, -0.108141184f), new(12f, 0.10885894f), new(12f, -0.09742844f), new(24f, 0.045223236f)), [new Exponential(2f)])
		};
		tentacleMidFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(10f, -2f)) {
			new(7, new(new(12f, 0.30794883f), new(12f, 0.4286418f), new(12f, 0.4878528f), new(12f, 0.49038506f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), []),
			new(13, new(new(12f, 0.37756622f), new(12f, 0.25135672f), new(12f, 0.17390859f), new(12f, 0.32140124f), new(12f, 0.35817432f), new(12f, 0.21112418f), new(12f, 0.13168955f), new(24f, 0.23754382f)), [new Exponential(2f)]),
			new(19, new(new(12f, 1.220962f), new(12f, 0.06368241f), new(12f, 0.24257427f), new(12f, -0.03894925f), new(12f, -0.378919f), new(12f, -0.3924481f), new(12f, -0.11583215f), new(24f, -0.16935521f)), []),
			new(36, new(new(12f, 0.30794883f), new(12f, 0.4286418f), new(12f, 0.4878528f), new(12f, 0.49038506f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), [new SinEaseBoth()]),
			new(59, new(new(12f, 1.5291598f), new(12f, 0.011682391f), new(12f, 0.09385443f), new(12f, -0.0705353f), new(12f, -0.061885238f), new(12f, 0.10841155f), new(12f, -0.07062352f), new(24f, 0f)), [new Exponential(2f)])
		};
		tentacleMidBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(-18f, -4f)) {
			new(7, new(new(12f, 2.1406956f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.20927048f), new(12f, -0.46808028f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), []),
			new(13, new(new(12f, 2.399209f), new(12f, -0.1806202f), new(12f, -0.14826512f), new(12f, -0.20927048f), new(12f, -0.3562163f), new(12f, -0.22707379f), new(12f, -0.20760036f), new(24f, 0f)), [new Exponential(2f)]),
			new(19, new(new(12f, 1.5285354f), new(12f, 0.18943655f), new(12f, 0.053178668f), new(12f, -6.086339f), new(12f, -6.094507f), new(12f, -6.230354f), new(12f, -6.242133f), new(24f, -6.015459f)), []),
			new(36, new(new(12f, 2.1406956f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.20927048f), new(12f, -0.46808028f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [new SinEaseIn()]),
			new(59, new(new(12f, 1.5685867f), new(12f, 0.013163924f), new(12f, -0.10502243f), new(12f, 0.0880034f), new(12f, -0.04618609f), new(12f, 0.055148482f), new(12f, -0.050602436f), new(24f, 0f)), [new Exponential(2f)])
		};
		tentacleBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(-48f, -8f)) {
			new(7, new(new(12f, 2.1481814f), new(12f, 6.0126324f), new(12f, -0.12210536f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), []),
			new(13, new(new(12f, 2.4313736f), new(12f, 6.25115f), new(12f, -0.21044767f), new(12f, -0.3430791f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), [new Exponential(2f)]),
			new(19, new(new(12f, 1.6073337f), new(12f, 6.0768595f), new(12f, 0.16546845f), new(12f, 0.28798866f), new(12f, -0.037413836f), new(12f, 0.17829704f), new(12f, -5.9599066f), new(24f, 0.22610116f)), []),
			new(36, new(new(12f, 2.1481814f), new(12f, 6.0126324f), new(12f, -0.12210536f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), [new SinEaseIn()]),
			new(59, new(new(12f, 1.5620587f), new(12f, 6.3262477f), new(12f, -0.057006717f), new(12f, -0.03203237f), new(12f, 0.044486284f), new(12f, 0.004768133f), new(12f, -0.036703467f), new(24f, 0.03426349f)), [new Exponential(2f)])
		};
		tool = new(new()) {
			new(7, new(new(4.5f, 78.75f), 0f, 1f), Stepped)
		};
		toolOpacity = new(1f) {
			new(7, 0.9916667f, []),
			new(59, 0f, [])
		};
		events = [
					new(7, Dev_CycleTool),
			new(10, Drop)
				];
	}
}
public class VV13_Use_Tool : VV13Animation {
	public VV13_Use_Tool() {
		tentacleFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(40f, 0f)) {
			new(3, new(new(12f, 0.7912359f), new(12f, 0.4286418f), new(12f, 0.275985f), new(12f, 0.23773241f), new(12f, 0.18323064f), new(12f, 0.3792218f), new(12f, 0.19664812f), new(24f, 0.3173499f)), []),
			new(14, new(new(12f, 0.5871603f), new(12f, 0.39242095f), new(12f, 0.1754865f), new(12f, 0.23773241f), new(12f, 0.18323064f), new(12f, 0.3792218f), new(12f, 0.19664812f), new(24f, 0.3173499f)), []),
			new(22, new(new(12f, 0.6144761f), new(12f, 0.3911534f), new(12f, 0.11738706f), new(12f, 0.23773241f), new(12f, 0.08912492f), new(12f, 0.43082714f), new(12f, 0.17347062f), new(24f, 0.3173499f)), [new Exponential(2f)]),
			new(24, new(new(12f, 0.5871603f), new(12f, 0.55561954f), new(12f, 0.1835351f), new(12f, 0.23773241f), new(12f, 0.08912492f), new(12f, 0.43082714f), new(12f, 0.17347062f), new(24f, 0.3173499f)), [new Exponential(2f)]),
			new(30, new(new(12f, 0.6144761f), new(12f, 0.3911534f), new(12f, 0.11738706f), new(12f, 0.23773241f), new(12f, 0.08912492f), new(12f, 0.43082714f), new(12f, 0.17347062f), new(24f, 0.3173499f)), [new Exponential(2f)]),
			new(34, new(new(12f, 0.6144761f), new(12f, 0.3911534f), new(12f, 0.11738706f), new(12f, 0.23773241f), new(12f, 0.08912492f), new(12f, 0.43082714f), new(12f, 0.17347062f), new(24f, 0.3173499f)), [new Exponential(2f)]),
			new(36, new(new(12f, 0.5871603f), new(12f, 0.6435645f), new(12f, 0.019587755f), new(12f, 0.12641954f), new(12f, 0.21015894f), new(12f, 0.29367328f), new(12f, 0.17347062f), new(24f, 0.3173499f)), [new Exponential(2f)]),
			new(43, new(new(12f, 0.5871603f), new(12f, 0.55561954f), new(12f, 0.1835351f), new(12f, 0.23773241f), new(12f, 0.08912492f), new(12f, 0.43082714f), new(12f, 0.17347062f), new(24f, 0.3173499f)), [new Exponential(2f)]),
			new(48, new(new(12f, 0.6144761f), new(12f, 0.31161153f), new(12f, 0.33250868f), new(12f, 0.13775766f), new(12f, 0.09110165f), new(12f, 0.389529f), new(12f, 0.17347062f), new(24f, 0.3173499f)), [new Exponential(2f)]),
			new(55, new(new(12f, 0.6144761f), new(12f, 0.5178317f), new(12f, 0.11738706f), new(12f, 0.23773241f), new(12f, 0.08912492f), new(12f, 0.43082714f), new(12f, 0.17347062f), new(24f, 0.3173499f)), [new Exponential(2f)]),
			new(60, new(new(12f, 0.6144761f), new(12f, 0.3911534f), new(12f, 0.11738706f), new(12f, 0.23773241f), new(12f, 0.08912492f), new(12f, 0.43082714f), new(12f, 0.17347062f), new(24f, 0.3173499f)), [new Exponential(2f)]),
			new(64, new(new(12f, 0.5871603f), new(12f, 0.55561954f), new(12f, 0.1835351f), new(12f, 0.23773241f), new(12f, 0.08912492f), new(12f, 0.43082714f), new(12f, 0.17347062f), new(24f, 0.3173499f)), [new Exponential(2f)]),
			new(69, new(new(12f, 0.6144761f), new(12f, 0.3911534f), new(12f, 0.11738706f), new(12f, 0.23773241f), new(12f, 0.08912492f), new(12f, 0.43082714f), new(12f, 0.17347062f), new(24f, 0.3173499f)), [new Exponential(2f)]),
			new(75, new(new(12f, 0.5871603f), new(12f, 0.39242095f), new(12f, 0.1754865f), new(12f, 0.23773241f), new(12f, 0.18323064f), new(12f, 0.3792218f), new(12f, 0.19664812f), new(24f, 0.3173499f)), []),
			new(85, new(new(12f, 0.7912359f), new(12f, 0.4286418f), new(12f, 0.275985f), new(12f, 0.23773241f), new(12f, 0.18323064f), new(12f, 0.3792218f), new(12f, 0.19664812f), new(24f, 0.3173499f)), [])
		};
		tentacleMidFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(10f, -2f)) {
			new(3, new(new(12f, 0.30794883f), new(12f, 0.4286418f), new(12f, 0.4878528f), new(12f, 0.49038506f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), []),
			new(14, new(new(12f, 0.30794883f), new(12f, 0.19047421f), new(12f, 0.4220088f), new(12f, 0.34360647f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), []),
			new(22, new(new(12f, 0.30794883f), new(12f, 0.26833296f), new(12f, 0.37028813f), new(12f, 0.34958053f), new(12f, 0.3383193f), new(12f, 0.0890286f), new(12f, 0f), new(24f, 0f)), [new Exponential(2f)]),
			new(24, new(new(12f, 0.38169897f), new(12f, 0.2828483f), new(12f, 0.4220088f), new(12f, 0.35594785f), new(12f, 0.24594212f), new(12f, 0.24232852f), new(12f, -0.09355855f), new(24f, 0.15989864f)), [new Exponential(2f)]),
			new(30, new(new(12f, 0.23602024f), new(12f, 0.2212297f), new(12f, 0.52080625f), new(12f, 0.23253429f), new(12f, 0.4836527f), new(12f, 0.09250188f), new(12f, 0f), new(24f, 0f)), [new Exponential(2f)]),
			new(34, new(new(12f, 0.17532724f), new(12f, 0.3082058f), new(12f, 0.49593705f), new(12f, 0.4060377f), new(12f, 0.14973521f), new(12f, 0.3193711f), new(12f, 0.21650255f), new(24f, 0f)), [new Exponential(2f)]),
			new(36, new(new(12f, 0.30794883f), new(12f, 0.19047421f), new(12f, 0.4220088f), new(12f, 0.34360647f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), [new Exponential(2f)]),
			new(43, new(new(12f, 0.30794883f), new(12f, 0.19047421f), new(12f, 0.53753525f), new(12f, 0.34360647f), new(12f, 0.3383193f), new(12f, 0.27141345f), new(12f, 0.05239439f), new(24f, 0f)), [new Exponential(2f)]),
			new(48, new(new(12f, 0.40765545f), new(12f, 0.19047421f), new(12f, 0.4220088f), new(12f, 0.34360647f), new(12f, 0.16207826f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), [new Exponential(2f)]),
			new(55, new(new(12f, 0.5398751f), new(12f, 0.15243012f), new(12f, 0.4220088f), new(12f, 0.34360647f), new(12f, 0.22965086f), new(12f, 0.27545965f), new(12f, 0f), new(24f, 0f)), [new Exponential(2f)]),
			new(60, new(new(12f, 0.30794883f), new(12f, 0.19047421f), new(12f, 0.4220088f), new(12f, 0.34360647f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), [new Exponential(2f)]),
			new(64, new(new(12f, 0.30794883f), new(12f, 0.37559247f), new(12f, 0.4220088f), new(12f, 0.34360647f), new(12f, 0.3383193f), new(12f, 0.13905776f), new(12f, 0f), new(24f, 0f)), [new Exponential(2f)]),
			new(69, new(new(12f, 0.30794883f), new(12f, 0.26124012f), new(12f, 0.4220088f), new(12f, 0.34360647f), new(12f, 0.22330737f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), [new Exponential(2f)]),
			new(75, new(new(12f, 0.30794883f), new(12f, 0.19047421f), new(12f, 0.4220088f), new(12f, 0.34360647f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), []),
			new(85, new(new(12f, 0.30794883f), new(12f, 0.4286418f), new(12f, 0.4878528f), new(12f, 0.49038506f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), [])
		};
		tentacleMidBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(-18f, -4f)) {
			new(3, new(new(12f, 2.1406956f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.20927048f), new(12f, -0.46808028f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), []),
			new(14, new(new(12f, 2.0380852f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.15500224f), new(12f, -0.3890586f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), []),
			new(22, new(new(12f, 2.0380852f), new(12f, -0.17001462f), new(12f, -0.14826512f), new(12f, -0.15500224f), new(12f, -0.3890586f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [new Exponential(2f)]),
			new(24, new(new(12f, 2.086883f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.15500224f), new(12f, -0.3890586f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [new Exponential(2f)]),
			new(30, new(new(12f, 1.9146014f), new(12f, 0.073403835f), new(12f, -0.10795784f), new(12f, -0.15500224f), new(12f, -0.3890586f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [new Exponential(2f)]),
			new(34, new(new(12f, 2.113903f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.15500224f), new(12f, -0.3890586f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [new Exponential(2f)]),
			new(36, new(new(12f, 1.9440367f), new(12f, -0.19132245f), new(12f, 0.05039215f), new(12f, -0.022704482f), new(12f, -0.3890586f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [new Exponential(2f)]),
			new(43, new(new(12f, 2.1905236f), new(12f, -0.0040709972f), new(12f, -0.14826512f), new(12f, -0.15500224f), new(12f, -0.3890586f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [new Exponential(2f)]),
			new(48, new(new(12f, 1.8399204f), new(12f, -0.053531766f), new(12f, -0.14826512f), new(12f, -0.021516204f), new(12f, -0.3890586f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [new Exponential(2f)]),
			new(55, new(new(12f, 2.2554238f), new(12f, -0.23461759f), new(12f, -0.14826512f), new(12f, -0.15500224f), new(12f, -0.3890586f), new(12f, -0.31638932f), new(12f, -0.20527661f), new(24f, 0f)), [new Exponential(2f)]),
			new(60, new(new(12f, 1.8635529f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.059476376f), new(12f, -0.1928742f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [new Exponential(2f)]),
			new(64, new(new(12f, 2.1824846f), new(12f, -0.07647014f), new(12f, -0.20247364f), new(12f, -0.15500224f), new(12f, -0.3890586f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [new Exponential(2f)]),
			new(69, new(new(12f, 2.0380852f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.15500224f), new(12f, -0.3890586f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [new Exponential(2f)]),
			new(75, new(new(12f, 2.0380852f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.15500224f), new(12f, -0.3890586f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), []),
			new(85, new(new(12f, 2.1406956f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.20927048f), new(12f, -0.46808028f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [])
		};
		tentacleBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(-48f, -8f)) {
			new(3, new(new(12f, 2.1481814f), new(12f, 6.0126324f), new(12f, -0.12210536f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), []),
			new(14, new(new(12f, 1.8807759f), new(12f, 6.3123713f), new(12f, -0.025649786f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), []),
			new(22, new(new(12f, 1.8080842f), new(12f, 6.269969f), new(12f, -0.025649786f), new(12f, 0.07095587f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), [new Exponential(2f)]),
			new(24, new(new(12f, 1.8807759f), new(12f, 6.3123713f), new(12f, -0.025649786f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), [new Exponential(2f)]),
			new(30, new(new(12f, 1.935117f), new(12f, 6.1668153f), new(12f, -0.025649786f), new(12f, -0.04045129f), new(12f, -0.3729931f), new(12f, -0.21191466f), new(12f, -0.3533004f), new(24f, 0f)), [new Exponential(2f)]),
			new(34, new(new(12f, 1.8807759f), new(12f, 6.3123713f), new(12f, 0.12461209f), new(12f, -0.04045129f), new(12f, -0.6080668f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, -0.02006954f)), [new Exponential(2f)]),
			new(36, new(new(12f, 1.8807759f), new(12f, 6.2445655f), new(12f, -0.025649786f), new(12f, -0.060984254f), new(12f, -0.44736588f), new(12f, -0.0018289089f), new(12f, -0.36739022f), new(24f, 0f)), [new Exponential(2f)]),
			new(43, new(new(12f, 1.9356172f), new(12f, 6.3895144f), new(12f, -0.025649786f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), [new Exponential(2f)]),
			new(48, new(new(12f, 1.7211057f), new(12f, 6.2548985f), new(12f, -0.025649786f), new(12f, -0.04045129f), new(12f, -0.2503842f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), [new Exponential(2f)]),
			new(55, new(new(12f, 1.8807759f), new(12f, 6.361508f), new(12f, -0.059361458f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.39565343f), new(24f, 0f)), [new Exponential(2f)]),
			new(60, new(new(12f, 1.7833673f), new(12f, 6.157698f), new(12f, -0.025649786f), new(12f, -0.04045129f), new(12f, -0.26642f), new(12f, -0.2471441f), new(12f, -0.35526103f), new(24f, 0f)), [new Exponential(2f)]),
			new(69, new(new(12f, 1.7870811f), new(12f, 6.2943196f), new(12f, -0.025649786f), new(12f, -0.04045129f), new(12f, -0.32674658f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), [new Exponential(2f)]),
			new(75, new(new(12f, 1.8807759f), new(12f, 6.3123713f), new(12f, -0.025649786f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), []),
			new(85, new(new(12f, 2.1481814f), new(12f, 6.0126324f), new(12f, -0.12210536f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), [])
		};
		tool = new(new()) {
		};
		toolOpacity = new(1f) {
		};
		events = [
					new(-0, Dev_CycleTool)
				];
	}
}
public class VV13_Swap_Tool : VV13Animation {
	public KeyframeSet<PosRotScale> swappingInTool = new(new()) {
	};
	public KeyframeSet<Percentage> swappingInToolOpacity = new(new()) {
	};
	public VV13_Swap_Tool() {
		swappingInTool = new(new()) {
			new(22, new(new(-2.25f, -12.75f), 3.1352565f, 1f), Stepped),
			new(55, new(new(12f, -38.25f), 3.1104045f, 1f), []),
			new(77, new(new(76.5f, -43.5f), 3.0947933f, 1f), []),
			new(85, new(new(108.75f, -15.75f), 4.4197235f, 1f), []),
			new(91, new(new(109.5f, 3.75f), -1.0832675f, 1f), []),
			new(95, new(new(115.704544f, 19.09091f), -0.8582682f, 1f), []),
			new(102, new(new(99.75f, 56.25f), -0.4645195f, 1f), []),
			new(107, new(new(88.36363f, 70.5f), -0.39913133f, 1f), []),
			new(111, new(new(72.91042f, 82.411766f), -0.3468208f, 1f), []),
			new(114, new(new(62.89263f, 89.98982f), -0.3075879f, 1f), []),
			new(123, new(new(19.114265f, 88.57398f), -0.059376836f, 1f), []),
			new(124, new(new(13.5f, 85.5f), 0.024232745f, 1f), []),
			new(128, new(new(-4.5f, 78f), 0.024232745f, 1f), [])
		};
		swappingInToolOpacity = new(1f) {
			new(1, 0f, []),
			new(9, 0f, []),
			new(22, 1f, [])
		};
		tentacleFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(40f, 0f)) {
			new(2, new(new(12f, 0.7912359f), new(12f, 0.4286418f), new(12f, 0.275985f), new(12f, 0.23773241f), new(12f, 0.18323064f), new(12f, 0.3792218f), new(12f, 0.19664812f), new(24f, 0.3173499f)), []),
			new(9, new(new(12f, 0.82080907f), new(12f, -0.0018869638f), new(12f, 0.42179155f), new(12f, 0.4689386f), new(12f, 0.4797392f), new(12f, 0.35941124f), new(12f, 0f), new(24f, 0.08558452f)), Stepped),
			new(22, new(new(12f, 0.8515515f), new(12f, 0.1871692f), new(12f, 0.36366564f), new(12f, 0.2803712f), new(12f, 0.4177183f), new(12f, 0.54024935f), new(12f, 0.23042893f), new(24f, 0.009509391f)), []),
			new(36, new(new(12f, 0.39093757f), new(12f, 0.1871692f), new(12f, 0.36366564f), new(12f, 0.2803712f), new(12f, 0.4177183f), new(12f, 0.54024935f), new(12f, 0.23042893f), new(24f, 0.009509391f)), []),
			new(85, new(new(12f, 0.39093757f), new(12f, 0.1871692f), new(12f, 0.36366564f), new(12f, 0.2803712f), new(12f, 0.4177183f), new(12f, 0.54024935f), new(12f, 0.23042893f), new(24f, 0.009509391f)), []),
			new(108, new(new(12f, 0.63171303f), new(12f, 0.033544004f), new(12f, 0.36366564f), new(12f, 0.42471206f), new(12f, 0.6106839f), new(12f, 0.6335561f), new(12f, 0.29666924f), new(24f, -0.37179375f)), []),
			new(129, new(new(12f, 0.8515515f), new(12f, 0.1871692f), new(12f, 0.36366564f), new(12f, 0.2803712f), new(12f, 0.4177183f), new(12f, 0.54024935f), new(12f, 0.23042893f), new(24f, 0.009509391f)), [])
		};
		tentacleMidFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(10f, -2f)) {
			new(2, new(new(12f, 0.30794883f), new(12f, 0.4286418f), new(12f, 0.4878528f), new(12f, 0.49038506f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), []),
			new(9, new(new(12f, 0.3120112f), new(12f, 0.4286418f), new(12f, 0.38291037f), new(12f, 0.16813838f), new(12f, 0.40710986f), new(12f, 0.50622535f), new(12f, 0.18329668f), new(24f, 0f)), Stepped),
			new(22, new(new(12f, 0.30794883f), new(12f, 0.4286418f), new(12f, 0.4878528f), new(12f, 0.49038506f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), []),
			new(36, new(new(12f, -0.07709736f), new(12f, 0.4286418f), new(12f, 0.4878528f), new(12f, 0.49038506f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), []),
			new(85, new(new(12f, -0.07709736f), new(12f, 0.4286418f), new(12f, 0.4878528f), new(12f, 0.49038506f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), []),
			new(108, new(new(12f, 0.12417677f), new(12f, 0.4286418f), new(12f, 0.3470533f), new(12f, 0.20195317f), new(12f, 0.76739657f), new(12f, 0.36385226f), new(12f, 0.10016346f), new(24f, 0.23026443f)), [new Exponential(2f)]),
			new(129, new(new(12f, 0.30794883f), new(12f, 0.4286418f), new(12f, 0.4878528f), new(12f, 0.49038506f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), [])
		};
		tentacleMidBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(-18f, -4f)) {
			new(2, new(new(12f, 2.1406956f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.20927048f), new(12f, -0.46808028f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), []),
			new(9, new(new(12f, 2.3833063f), new(12f, -0.07936001f), new(12f, -0.09410739f), new(12f, -0.30388677f), new(12f, -0.62854266f), new(12f, -0.49267775f), new(12f, -0.30729944f), new(24f, 0f)), Stepped),
			new(22, new(new(12f, 2.1406956f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.20927048f), new(12f, -0.46808028f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), []),
			new(51, new(new(12f, 2.7775743f), new(12f, 0.20113587f), new(12f, -6.156761f), new(12f, 0.20541453f), new(12f, 0.24441433f), new(12f, 0.78343654f), new(12f, -5.947764f), new(24f, 0.3213408f)), [new SinEaseBoth(), new Exponential(2f)]),
			new(59, new(new(12f, 2.7775743f), new(12f, 0.20113587f), new(12f, -6.156761f), new(12f, 0.20541453f), new(12f, 0.24441433f), new(12f, 0.78343654f), new(12f, -5.947764f), new(24f, 0.3213408f)), [new SinEaseBoth(), new Exponential(2f)]),
			new(73, new(new(12f, 2.7775743f), new(12f, 0.20113587f), new(12f, -6.156761f), new(12f, 0.20541453f), new(12f, 0.24441433f), new(12f, 0.78343654f), new(12f, -5.947764f), new(24f, 0.3213408f)), [new SinEaseBoth(), new Exponential(2f)]),
			new(77, new(new(12f, 2.7775743f), new(12f, -5.950261f), new(12f, 0.21293259f), new(12f, -0.006100893f), new(12f, 0.24441433f), new(12f, 0.26550746f), new(12f, -5.947764f), new(24f, 0.3213408f)), []),
			new(90, new(new(12f, 2.7775743f), new(12f, -0.07647014f), new(12f, 0.022016525f), new(12f, -0.006100893f), new(12f, 0.24441433f), new(12f, 0.26550746f), new(12f, -5.947764f), new(24f, 0.3213408f)), []),
			new(129, new(new(12f, 2.1406956f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.20927048f), new(12f, -0.46808028f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), [])
		};
		tentacleBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(-48f, -8f)) {
			new(2, new(new(12f, 2.1481814f), new(12f, 6.0126324f), new(12f, -0.12210536f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), []),
			new(9, new(new(12f, 2.2626631f), new(12f, 6.2222986f), new(12f, -0.32021964f), new(12f, -0.27815878f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), Stepped),
			new(22, new(new(12f, 2.1481814f), new(12f, 6.0126324f), new(12f, -0.12210536f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), []),
			new(51, new(new(12f, 2.9140017f), new(12f, 0.8897743f), new(12f, 0.3566146f), new(12f, 0.5108174f), new(12f, 0.16285932f), new(12f, 0.2151249f), new(12f, 0.5177735f), new(24f, 0.3944631f)), []),
			new(55, new(new(12f, 2.9140017f), new(12f, 0.8897743f), new(12f, 0.46861196f), new(12f, 0.86791015f), new(12f, 0.29351646f), new(12f, 0.2151249f), new(12f, 0.5177735f), new(24f, 0.3944631f)), [new Exponential(2f)]),
			new(73, new(new(12f, 2.9140017f), new(12f, 0.8897743f), new(12f, 0.46861196f), new(12f, 0.86791015f), new(12f, 0.29351646f), new(12f, 0.2151249f), new(12f, 0.5177735f), new(24f, 0.3944631f)), [new Exponential(2f)]),
			new(77, new(new(12f, 2.9140017f), new(12f, 0.8897743f), new(12f, 0.3566146f), new(12f, 0.5108174f), new(12f, 0.16285932f), new(12f, 0.2151249f), new(12f, 0.5177735f), new(24f, 0.3944631f)), [new Exponential(2f)]),
			new(100, new(new(12f, 2.5752735f), new(12f, -0.037510157f), new(12f, 0.14487308f), new(12f, 0.26698703f), new(12f, -0.107047975f), new(12f, 0.026244506f), new(12f, 0.055461347f), new(24f, 0.21998903f)), []),
			new(129, new(new(12f, 2.1481814f), new(12f, 6.0126324f), new(12f, -0.12210536f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), [])
		};
		tool = new(new()) {
			new(2, new(new(-9.75f, 74.25f), 0f, 1f), []),
			new(9, new(new(-11.25f, 69f), 0f, 1f), Stepped),
			new(22, new(new(-12f, 77.25f), 0f, 1f), []),
			new(34, new(new(23.884615f, 89.71154f), -0.21782172f, 1f), []),
			new(35, new(new(31.5f, 89.25f), -0.108270645f, 1f), []),
			new(37, new(new(55.35f, 85.95f), -0.44343635f, 1f), []),
			new(38, new(new(67.368744f, 83.081245f), -0.61101925f, 1f), []),
			new(39, new(new(80.8875f, 75.712494f), -0.7786021f, 1f), []),
			new(40, new(new(93.90625f, 64.84374f), -0.946185f, 1f), []),
			new(41, new(new(103.775f, 49.174995f), -1.1137677f, 1f), []),
			new(42, new(new(111.95625f, 33.88125f), -1.2813506f, 1f), []),
			new(45, new(new(109.5f, -21f), 4.4990864f, 1f), []),
			new(51, new(new(76.5f, -43.5f), 3.0947933f, 1f), []),
			new(53, new(new(10.596153f, -32.942307f), 3.0159793f, 1f), []),
			new(93, new(new(-2.6781387f, -9.485828f), 3.1173968f, 1f), [])
		};
		toolOpacity = new(1f) {
			new(53, 1f, []),
			new(93, 0f, [])
		};
		events = [
			new(129, SwitchTool)
		];
	}
	static void SwitchTool(in VV13 vv13) => Utils.Swap(ref vv13.CurrentToolIndex, ref vv13.AnimationVar);
	static void Dev_GuaranteeTools(in VV13 vv13) {
		vv13.CurrentTool = ModContent.GetInstance<VV13_Drill>();
		vv13.AnimationVar = ModContent.GetInstance<VV13_Saw>().Type;
	}
	public override void DrawExtras(SpriteBatch spriteBatch, VV13 vv13, Transform root, Color drawColor) {
		VV13Tool.tools[(int)vv13.AnimationVar].Draw(
			spriteBatch,
			vv13,
			root + swappingInTool.GetCurrentValue(vv13.AnimationTime),
			drawColor,
			swappingInToolOpacity.GetCurrentValue(vv13.AnimationTime)
		);
	}
}
#endregion animations
#region tools
public abstract class VV13Tool : ModTexturedType {
	public static List<VV13Tool> tools = [];
	public virtual VV13Animation HoldAnimation => GetInstance<VV13_Holding>();
	public abstract VV13Animation UseAnimation { get; }
	public abstract Vector2 Origin { get; }
	public abstract int FrameCount { get; }
	public abstract int DroppedSize { get; }
	public virtual int DroppedHealth => 200;
	public virtual int IncineratorPowerTime => 900;
	public int Type { get; private set; }
	protected static RangeRandom randomWeights;
	protected sealed override void Register() {
		ModTypeLookup<VV13Tool>.Register(this);
		Type = tools.Count;
		tools.Add(this);
	}
	public virtual void AutoStaticDefaults() {
		Sets.Texture[Type] = Request<Texture2D>(Texture);
		RequestIfExists(Texture + "_Glow", out Sets.GlowTexture[Type]);
	}
	public sealed override void SetupContent() {
		randomWeights ??= new(Main.rand, 0, tools.Count);
		AutoStaticDefaults();
		SetStaticDefaults();
	}
	public virtual void AI(VV13 vv13) {
		NPC npc = vv13.NPC;
		if (npc.HasValidTarget && npc.HasPlayerTarget) {
			NPCAimedTarget target = npc.GetTargetData();
			const float hover_range = 16 * 13;
			vv13.Approach(target.Center, hover_range - 32, hover_range + 32);
		}
	}
	public virtual void Draw(SpriteBatch spriteBatch, VV13 vv13, Transform transform, Color drawColor, float opacity) {
		Rectangle frame = Sets.Texture[Type].Frame(verticalFrames: FrameCount);
		SpriteEffects spriteEffects = vv13?.NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		DrawData data = new(
			Sets.Texture[Type].Value,
			transform.Position,
			frame,
			drawColor,
			transform.Rotation,
			Origin.Apply(spriteEffects, frame.Size()),
			transform.Scale,
			spriteEffects
		);
		data.Draw(spriteBatch);
		if (Sets.GlowTexture[Type] is not null) {
			using ScopedOverride<bool> o = vv13 is null ? default : vv13.NPC.oiled.ScopedOverride(false);
			data.texture = Sets.GlowTexture[Type].Value;
			data.color = vv13.NPC.GetTintColor(Color.White);
			data.Draw(spriteBatch);
		}
	}
	public static VV13Tool GetRandom() => tools[randomWeights.Get()];
	public static class Sets {
		public static SetFactory Factory { get; } = new(tools.Count, nameof(ToolID), ToolID.Search);
		public static Asset<Texture2D>[] Texture = Factory.CreateCustomSet(Asset<Texture2D>.Empty);
		public static Asset<Texture2D>[] GlowTexture = Factory.CreateCustomSet<Asset<Texture2D>>(null);
	}
	private class ToolID {
		/// <inheritdoc cref="IdDictionary"/>
		public static readonly IdDictionary Search = IdDictionary.Create<ToolID, int>();
	}
}
public class VV13_No_Tool : VV13Tool {
	public override string Texture => "Terraria/Images/NPC_0";
	public override VV13Animation HoldAnimation => GetInstance<VV13_Idle>();
	public override VV13Animation UseAnimation => GetInstance<VV13_Idle>();
	public override Vector2 Origin => default;
	public override int FrameCount => default;
	public override int DroppedSize => default;
	public override void SetStaticDefaults() => randomWeights.Multiply(Type, Type + 1, 0);
	public override void Draw(SpriteBatch spriteBatch, VV13 vv13, Transform transform, Color drawColor, float opacity) { }
}
public class VV13_Drill : VV13Tool {
	public override VV13Animation UseAnimation => GetInstance<VV13_Use_Tool>();
	public override int FrameCount => 3;
	public override Vector2 Origin => new(80, 8);
	public override int DroppedSize => 38;
	public override void AI(VV13 vv13) {
		NPC npc = vv13.NPC;
		if (npc.HasValidTarget && npc.HasPlayerTarget) {
			NPCAimedTarget target = npc.GetTargetData();
			const float hover_range = 16 * 13;
			vv13.Approach(target.Center, hover_range - 32, hover_range + 32);
		}
	}
}
public class VV13_Saw : VV13Tool {
	public override VV13Animation UseAnimation => GetInstance<VV13_Use_Tool>();
	public override int FrameCount => 5;
	public override Vector2 Origin => new(72, 10);
	public override int DroppedSize => 48;
}
public class VV13_Puncher : VV13Tool {
	public override VV13Animation UseAnimation => GetInstance<VV13_Use_Tool>();
	public override int FrameCount => 5;
	public override Vector2 Origin => new(56, 40);
	public override int DroppedSize => 30;
}
public class VV13_Torch : VV13Tool {
	public override VV13Animation UseAnimation => GetInstance<VV13_Use_Tool>();
	public override int FrameCount => 1;
	public override Vector2 Origin => new(10, 18);
	public override int DroppedSize => 24;
}
[Autoload(false)]
public class VV13_Scrap(HangingScrap scrap) : VV13Tool {
	public override string Texture => scrap.Texture;
	public override string Name => "VV13_" + scrap.Name;
	public override VV13Animation UseAnimation => GetInstance<VV13_Drop>();
	public override Vector2 Origin => scrap.Texture2D.Size() * 0.5f;
	public override int FrameCount => 1;
	public override int DroppedSize => scrap.Size;
	public override int DroppedHealth => scrap.ScrapValue.Max * 40;
	public override int IncineratorPowerTime => scrap.ScrapValue.GetValue() * 180;
	public override void SetStaticDefaults() => randomWeights.Multiply(Type, Type + 1, 1f / GetContent<VV13_Scrap>().Count());
}
#endregion tool
public class VV13_Dropped_Tool : ModNPC, IIncineratorPitValue {
	public override string Texture => "Terraria/Images/NPC_0";
	int IncineratorPowerTime { get; set; }
	int IIncineratorPitValue.PowerTime => IncineratorPowerTime;
	public VV13Tool Tool {
		get => VV13Tool.tools[(int)NPC.ai[0]];
		set {
			NPC.ai[0] = value.Type;
			IncineratorPowerTime = value.IncineratorPowerTime;
			Vector2 center = NPC.Center;
			NPC.width = NPC.height = value.DroppedSize;
			NPC.Center = center;
			NPC.life = NPC.lifeMax = value.DroppedHealth;
		}
	}
	public override void SetStaticDefaults() {
		NPC.aiStyle = NPCAIStyleID.ActuallyNone;
		NPC.lifeMax = 475;
		NPC.defense = 24;
		NPC.damage = 48;
		NPC.width = 0;
		NPC.height = 0;
		NPC.value = 0;
		NPC.knockBackResist = 0.5f;
		NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
		NPC.DeathSound = SoundID.NPCDeath44;
	}
	public override void OnSpawn(IEntitySource source) => Tool = Tool;
	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		drawColor = NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(drawColor));
		Texture2D texture = VV13Tool.Sets.Texture[Tool.Type].Value;
		Rectangle frame = texture.Frame(verticalFrames: Tool.FrameCount);
		spriteBatch.Draw(
			texture,
			NPC.Center - screenPos,
			frame,
			drawColor,
			NPC.ai[1],
			frame.Size() * 0.5f,
			1,
			SpriteEffects.None,
		0);
		return false;
	}
}