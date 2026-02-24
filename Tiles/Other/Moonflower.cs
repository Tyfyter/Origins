using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
	public class Moonflower : ModTile {
		public static ushort ID { get; private set; }
		public override void Load() => new TileItem(this).WithExtraStaticDefaults(this.DropTileItem).RegisterItem();
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Moonflower).GetDefaultTMLName("_Glow");
		public static new int AnimationFrameHeight { get; private set; }
		public override void SetStaticDefaults() {

			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.SetHeight(4, 18);
			AnimationFrameHeight = TileObjectData.newTile.CoordinateHeights.Sum(h => h + 2) - 2;
			base.AnimationFrameHeight = AnimationFrameHeight;
			TileObjectData.newTile.SetOriginBottom();
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;

			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.AnchorValidTiles = [
				TileID.Grass,
				TileID.Stone,
				TileID.Pearlstone,
				TileID.HallowedGrass,
				TileID.JungleGrass,
				TileID.SnowBlock,
				TileID.IceBlock,
				TileID.HallowedIce,
				TileID.LunarOre,
			];

			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.Allowed;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(40, 120, 80), CreateMapEntryName());
			ID = Type;
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowTexture = glowTexture;
			drawData.glowColor = Color.White;
			drawData.glowSourceRect = new(drawData.tileFrameX, drawData.tileFrameY + AnimationFrameHeight * Main.moonPhase, 16, 16);
			Mood.Current.TileDrawEffects(i, j, spriteBatch, ref drawData);
		}
		public abstract class Mood : ModType {
			static Mood lastMood = null;
			static long lastUpdated;
			public static Mood Current {
				get {
					if (lastMood is null || OriginSystem.gameTickCount != lastUpdated) {
						lastMood = ModContent.GetInstance<Arcane>();
						foreach (Mood option in ModContent.GetContent<Mood>()) {
							if (option.IsActive && option.Priority > lastMood.Priority) lastMood = option;
						}
						lastUpdated = OriginSystem.gameTickCount;
					}
					return lastMood;
				}
			}
			string BaseKey => $"Mods.Origins.Buffs.Moonflower_Buff.{Name}";
			public virtual LocalizedText DisplayName => Language.GetOrRegister($"{BaseKey}.{nameof(ModBuff.DisplayName)}", () => Name);
			public virtual LocalizedText Description => Language.GetOrRegister($"{BaseKey}.{nameof(ModBuff.Description)}");
			protected sealed override void Register() {
				ModTypeLookup<Mood>.Register(this);
			}
			public sealed override void SetupContent() {
				_ = DisplayName;
				_ = Description;
				SetStaticDefaults();
			}
			public virtual float Priority => 0;
			public abstract bool IsActive { get; }
			public abstract void Update(Player player, Moonflower_Player moonflowerPlayer);
			public virtual bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams) => true;
			public virtual void TileDrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) { }
			public class Arcane : Mood {
				public override bool IsActive => true;
				public override float Priority => -1;
				public override void Update(Player player, Moonflower_Player moonflowerPlayer) {
					player.manaCost = 0;
					player.maxMinions += 1;
					moonflowerPlayer.discount += 1f;
					moonflowerPlayer.spawnRateBonus += 0.3f;
				}
			}
			public class Brilliant : Mood {
				public override bool IsActive => Main.GetMoonPhase() == MoonPhase.Full;
				public override void Update(Player player, Moonflower_Player moonflowerPlayer) {
					player.manaRegenDelayBonus += 1;
					player.GetDamage(DamageClass.Generic) += 0.05f;
				}
			}
			public class Tranquil : Mood {
				public override bool IsActive => Main.GetMoonPhase() is MoonPhase.ThreeQuartersAtLeft or MoonPhase.ThreeQuartersAtRight;
				public override void Update(Player player, Moonflower_Player moonflowerPlayer) {
					moonflowerPlayer.spawnRateBonus -= 0.1f;
					player.lifeRegen += 2;
					moonflowerPlayer.discount += 0.03f;
				}
			}
			public class Balanced : Mood {
				public override bool IsActive => Main.GetMoonPhase() is MoonPhase.HalfAtLeft or MoonPhase.HalfAtRight;
				public override void Update(Player player, Moonflower_Player moonflowerPlayer) {
					player.endurance += (1 - player.endurance) * 0.04f;
					player.GetDamage(DamageClass.Generic) += 0.04f;
				}
			}
			public class Reticent : Mood {
				public override bool IsActive => Main.GetMoonPhase() == MoonPhase.QuarterAtLeft;
				public override void Update(Player player, Moonflower_Player moonflowerPlayer) {
					player.lifeRegen += 2;
					player.endurance += (1 - player.endurance) * 0.05f;
					moonflowerPlayer.spawnRateBonus -= 0.3f;
				}
			}
			public class Hidden : Mood {
				public override bool IsActive => Main.GetMoonPhase() == MoonPhase.Empty;
				public override void Update(Player player, Moonflower_Player moonflowerPlayer) {
					moonflowerPlayer.spawnRateBonus -= 0.1f;
					player.GetCritChance(DamageClass.Generic) += 5;
					player.aggro -= 200;
				}
			}
			public class Curious : Mood {
				public override bool IsActive => Main.GetMoonPhase() == MoonPhase.QuarterAtRight;
				public override void Update(Player player, Moonflower_Player moonflowerPlayer) {
					moonflowerPlayer.luckBonus += 0.2f;
					player.OriginPlayer().fairyLotus = true;
				}
			}
			public class Sanguine : Mood {
				public override bool IsActive => Main.bloodMoon;
				public override float Priority => 2;
				AutoLoadingAsset<Texture2D> baseTexture = typeof(Moonflower).GetDefaultTMLName("_Blood");
				AutoLoadingAsset<Texture2D> glowTexture = typeof(Moonflower).GetDefaultTMLName("_Blood_Glow");
				public override void Update(Player player, Moonflower_Player moonflowerPlayer) {
					float funAmount = moonflowerPlayer.murderAmount * 0.002f;
					moonflowerPlayer.luckBonus += Math.Min(funAmount * 0.1f, 0.4f);
					moonflowerPlayer.spawnRateBonus += Math.Min(funAmount * 0.1f, 0.5f);
				}
				public override void TileDrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
					drawData.drawTexture = baseTexture;
					drawData.glowTexture = glowTexture;
					drawData.glowSourceRect = new(drawData.tileFrameX, drawData.tileFrameY + AnimationFrameHeight * Main.tileFrame[TileID.BloodMoonMonolith], 16, 16);
				}
			}
			public class Cold : Mood {
				public override bool IsActive => Main.snowMoon;
				public override float Priority => 3;
				AutoLoadingAsset<Texture2D> glowTexture = typeof(Moonflower).GetDefaultTMLName("_Cold_Glow");
				public override void Update(Player player, Moonflower_Player moonflowerPlayer) {
					moonflowerPlayer.coldImmune = true;
				}
				public override void TileDrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
					drawData.glowTexture = glowTexture;
					drawData.glowSourceRect.Y = drawData.tileFrameY;
				}
			}
			public class Impish : Mood {
				public override bool IsActive => Main.pumpkinMoon;
				public override float Priority => 3;
				AutoLoadingAsset<Texture2D> glowTexture = typeof(Moonflower).GetDefaultTMLName("_Impish_Glow");
				public override void Update(Player player, Moonflower_Player moonflowerPlayer) {
					moonflowerPlayer.coldImmune = true;
				}
				public override void TileDrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
					drawData.glowTexture = glowTexture;
					drawData.glowSourceRect.Y = drawData.tileFrameY;
				}
			}
		}
		public class Moonflower_Buff : ModBuff {
			public override string Texture => $"Terraria/Images/Buff_{BuffID.Sunflower}";
			public override LocalizedText DisplayName => LocalizedText.Empty;
			public override LocalizedText Description => LocalizedText.Empty;
			public override void SetStaticDefaults() {
				On_ShopHelper.GetShoppingSettings += ShopHelper_GetShoppingSettings;
			}
			static ShoppingSettings ShopHelper_GetShoppingSettings(On_ShopHelper.orig_GetShoppingSettings orig, ShopHelper self, Player player, NPC npc) {
				ShoppingSettings settings = orig(self, player, npc);
				settings.PriceAdjustment -= player.GetModPlayer<Moonflower_Player>().discount;
				return settings;
			}
			public override void Update(Player player, ref int buffIndex) => Mood.Current.Update(player, player.GetModPlayer<Moonflower_Player>());
			public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare) {
				Mood mood = Mood.Current;
				buffName = mood.DisplayName.Value;
				tip = mood.Description.Value;
			}
			public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams) {
				return Mood.Current.PreDraw(spriteBatch, buffIndex, ref drawParams);
			}
		}
		public class Moonflower_Player : ModPlayer {
			public override void Load() => autoReset = AutoResetAttribute.GenerateReset<Moonflower_Player>();
			static Action<Moonflower_Player> autoReset;
			[AutoReset] public bool buffActive;
			[AutoReset] public float luckBonus;
			[AutoReset] public float spawnRateBonus;
			[AutoReset] public float discount;
			[AutoReset] public bool coldImmune;
			[AutoReset] public bool ignoredChilled;
			[AutoReset] public bool ignoredFrozen;
			[AutoReset] public bool ignoredFrostburn;
			public float murderAmount = 0f;
			public override void ResetEffects() {
				autoReset(this);
				if (murderAmount > 0) {
					if (Main.bloodMoon) murderAmount -= 2 + murderAmount * 0.00075f;
					else murderAmount = 0;
				} else murderAmount = 0;
				if (Player.whoAmI != Main.myPlayer) return;
				if (Main.SceneMetrics.GetTileCount(Moonflower.ID) > 0) Player.AddBuff(ModContent.BuffType<Moonflower_Buff>(), 2, false);
			}
			public override void PostUpdateBuffs() {
				if (coldImmune) {
					if (Player.chilled) {
						Player.statDefense += 8;
						Player.chilled = false;
						ignoredChilled = true;
					}
					if (Player.frozen) {
						Player.endurance += (1 - Player.endurance) * 0.08f;
						Player.frozen = false;
						ignoredFrozen = true;
					}
					if (Player.onFrostBurn) {
						Player.lifeRegenCount += 4;
						Player.onFrostBurn = false;
						Player.frostBurn = true;
						ignoredFrostburn = true;
					}
					if (Player.onFrostBurn2) {
						Player.lifeRegenCount += 8;
						Player.onFrostBurn2 = false;
						Player.frostBurn = true;
						ignoredFrostburn = true;
					}
				}
			}
			public override void PostUpdateEquips() {
				if (coldImmune) {
					Player.buffImmune[BuffID.Chilled] = false;
					Player.buffImmune[BuffID.Frozen] = false;
					Player.buffImmune[BuffID.Frostburn] = false;
					Player.buffImmune[BuffID.Frostburn2] = false;
				}
			}
			public override void ModifyLuck(ref float luck) {
				luck += luckBonus;
			}
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
				if (target.life <= 0) murderAmount += target.lifeMax;
			}
			public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
				if (drawInfo.drawPlayer != Player) return;
				if (coldImmune) {
					float saturation = 0.9f;
					float lerpAmount = 0.1f;
					Color coldColor = new Color(96, 240, 235);
					if (ignoredChilled) {
						saturation -= 0.15f;
						lerpAmount += 0.1f;
					}
					if (ignoredFrozen) {
						saturation -= 0.35f;
						lerpAmount += 0.25f;
					}
					drawInfo.colorHead = Color.Lerp(drawInfo.colorBodySkin.Desaturate(saturation), coldColor.MultiplyRGBA(drawInfo.colorArmorHead), lerpAmount);
					drawInfo.colorBodySkin = Color.Lerp(drawInfo.colorBodySkin.Desaturate(saturation), coldColor.MultiplyRGBA(drawInfo.colorArmorBody), lerpAmount);
					drawInfo.colorLegs = Color.Lerp(drawInfo.colorBodySkin.Desaturate(saturation), coldColor.MultiplyRGBA(drawInfo.colorArmorLegs), lerpAmount);
					if (ignoredFrostburn && !drawInfo.headOnlyRender) {
						if (Main.rand.NextBool(4) && drawInfo.shadow == 0f) {
							Dust dust = Dust.NewDustDirect(
								drawInfo.Position - Vector2.One * 2,
								drawInfo.drawPlayer.width + 4,
								drawInfo.drawPlayer.height + 4,
								DustID.IceTorch,
								drawInfo.drawPlayer.velocity.X * 0.4f,
								drawInfo.drawPlayer.velocity.Y * 0.4f,
								100,
								default,
								3f
							);
							dust.noGravity = true;
							dust.velocity *= 1.8f;
							dust.velocity.Y -= 0.5f;
							drawInfo.DustCache.Add(dust.dustIndex);
						}
					}
				}
			}
		}
		class Moonflower_Global_NPC : GlobalNPC {
			public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
				float rateMod = 1 + player.GetModPlayer<Moonflower_Player>().spawnRateBonus;
				if (rateMod != 0) spawnRate = (int)(spawnRate / rateMod);
				maxSpawns = (int)(maxSpawns * rateMod);
			}
		}
	}
}
