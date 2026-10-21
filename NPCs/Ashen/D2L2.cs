using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Core.Animation;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Mounts.Star_Soldier;
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
using static Origins.Items.Mounts.Star_Soldier.Landing_Animation;
using static Origins.NPCs.Ashen.D2L2;
using static Terraria.ModLoader.ModContent;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen;
public class D2L2 : ModNPC, IAshenEnemy {
	static AutoLoadingTexture gunTex = typeof(D2L2).GetDefaultTMLName("_Gun");
	static AutoGlowingTexture thighTex = typeof(D2L2).GetDefaultTMLName("_Thigh");
	static AutoLoadingTexture calfTex = typeof(D2L2).GetDefaultTMLName("_Calf");
	static AutoLoadingTexture footTex = typeof(D2L2).GetDefaultTMLName("_Foot");
	public D2L2Animation Animation {
		get => D2L2Animation.animations[NPC.aiAction];
		set => NPC.aiAction = value.Type;
	}
	public ref float AnimationTime => ref NPC.ai[0];
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
		NPC.width = 70;
		NPC.height = 76;
		NPC.value = 230;
		NPC.knockBackResist = 0.5f;
		NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
		NPC.DeathSound = SoundID.NPCDeath44;
		NPC.noGravity = true;
		SpawnModBiomes = [
			GetInstance<Ashen_Biome>().Type,
		];
	}
	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
		bestiaryEntry.CustomBestiaryName(Type, this.GetLocalizationKey("FullName"));
		bestiaryEntry.AddTags(
			this.GetBestiaryFlavorText()
		);
	}
	public override void AI() {
		NPC.velocity = default;
	}
	public override void FindFrame(int frameHeight) {
		NPC.spriteDirection = NPC.direction;
		D2L2Animation animation = Animation;
		float oldTime = AnimationTime;
		animationControls.Play(ref AnimationTime, animation);
		animation.events.Update(this, oldTime, AnimationTime);
	}
	public override void ModifyNPCLoot(NPCLoot npcLoot) {
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
		D2L2Animation animation = Animation;
		Vector2 pos = NPC.Center - screenPos;
		DrawLeg(spriteBatch, pos, animation.thighBack, animation.calfBack, animation.footBack, animation.footBackFrame, drawColor.MultiplyRGBA(Color.Gray), glowColor.MultiplyRGBA(Color.Gray));
		spriteBatch.DrawRiggedPart(
			gunTex,
			pos,
			animation.root.GetCurrentValue(AnimationTime) + D2L2Animation.gunOffset + animation.gun.GetCurrentValue(AnimationTime),
			null,
			drawColor,
			0,
			new(62, 9),
			1,
			SpriteEffects.None
		);
		spriteBatch.DrawRiggedPart(
			TextureAssets.Npc[Type].Value,
			pos,
			animation.root.GetCurrentValue(AnimationTime),
			null,
			drawColor,
			0,
			new(35, 38),
			1,
			SpriteEffects.None
		);
		DrawLeg(spriteBatch, pos, animation.thighFront, animation.calfFront, animation.footFront, animation.footFrontFrame, drawColor, glowColor);
		NPC.DrawConfused();
		Editor.D2L2 = this;
		Animation_Editor_UI.DrawEditorUI = Editor.DrawUI;
		return false;
	}
	public void DrawLeg(SpriteBatch spriteBatch, Vector2 pos, KeyframeSet<PosRotScale> thighOffset, KeyframeSet<PosRotScale> calfOffset, KeyframeSet<PosRotScale> footOffset, KeyframeSet<FootFrame> footFrame, Color drawColor, Color glowColor) {
		PosRotScale legTransform = Animation.root.GetCurrentValue(AnimationTime) + D2L2Animation.thighOffset + thighOffset.GetCurrentValue(AnimationTime);
		spriteBatch.DrawGlowingRiggedPart(
			thighTex.Texture,
			thighTex.GlowTexture,
			pos,
			legTransform,
			null,
			drawColor,
			glowColor,
			0,
			new(16, 16),
			1,
			SpriteEffects.None
		);
		legTransform += D2L2Animation.calfOffset;
		legTransform += calfOffset.GetCurrentValue(AnimationTime);
		spriteBatch.DrawRiggedPart(
			calfTex,
			pos,
			legTransform,
			null,
			drawColor,
			0,
			new(68, 20),
			1,
			SpriteEffects.None
		);
		legTransform += D2L2Animation.footOffset;
		legTransform += footOffset.GetCurrentValue(AnimationTime);
		spriteBatch.DrawRiggedPart(
			footTex,
			pos,
			legTransform,
			footTex.Frame(verticalFrames: 2, frameY: footFrame.GetCurrentValue(AnimationTime)),
			drawColor,
			0,
			new(32, 10),
			1,
			SpriteEffects.None
		);
	}
	KeyframeAnimation.PlayingState animationControls;
	static class Editor {
		public static D2L2 D2L2;
		public static void DrawUI(SpriteBatch spriteBatch) {
			D2L2Animation animation = D2L2.Animation;
			animation.gizmoBasePosition = D2L2.NPC.Center - Main.screenPosition;
			animation.DrawEditorUI(spriteBatch, ref D2L2.AnimationTime, ref D2L2.animationControls);
			if (KeyframeAnimation.DrawSwapper(spriteBatch, D2L2Animation.animations, out D2L2Animation selected, D2L2.Animation)) D2L2.Animation = selected;
		}
	}
	public record struct FootFrame(int Frame) : IAnimatableSpriteFrame<FootFrame> {
		public static Texture2D Texture => footTex;
		public static int FrameCount => 2;
		public static implicit operator FootFrame(int value) => new(value);
		public static implicit operator int(FootFrame value) => value.Frame;
	}
}
#region animations
public abstract class D2L2Animation : KeyframeAnimation, ILoadable {
	public static List<D2L2Animation> animations = [];
	public readonly static Vector2 gunOffset = new(-11, 29);
	public readonly static Vector2 thighOffset = new(15, -19);
	public readonly static Vector2 calfOffset = new(74, 0);
	public readonly static Vector2 footOffset = new(-54, 100);
	public KeyframeSet<PosRotScale> root = new(new());
	public KeyframeSet<PosRotScale> gun = new(new(), new ChildOfOffset<PosRotScale>("root", gunOffset));
	public KeyframeSet<PosRotScale> thighFront = new(new(), new ChildOfOffset<PosRotScale>("root", thighOffset));
	public KeyframeSet<PosRotScale> calfFront = new(new(), new ChildOfOffset<PosRotScale>("thighFront", calfOffset));
	public KeyframeSet<PosRotScale> footFront = new(new(), new ChildOfOffset<PosRotScale>("calfFront", footOffset));
	public KeyframeSet<FootFrame> footFrontFrame = new();
	public KeyframeSet<PosRotScale> thighBack = new(new(), new ChildOfOffset<PosRotScale>("root", thighOffset));
	public KeyframeSet<PosRotScale> calfBack = new(new(), new ChildOfOffset<PosRotScale>("thighBack", calfOffset));
	public KeyframeSet<PosRotScale> footBack = new(new(), new ChildOfOffset<PosRotScale>("calfBack", footOffset));
	public KeyframeSet<FootFrame> footBackFrame = new();
	public EventKeyframeSet<D2L2> events = [];

	public int Type { get; private set; }
	public void Load(Mod mod) {
		Type = animations.Count;
		animations.Add(this);
	}
	public void Unload() { }
	public virtual void DrawExtras(SpriteBatch spriteBatch, D2L2 D2L2, Vector2 position, Color drawColor) { }
}
public class D2L2_Idle : D2L2Animation {
	public D2L2_Idle() {
		MinimumLength = 30;
	}
}
#endregion animations