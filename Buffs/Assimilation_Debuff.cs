﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.NPCs;
using Origins.World.BiomeData;
using PegasusLib;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Corrupt_Assimilation : AssimilationDebuff {
		public override void Update(Player player, float percent) {
			player.GetDamage(DamageClass.Generic) -= percent / 2;
			player.GetAttackSpeed(DamageClass.Generic) -= percent / 2;
			player.GetCritChance(DamageClass.Generic) -= percent * 10;
			player.GetKnockback(DamageClass.Generic) -= percent / 2;
		}
	}
	public class Crimson_Assimilation : AssimilationDebuff {
		public override void Update(Player player, float percent) {
			if (Main.rand.NextFloat(0.5f, 2f) < percent) {
				if (Main.rand.NextBool(2)) {
					player.AddBuff(BuffID.Confused, Main.rand.Next(24, 48) * (1 + (int)percent));
				} else {
					player.AddBuff(BuffID.Bleeding, Main.rand.Next(48, 138) * (1 + (int)percent));
				}
			}
		}
	}
	public class Defiled_Assimilation : AssimilationDebuff {
		public override string BestiaryStatTexture => "Origins/UI/WorldGen/IconEvilDefiled";
		public override void Update(Player player, float percent) {
			if (percent >= 0.125 /*&& Main.rand.NextFloat(0, 200) < percent - 0.125*/) {
				player.AddBuff(BuffID.Weak, 300);
			}
			if (percent >= 0.35 /*&& Main.rand.NextFloat(0, 200) < percent - 0.35*/) {
				player.AddBuff(BuffID.BrokenArmor, 180);
			}
			if (percent >= 0.5) {
				player.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), (int)(((percent - 0.5) / (1 - 0.5)) * 14));
			}
			player.OriginPlayer().GetAssimilation(AssimilationType).Percent += percent * player.InModBiome<Defiled_Wastelands>().ToDirectionInt() * 0.0000444f;
		}
	}
	public class Riven_Assimilation : AssimilationDebuff {
		public override string BestiaryStatTexture => "Origins/UI/WorldGen/IconEvilRiven";
		public override void OnChanged(Player player, float oldValue, float newValue) {
			if (oldValue < newValue) player.OriginPlayer().timeSinceRivenAssimilated = 0;
		}
		public override void Update(Player player, float percent) {
			OriginPlayer.InflictTorn(player, 60, player.OriginPlayer().timeSinceRivenAssimilated < 5 ? 5 : 1000, percent * ServerSideAccessibility.Instance.RivenAsimilationMultiplier, true);
		}
	}
	public static class AssimilationLoader {
		public static List<AssimilationDebuff> Debuffs { get; private set; } = [];
		public static void Register(AssimilationDebuff debuff) {
			if (debuff.AssimilationType != -1) return;
			debuff.AssimilationType = Debuffs.Count;
			Debuffs.Add(debuff);
		}
		public static void Load() { }
		public static void Unload() {
			Debuffs = null;
		}
		public static AssimilationInfo GetAssimilation(this Player player, int type) => player.OriginPlayer().GetAssimilation(type);
		public static AssimilationInfo GetAssimilation<TDebuff>(this Player player) where TDebuff : AssimilationDebuff => player.OriginPlayer().GetAssimilation<TDebuff>();
		public static void AddNPCAssimilation<TDebuff>(int type, AssimilationAmount amount) where TDebuff : AssimilationDebuff {
			BiomeNPCGlobals.NPCAssimilationAmounts.TryAdd(type, []);
			BiomeNPCGlobals.NPCAssimilationAmounts[type].TryAdd(ModContent.GetInstance<TDebuff>().AssimilationType, amount);
		}
		public static void AddProjectileAssimilation<TDebuff>(int type, AssimilationAmount amount) where TDebuff : AssimilationDebuff {
			BiomeNPCGlobals.ProjectileAssimilationAmounts.TryAdd(type, []);
			BiomeNPCGlobals.ProjectileAssimilationAmounts[type].TryAdd(ModContent.GetInstance<TDebuff>().AssimilationType, amount);
		}
		public static void AddDebuffAssimilation<TDebuff>(int type, AssimilationAmount amount) where TDebuff : AssimilationDebuff {
			BiomeNPCGlobals.DebuffAssimilationAmounts.TryAdd(type, []);
			BiomeNPCGlobals.DebuffAssimilationAmounts[type].TryAdd(ModContent.GetInstance<TDebuff>().AssimilationType, amount);
		}
		public static void AddAssimilation<TDebuff>(this ModNPC npc, AssimilationAmount amount) where TDebuff : AssimilationDebuff => AddNPCAssimilation<TDebuff>(npc.Type, amount);
		public static void AddAssimilation<TDebuff>(this ModProjectile projectile, AssimilationAmount amount) where TDebuff : AssimilationDebuff => AddProjectileAssimilation<TDebuff>(projectile.Type, amount);
	}
	[Autoload(false)]
	public class AssimilationStat(AssimilationDebuff debuff) : BestiaryCombatStat {
		public override string Texture => debuff.BestiaryStatTexture;
		public override string Name => $"{debuff.Name}_BestiaryStat";
		public override LocalizedText DisplayName => debuff.DisplayName;
		AssimilationAmount? GetValue(NPC npc) {
			AssimilationAmount value;
			Dictionary<int, AssimilationAmount> dict;
			if (BiomeNPCGlobals.assimilationDisplayOverrides.TryGetValue(npc.netID, out dict) && dict.TryGetValue(debuff.AssimilationType, out value)) return value;
			if (BiomeNPCGlobals.NPCAssimilationAmounts.TryGetValue(npc.netID, out dict) && dict.TryGetValue(debuff.AssimilationType, out value)) return value;
			if (BiomeNPCGlobals.assimilationDisplayOverrides.TryGetValue(npc.type, out dict) && dict.TryGetValue(debuff.AssimilationType, out value)) return value;
			if (BiomeNPCGlobals.NPCAssimilationAmounts.TryGetValue(npc.type, out dict) && dict.TryGetValue(debuff.AssimilationType, out value)) return value;
			return null;
		}
		public override string GetDisplayText(NPC npc) {
			return GetValue(npc)?.GetText()?.ToString();
		}
		public override bool ShouldDisplay(NPC npc) {
			return GetValue(npc).HasValue;
		}
	}
	public abstract class AssimilationDebuff : ModBuff {
		public virtual LocalizedText DeathMessage => this.GetLocalization(nameof(DeathMessage), PrettyPrintName);
		public virtual string BestiaryStatTexture => Texture + "_BestiaryStat";
		public int AssimilationType { get; internal set; } = -1;
		public override void Load() {
			AssimilationLoader.Register(this);
			Mod.AddContent(new AssimilationStat(this));
		}
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
			_ = DeathMessage.Value;
		}
		public virtual void Update(Player player, float percent) { }
		public virtual void OnChanged(Player player, float oldValue, float newValue) { }
		public sealed override void Update(Player player, ref int buffIndex) {
			AssimilationInfo info = player.OriginPlayer().GetAssimilation(AssimilationType);
			float percent = info.EffectivePercent;
			if (percent >= OriginPlayer.assimilation_max) {
				player.KillMe(new KeyedPlayerDeathReason() {
					Key = DeathMessage.Key
				}, 40, 0);
			}
			Update(player, percent);

		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			float percent = Main.LocalPlayer.OriginPlayer().GetAssimilation(AssimilationType).Percent;

			string text = $"{percent * 100:#0}%";
			float alpha = Main.buffAlpha[buffIndex];
			Color color = new(alpha, alpha, alpha, alpha);
			spriteBatch.DrawString(
				FontAssets.ItemStack.Value,
				text,
				drawParams.TextPosition,
				color,
				0f,
				Vector2.Zero,
				0.8f,
				SpriteEffects.None,
			0f);
		}
	}
	public class AssimilationGlobalBuff : GlobalBuff {
		public override void Update(int type, Player player, ref int buffIndex) {
			if (BiomeNPCGlobals.ProjectileAssimilationAmounts.TryGetValue(type, out Dictionary<int, AssimilationAmount> assimilationValues)) {
				foreach (KeyValuePair<int, AssimilationAmount> value in assimilationValues) {
					player.GetAssimilation(value.Key).Percent += value.Value.GetValue(null, player);
				}
			}
		}
		/*public override void ModifyBuffText(int type, ref string buffName, ref string tip, ref int rare) {
			tip += "\n" + BuffID.Search.GetName(type);
		}*/
	}
	public class AssimilationInfo(AssimilationDebuff type, Player player) {
		float currentValue;
		public float Percent {
			get => currentValue;
			set {
				Type.OnChanged(Player, Percent, value);
				currentValue = value;
			}
		}
		float currentMultiplier = 1f;
		float nextMultiplier = 1f;
		public float EffectivePercent => Percent * currentMultiplier;
		public AssimilationDebuff Type { get; } = type;
		public Player Player { get; } = player;
		public void ResetEffects() {
			currentMultiplier = nextMultiplier;
			nextMultiplier = 1f;
		}
		public void AddMultiplier(float multiplier) {
			currentMultiplier *= multiplier;
			nextMultiplier *= multiplier;
		}
	}
}
