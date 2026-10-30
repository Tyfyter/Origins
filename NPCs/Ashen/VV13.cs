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
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.Ashen.VV13;
using static Terraria.ModLoader.ModContent;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen;
public class VV13 : ModNPC, IWikiNPC, IAshenEnemy, IPlatformNPC, ISpecialTargetingNPC {
	public override void OnSpawn(IEntitySource source) {
		AnimationVar = 1;
	}
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
		if (NPC.HasValidTarget && NPC.HasPlayerTarget) {
			NPCAimedTarget target = NPC.GetTargetData();
			float speed = 8f;
			float inertia = 128f;
			//NPC.rotation = NPC.velocity.X * 0.1f;
			Vector2 vectorToTargetPosition = (target.Center - NPC.Center).Normalized(out float dist);
			if (NPC.confused) vectorToTargetPosition *= -1;
			const float hover_range = 16 * 13;
			if (dist < hover_range - 32) {
				speed *= -1;
			} else if (dist < hover_range + 32) {
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
		Vector2 nextVel = Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, true, true);
		if (nextVel.X != NPC.velocity.X) NPC.velocity.X *= -0.9f;
		if (nextVel.Y != NPC.velocity.Y) NPC.velocity.Y *= -0.9f;
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
		Vector2 position = NPC.Bottom + Vector2.UnitY * 2 - screenPos;
		Vector2 dirMult = new(NPC.direction, 1);
		if (animation.HasTool) CurrentTool.Draw(spriteBatch, this, position, animation.tool.GetCurrentValue(AnimationTime), drawColor, animation.toolOpacity.GetCurrentValue(AnimationTime));
		animation.DrawExtras(spriteBatch, this, position, drawColor);
		animation.tentacleMidFront.GetCurrentValue(AnimationTime).Draw(spriteBatch, position + VV13Animation.tentaclePosMidFront * dirMult, drawColor, 0, NPC.direction);
		animation.tentacleMidBack.GetCurrentValue(AnimationTime).Draw(spriteBatch, position + VV13Animation.tentaclePosMidBack * dirMult, drawColor, 0, NPC.direction);
		animation.tentacleBack.GetCurrentValue(AnimationTime).Draw(spriteBatch, position + VV13Animation.tentaclePosBack * dirMult, drawColor, 0, NPC.direction);
		animation.tentacleFront.GetCurrentValue(AnimationTime).Draw(spriteBatch, position + VV13Animation.tentaclePosFront * dirMult, drawColor, 0, NPC.direction);
		spriteBatch.DrawGlowingNPCPart(
			TextureAssets.Npc[Type].Value,
			glowTexture,
			position,
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
		public readonly void Draw(SpriteBatch spriteBatch, Vector2 offset, Color drawColor, int xFrame, int direction) {
			Rectangle frame = texture.Frame(3, 3, xFrame);
			frame.Height = 12;
			int i = 0;
			Vector2 origin = frame.Size() * new Vector2(0.5f, 1);
			Vector2 mult = new(direction, 1);
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
	public static readonly Vector2 tentaclePosFront = new(40, 0);
	public static readonly Vector2 tentaclePosMidFront = new(10, -2);
	public static readonly Vector2 tentaclePosMidBack = new(-18, -4);
	public static readonly Vector2 tentaclePosBack = new(-48, -8);
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
	public virtual void DrawExtras(SpriteBatch spriteBatch, VV13 vv13, Vector2 position, Color drawColor) { }
}
public class VV13_Idle : VV13Animation {
	public override bool HasTool => false;
	public VV13_Idle() {
		tentacleFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), (true, []), new Vector2(40f, 0f)) {
			new(20, new(new(12f, 1.5707964f), new(12f, 0.05255139f), new(12f, -0.15629911f), new(12f, 0.20958805f), new(12f, -0.17692292f), new(12f, 0.31335962f), new(12f, -0.38604045f), new(24f, 0.29743743f)), [])
		};
		tentacleMidFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), (true, []), new Vector2(10f, -2f)) {
			new(20, new(new(12f, 1.5707964f), new(12f, 0.05255139f), new(12f, -0.15629911f), new(12f, 0.20958805f), new(12f, -0.17692292f), new(12f, 0.31335962f), new(12f, -0.38604045f), new(24f, 0.29743743f)), [])
		};
		tentacleMidBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), (true, []), new Vector2(-18f, -4f)) {
			new(20, new(new(12f, 1.5707964f), new(12f, 0.05255139f), new(12f, -0.15629911f), new(12f, 0.20958805f), new(12f, -0.17692292f), new(12f, 0.31335962f), new(12f, -0.38604045f), new(24f, 0.29743743f)), [])
		};
		tentacleBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), (true, []), new Vector2(-48f, -8f)) {
			new(20, new(new(12f, 1.5707964f), new(12f, 0.05255139f), new(12f, -0.15629911f), new(12f, 0.20958805f), new(12f, -0.17692292f), new(12f, 0.31335962f), new(12f, -0.38604045f), new(24f, 0.29743743f)), [])
		};
		tool = new(new()) {
		};
		events = [];
		MinimumLength = 40f;
	}
}
public class VV13_Holding : VV13Animation {
	public VV13_Holding() {
		//timelines here
	}
}
public class VV13_Drop : VV13Animation {
	public VV13_Drop() {
		tentacleFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(40f, 0f)) {
			new(7, new(new(12f, 0.7912359f), new(12f, 0.4286418f), new(12f, 0.275985f), new(12f, 0.23773241f), new(12f, 0.18323064f), new(12f, 0.3792218f), new(12f, 0.19664812f), new(24f, 0.3173499f)), []),
			new(19, new(new(12f, 0.88321584f), new(12f, 0.33936298f), new(12f, -0.19077718f), new(12f, 0.022064716f), new(12f, -0.15902036f), new(12f, -0.1429162f), new(12f, -0.2447167f), new(24f, -0.01968513f)), [])
		};
		tentacleMidFront = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(10f, -2f)) {
			new(7, new(new(12f, 0.30794883f), new(12f, 0.4286418f), new(12f, 0.4878528f), new(12f, 0.49038506f), new(12f, 0.3383193f), new(12f, 0.15430582f), new(12f, 0f), new(24f, 0f)), []),
			new(19, new(new(12f, 0.558076f), new(12f, 0.06368241f), new(12f, 0.24257427f), new(12f, -0.03894925f), new(12f, -0.378919f), new(12f, -0.3924481f), new(12f, -0.11583215f), new(24f, -0.16935521f)), [])
		};
		tentacleMidBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(-18f, -4f)) {
			new(7, new(new(12f, 2.1406956f), new(12f, -0.07647014f), new(12f, -0.14826512f), new(12f, -0.20927048f), new(12f, -0.46808028f), new(12f, -0.31638932f), new(12f, -0.30729944f), new(24f, 0f)), []),
			new(19, new(new(12f, 1.5285354f), new(12f, 0.63582313f), new(12f, 0.37780166f), new(12f, -6.086339f), new(12f, -6.094507f), new(12f, -6.230354f), new(12f, -6.242133f), new(24f, -6.015459f)), [])
		};
		tentacleBack = new(new(new(12f, 1.5707964f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(12f, 0f), new(24f, 0f)), new Vector2(-48f, -8f)) {
			new(7, new(new(12f, 2.1481814f), new(12f, 6.0126324f), new(12f, -0.12210536f), new(12f, -0.04045129f), new(12f, -0.44736588f), new(12f, -0.21190906f), new(12f, -0.5274541f), new(24f, 0f)), []),
			new(19, new(new(12f, 2.0584033f), new(12f, 6.0768595f), new(12f, 0.16546845f), new(12f, 0.28798866f), new(12f, -0.037413836f), new(12f, 0.17829704f), new(12f, -5.9599066f), new(24f, 0.22610116f)), [])
		};
		tool = new(new()) {
			new(7, new(new(252f, 111.75f), 0f, 1f), [])
		};
		toolOpacity = new(1f) {
			new(7, 0.9916667f, []),
			new(120, 0f, [])
		};
		events = [
					new(7, Dev_CycleTool),
			new(10, Drop)
				];
	}
}
public class VV13_Use_Tool : VV13Animation {
	public VV13_Use_Tool() {
		//timelines here
	}
}
public class VV13_Swap_Tool : VV13Animation {
	public KeyframeSet<PosRotScale> swappingInTool = new(new()) {
	};
	public KeyframeSet<Percentage> swappingInToolOpacity = new(new()) {
	};
	public VV13_Swap_Tool() {
		//timelines here
	}
	static void SwitchTool(in VV13 vv13) => Utils.Swap(ref vv13.CurrentToolIndex, ref vv13.AnimationVar);
	public override void DrawExtras(SpriteBatch spriteBatch, VV13 vv13, Vector2 position, Color drawColor) {
		VV13Tool.tools[(int)vv13.AnimationVar].Draw(
			spriteBatch,
			vv13,
			position,
			swappingInTool.GetCurrentValue(vv13.AnimationTime),
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
		AutoStaticDefaults();
		SetStaticDefaults();
	}
	public virtual void Draw(SpriteBatch spriteBatch, VV13 vv13, Vector2 position, PosRotScale transform, Color drawColor, float opacity) {
		Rectangle frame = Sets.Texture[Type].Frame(verticalFrames: FrameCount);
		SpriteEffects spriteEffects = vv13?.NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		DrawData data = new(
			Sets.Texture[Type].Value,
			position + transform.Position,
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
	public override void Draw(SpriteBatch spriteBatch, VV13 vv13, Vector2 position, PosRotScale transform, Color drawColor, float opacity) { }
}
public class VV13_Drill : VV13Tool {
	public override VV13Animation UseAnimation => GetInstance<VV13_Use_Tool>();
	public override int FrameCount => 3;
	public override Vector2 Origin => new(80, 8);
	public override int DroppedSize => 1;
}
public class VV13_Saw : VV13Tool {
	public override VV13Animation UseAnimation => GetInstance<VV13_Use_Tool>();
	public override int FrameCount => 5;
	public override Vector2 Origin => new(72, 10);
	public override int DroppedSize => 1;
}
public class VV13_Puncher : VV13Tool {
	public override VV13Animation UseAnimation => GetInstance<VV13_Use_Tool>();
	public override int FrameCount => 5;
	public override Vector2 Origin => new(56, 40);
	public override int DroppedSize => 1;
}
public class VV13_Torch : VV13Tool {
	public override VV13Animation UseAnimation => GetInstance<VV13_Use_Tool>();
	public override int FrameCount => 1;
	public override Vector2 Origin => new(10, 18);
	public override int DroppedSize => 1;
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