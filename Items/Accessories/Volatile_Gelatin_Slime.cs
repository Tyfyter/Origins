using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Origins.Dev;
using Origins.Graphics;
using Origins.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
namespace Origins.Items.Accessories {
	public class Volatile_Gelatin_Global_Item : GlobalItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override bool IsLoadingEnabled(Mod mod) => OriginConfig.Instance.VolatileGelatin;
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.VolatileGelatin;
		public override void SetStaticDefaults() {
			ContentSamples.ItemsByType[ItemID.VolatileGelatin].GetPrefixCategories().AddRange([PrefixCategory.AnyWeapon]);
		}
		public override void SetDefaults(Item entity) {
			entity.DamageType = DamageClasses.Explosive;
			entity.damage = 65;
			entity.knockBack = 7;
			entity.useTime = 40;
			entity.useAnimation = 40;
		}
		public override void Load() {
			IL_Player.VolatileGelatin += IL_Player_VolatileGelatin;
		}

		private void IL_Player_VolatileGelatin(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.Before,
				i => i.MatchLdcI4(40)
			);
			c.Remove();
			c.EmitLdarg0();
			c.EmitLdarg1();
			c.EmitDelegate((Player player, Item item) => CombinedHooks.TotalUseTime(item.useTime, player, item));
			c.GotoNext(MoveType.Before,
				i => i.MatchLdcI4(65),
				i => i.MatchStloc(out _)
			);
			c.Remove();
			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldarg_1);
			c.Emit(OpCodes.Ldc_I4_0);
			c.Emit<Player>(OpCodes.Call, nameof(Player.GetWeaponDamage));
			c.GotoNext(MoveType.Before,
				i => i.MatchLdcR4(7f),
				i => i.MatchStloc(out _)
			);
			c.Remove();
			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldarg_1);
			c.Emit<Player>(OpCodes.Call, nameof(Player.GetWeaponKnockback));
		}
		public override int ChoosePrefix(Item item, UnifiedRandom rand) {
			return OriginExtensions.GetAllPrefixes(item, rand, (PrefixCategory.AnyWeapon, pre => Origins.SpecialPrefix[pre] ? 1 : 0.25f), (PrefixCategory.Accessory, null));
		}
	}
	public class Volatile_Gelatin_Global : GlobalProjectile {
		public override bool IsLoadingEnabled(Mod mod) => OriginConfig.Instance.VolatileGelatin;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.type == ProjectileID.VolatileGelatinBall;
		public override void SetDefaults(Projectile entity) {
			entity.DamageType = DamageClasses.Explosive;
		}
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.TryGetGlobalNPC(out Volatile_Gelatin_Slime_Global global)) {
				global.slimeTime = 600;
				global.slimeDamage = hit.SourceDamage;
			}
		}
	}
	public class Volatile_Gelatin_Slime_Global : GlobalNPC {
		public override bool IsLoadingEnabled(Mod mod) => OriginConfig.Instance.VolatileGelatin;
		internal static List<int> cachedNPCs;
		internal static bool anyActive;
		public static ScreenTarget SlimeTarget { get; private set; }
		public override bool InstancePerEntity => true;
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => !entity.friendly || entity.type is NPCID.Guide or NPCID.Clothier;
		public int slimeTime = 0;
		public int slimeDamage = 0;
		public override void ResetEffects(NPC npc) {
			if (slimeTime > 0) slimeTime--;
		}
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) {
			if (slimeTime > 0 && (ExplosiveGlobalProjectile.IsExploding(projectile, true) || OriginsSets.Projectiles.FireProjectiles[projectile.type])) {
				slimeTime = 0;
				Projectile.NewProjectile(
					projectile.GetSource_OnHit(npc, OriginGlobalProj.no_multishot_context),
					npc.Center,
					default,
					ModContent.ProjectileType<Volatile_Gelatin_Slime_Explosion>(),
					slimeDamage,
					6,
					projectile.owner
				);
			}
		}
		public override void PostAI(NPC npc) {
			if (slimeTime > 0) {
				for (int i = 0; i < npc.buffType.Length; i++) {
					if(npc.buffTime[i] > 0 && npc.buffType[i] is BuffID.OnFire or BuffID.OnFire or BuffID.OnFire3) {
						slimeTime = 0;
						EntitySource_Buff Source = new(npc, npc.buffType[i], i, OriginGlobalProj.no_multishot_context);

						Projectile.NewProjectile(
							npc.GetSource_Buff(i),
							npc.Center,
							default,
							ModContent.ProjectileType<Volatile_Gelatin_Slime_Explosion>(),
							slimeDamage,
							6,
							Main.myPlayer
						);
						npc.DelBuff(i);
						break;
					}
				}
			}
		}
		#region rendering
		public override void Load() {
			if (Main.dedServ) return;
			cachedNPCs = new();
			SlimeTarget = new(
				MaskAura,
				() => {
					bool isActive = anyActive;
					anyActive = false;
					return isActive && Lighting.NotRetro;
				},
				0
			);
			On_Main.DrawInfernoRings += Main_DrawInfernoRings;
		}
		private void Main_DrawInfernoRings(On_Main.orig_DrawInfernoRings orig, Main self) {
			orig(self);
			if (Main.dedServ) return;
			if (Lighting.NotRetro) DrawAura(Main.spriteBatch);
		}
		public override void Unload() {
			cachedNPCs = null;
			SlimeTarget = null;
		}
		static void MaskAura(SpriteBatch spriteBatch) {
			if (Main.dedServ) return;
			//SpriteBatch mainSpriteBatch = Main.spriteBatch;
			try {
				//Main.spriteBatch = spriteBatch;
				GraphicsUtils.drawingEffect = true;
				for (int i = 0; i < cachedNPCs.Count; i++) {
					int index = cachedNPCs[i];
					NPC npc = Main.npc[index];
					if (npc.active) {
						Main.instance.DrawNPC(index, npc.behindTiles);
					}
				}
			} finally {
				cachedNPCs.Clear();
				GraphicsUtils.drawingEffect = false;
				//Main.spriteBatch = mainSpriteBatch;
			}
		}
		static void DrawAura(SpriteBatch spriteBatch) {
			if (Main.dedServ) return;
			//anyActive = false;
			Main.LocalPlayer.ManageSpecialBiomeVisuals("Origins:VolatileGelatinFilter", anyActive, Main.LocalPlayer.Center);
			if (anyActive) {
				Filters.Scene["Origins:VolatileGelatinFilter"].GetShader().UseImage(SlimeTarget.RenderTarget, 1);
			}
			spriteBatch.Draw(SlimeTarget.RenderTarget, Vector2.Zero, Color.Blue);
		}
		public override void DrawEffects(NPC npc, ref Color drawColor) {
			if (GraphicsUtils.drawingEffect) {
				const float fadeTime = 180;
				drawColor = Color.Lerp(drawColor, Color.White, 0.5f);
				drawColor *= slimeTime < fadeTime ? (slimeTime / fadeTime) : 1;
			}
		}
		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (slimeTime > 0 && npc.type != NPCID.TargetDummy && !GraphicsUtils.drawingEffect) {
				cachedNPCs.Add(npc.whoAmI);
				anyActive = true;
			}
			return true;
		}
		#endregion
	}
	public class Volatile_Gelatin_Slime_Explosion : ModProjectile, IIsExplodingProjectile {
		public override string Texture => $"Terraria/Images/Item_{ItemID.VolatileGelatin}";
		public override void SetStaticDefaults() {
			OriginsSets.Projectiles.NoMultishot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 3;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				Rectangle area = Projectile.Hitbox;
				Vector2 center = area.Center.ToVector2();
				Terraria.Audio.SoundEngine.PlaySound(in SoundID.Item62, center);
				Vector2 topLeft = area.TopLeft();
				for (int i = 0; i < 20; i++) {
					Color slimeColor = Main.hslToRgb(0.7f + 0.2f * Main.rand.NextFloat(), 1f, 0.7f);
					Dust dust = Dust.NewDustDirect(
						topLeft,
						area.Width,
						area.Height,
						DustID.ShimmerSpark,
						0f,
						0f,
						100,
						slimeColor,
						2.5f
					);
					dust.noGravity = true;
					dust.velocity *= 7f;
					Dust.NewDustDirect(
						topLeft,
						area.Width,
						area.Height,
						DustID.ShimmerSpark,
						0f,
						0f,
						100,
						slimeColor,
						1.5f
					).velocity *= 3f;
				}
				Projectile.ai[0] = 1;
			}
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding => true;
	}
}
