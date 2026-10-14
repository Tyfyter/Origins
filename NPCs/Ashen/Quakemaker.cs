using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Gores;
using Origins.Items.Accessories;
using Origins.Items.Armor.Ashen;
using Origins.Items.Materials;
using Origins.LootConditions;
using Origins.World.BiomeData;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.NPCs.Ashen; 
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
	public override void ModifyNPCLoot(NPCLoot npcLoot) {
		npcLoot.Add(new CommonDrop(ModContent.ItemType<Biocomponent10>(), 1, 1, 3));
		npcLoot.Add(ScavengerBonus.Scrap(amountDroppedMinimum: 3, amountDroppedMaximum: 7));
		npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ashen2_Helmet>(), 525));
		npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ashen2_Breastplate>(), 525));
		npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ashen2_Greaves>(), 525));
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
			if (IsDrilling()) {
				Main.instance.CameraModifiers.Add(new CameraShakeModifier(
					NPC.Center, 2f, 1f, 10, 750f, -1f, "Quakemaker"
				));
				/*drillSound.PlaySoundIfInactive(
					Origins.Sounds.Saw,
					NPC.Center,
					_IsDrilling ??= IsDrilling
				);*/
			}
		}
	}
	SoundUpdateCallback _IsDrilling;
	float soundVolume;
	bool IsDrilling(ActiveSound sound = default) {
		Rectangle hitbox = NPC.Hitbox.Add((NPC.rotation - MathHelper.PiOver2).ToRotationVector2() * 16);
		hitbox.Inflate(-12, -12);
		if (sound is not null) {
			const float volume = 0.5f;
			sound.Position = hitbox.Center();
			MathUtils.LinearSmoothing(ref soundVolume, (NPC.active && hitbox.OverlapsAnyTiles()).Mul(volume), volume / 10);
			sound.Volume = soundVolume;
			return soundVolume > 0;
		}
		return hitbox.OverlapsAnyTiles();
	}
	SlotId drillSound;
	protected internal override void HeadAI() {
		bool isAlreadyAttacking = false;
		float totalSegs = 0;
		float groundedSegs = 0;
		foreach (Worm seg in IterateWorm()) {
			NPC segment = seg.NPC;
			if (seg != this && (segment.ai[2] != 0 || segment.localAI[3] != 0)) isAlreadyAttacking = true;
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
	public override void DeathEffect(in NPC.HitInfo hit, IEntitySource source, Vector2 velocity, int index) {
		for (int i = 0; i < 4; i++) {
			Gore.NewGorePerfect(
				source,
				NPC.Center,
				(velocity + NPC.velocity) * 0.5f + Main.rand.NextVector2Circular(2, 2),
				GoreCache.Ashen_Generic
			);
		}
		Gore.NewGore(
			NPC.GetSource_Death(),
			NPC.Center + new Vector2(0 * NPC.direction, -2).RotatedBy(NPC.rotation),
			(velocity + NPC.velocity) * 0.5f + Main.rand.NextVector2Circular(2, 2),
			Mod.GetGoreSlot("Gores/NPCs/Quakemaker_Gore5")
		);
		if (UseAltHead) {
			Gore.NewGore(
				NPC.GetSource_Death(),
				NPC.Center + new Vector2(0 * NPC.direction, 9).RotatedBy(NPC.rotation),
				(velocity + NPC.velocity) * 0.5f + Main.rand.NextVector2Circular(2, 2),
				Mod.GetGoreSlot("Gores/NPCs/Quakemaker_Gore4")
			);
		} else {
			Gore.NewGore(
				NPC.GetSource_Death(),
				NPC.Center + new Vector2(0 * NPC.direction, -4).RotatedBy(NPC.rotation),
				(velocity + NPC.velocity) * 0.5f + Main.rand.NextVector2Circular(2, 2),
				Mod.GetGoreSlot("Gores/NPCs/Quakemaker_Gore6")
			);
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
		UseAltHead = false;
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
		foreach (Point pos in NPC.Hitbox.IterateTilesIn()) {
			Tile tile = Main.tile[pos];
			if (!tile.HasTile || !Main.tileBlockLight[tile.TileType]) Lighting.AddLight(pos.X, pos.Y, 0.03f, 0.0183f, 0.003f);
		}
		if (NPC.ai[2] != 0 || NPC.localAI[3] != 0) {
			Vector2 pos = new(NPC.ai[2], NPC.localAI[3]);
			if (NPC.Hitbox.Contains(pos)) {
				NPC.SpawnProjectile(
					NPC.GetSource_FromAI(),
					pos,
					NPC.localAI[2].ToRotationVector2() * 10 + Main.rand.NextVector2Circular(2, 2),
					ModContent.ProjectileType<Quakemaker_Fire>(),
					Quakemaker_Fire.ShotDamage,
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
	public override void DeathEffect(in NPC.HitInfo hit, IEntitySource source, Vector2 velocity, int index) {
		for (int i = 0; i < 4; i++) {
			Gore.NewGorePerfect(
				source,
				NPC.Center,
				(velocity + NPC.velocity) * 0.5f + Main.rand.NextVector2Circular(2, 2),
				GoreCache.Ashen_Generic
			);
		}
		Gore.NewGore(
			NPC.GetSource_Death(),
			NPC.Center + new Vector2(-3 * NPC.direction, 2).RotatedBy(NPC.rotation),
			(velocity + NPC.velocity) * 0.5f + Main.rand.NextVector2Circular(2, 2),
			Mod.GetGoreSlot("Gores/NPCs/Quakemaker_Gore3")
		);
		Gore.NewGore(
			NPC.GetSource_Death(),
			NPC.Center + new Vector2(18 * NPC.direction, 6).RotatedBy(NPC.rotation),
			(velocity + NPC.velocity) * 0.5f + Main.rand.NextVector2Circular(2, 2),
			Mod.GetGoreSlot("Gores/NPCs/Quakemaker_Gore2")
		);
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
public class Quakemaker_Tail : WormTail {
	public override bool SharesImmunityFrames => true;
	public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.NPCs.Quakemaker.DisplayName");
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
	public override void DeathEffect(in NPC.HitInfo hit, IEntitySource source, Vector2 velocity, int index) {
		for (int i = 0; i < 4; i++) {
			Gore.NewGorePerfect(
				source,
				NPC.Center,
				(velocity + NPC.velocity) * 0.5f + Main.rand.NextVector2Circular(2, 2),
				GoreCache.Ashen_Generic
			);
		}
		Gore.NewGore(
			NPC.GetSource_Death(),
			NPC.Center + new Vector2(3 * NPC.direction, 3).RotatedBy(NPC.rotation),
			(velocity + NPC.velocity) * 0.5f + Main.rand.NextVector2Circular(2, 2),
			Mod.GetGoreSlot("Gores/NPCs/Quakemaker_Gore1")
		);
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
public class Quakemaker_Fire : ModProjectile {
	public static int ShotDamage => (int)(18 * ContentExtensions.DifficultyDamageMultiplier);
	public static int ShotLingerDuration => (int)(300 + 100 * ContentExtensions.DifficultyDamageMultiplier);
	public override string Texture => $"Terraria/Images/Projectile_" + ProjectileID.BallofFire;
	public override void SetStaticDefaults() {
		ProjectileID.Sets.TrailingMode[Type] = 3;
		ProjectileID.Sets.TrailCacheLength[Type] = 30;
	}
	public override void SetDefaults() {
		Projectile.width = 4;
		Projectile.height = 4;
		Projectile.hostile = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 60000;
		Projectile.DamageType = DamageClass.Ranged;
		Projectile.extraUpdates = 1;
		Projectile.hide = true;
	}
	public override bool ShouldUpdatePosition() => Projectile.tileCollide;
	public override void AI() {
		if (Projectile.soundDelay.TrySet(-1)) ;
		if (Projectile.tileCollide) {
			SoundEngine.PlaySound(Origins.Sounds.Bonk.WithVolume(0.5f), Projectile.Center);
			SoundEngine.PlaySound(SoundID.Item45.WithVolume(0.8f), Projectile.Center);
			Projectile.velocity.Y += 0.08f;
			Projectile.rotation = Projectile.velocity.ToRotation();
		} else {
			Min(ref Projectile.timeLeft, ShotLingerDuration);
			Projectile.ai[0] += 2;
			Projectile.rotation = MathHelper.PiOver2;
			for (int i = 0; i < Projectile.oldPos.Length && i < Projectile.ai[0]; i++) {
				if (Projectile.oldPos[i] == default) break;
				Projectile.oldPos[i] += Projectile.velocity;
				Projectile.oldPos[i].Y -= i * 0.25f;
			}
		}
		Lighting.AddLight(Projectile.Center, TorchID.Orange);
		if (Projectile.timeLeft < 20f) Projectile.Opacity = Projectile.timeLeft / 20f;
	}
	public override bool PreDraw(ref Color lightColor) {
		default(FlameLashDrawer).Draw(Projectile);
		return false;
	}
	public override bool OnTileCollide(Vector2 oldVelocity) {
		if (Projectile.velocity.Y > oldVelocity.Y) return false;
		Projectile.position += Projectile.velocity;
		Projectile.tileCollide = false;
		_ = (Projectile.velocity - oldVelocity).Abs(out Projectile.velocity);
		Projectile.position += Projectile.velocity.Normalized(out _) * -8;
		Projectile.velocity.Y -= 1.5f;
		Projectile.velocity.Y *= 0.35f;
		Projectile.velocity *= 2;
		return false;
	}
	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
		targetHitbox.Inflate(12, 12);
		for (int i = 0; i < 20; i++) {
			if (Projectile.oldPos[i] == default) break;
			if (targetHitbox.Contains(Projectile.oldPos[i] + Vector2.One * 2)) return true;
		}
		return false;
	}
	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
		behindNPCsAndTiles.Add(index);
	}
	public struct FlameLashDrawer {
		private static readonly VertexStrip _vertexStrip = new();
		private static float TransitToDark => 0;
		float opacity;
		public void Draw(Projectile proj) {
			opacity = proj.Opacity;
			MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
			miscShaderData.UseSaturation(-2f);
			miscShaderData.UseOpacity(MathHelper.Lerp(4f, 8f, TransitToDark));
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(proj.oldPos, proj.oldRot, StripColors, StripWidth, -Main.screenPosition + proj.Size / 2f);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}
		private readonly Color StripColors(float progressOnStrip) {
			float lerpValue = Utils.GetLerpValue(0f - 0.1f * TransitToDark, 0.7f - 0.2f * TransitToDark, progressOnStrip, clamped: true);
			Color result = Color.Lerp(Color.Lerp(Color.White, Color.Orange, TransitToDark * 0.5f), Color.Red, lerpValue) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
			result.A = (byte)Math.Min(result.A * 2, 255);
			return result * opacity;
		}
		private static float StripWidth(float progressOnStrip) {
			float lerpValue = Utils.GetLerpValue(0f, 0.06f + TransitToDark * 0.01f, progressOnStrip, clamped: true);
			lerpValue = 1f - (1f - lerpValue) * (1f - lerpValue);
			return MathHelper.Lerp(24f + TransitToDark * 16f, 8f, Utils.GetLerpValue(0f, 1f, progressOnStrip, clamped: true)) * lerpValue;
		}
	}
}
public class Quakemaker_Lava : ModProjectile {
	public override string Texture => "Origins/Projectiles/Weapons/Lava_Cast_P";
}