using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Lunatics_Rune : ModItem {
		static readonly List<Option> options = [];
		public static int ChargeThreshold => 2 * 60;
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Type, new DrawAnimationSwitching(OriginsModIntegrations.CheckAprilFools, NoDrawAnimation.AtAll, new DrawAnimationRandom(3, 20)));
			AprilFoolsTextures.AddItem(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.mana = 120;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) { }
		public bool CheckMana(Player player) {
			player.manaRegenDelay = player.maxRegenDelay;
			float reduce = player.manaCost;
			float mult = 1;

			CombinedHooks.ModifyManaCost(player, Item, ref reduce, ref mult);
			int mana = Main.rand.RandomRound(Item.mana * reduce * mult / ChargeThreshold);

			if (player.statMana < mana) {
				CombinedHooks.OnMissingMana(player, Item, mana);
				if (player.statMana < mana && player.manaFlower)
					player.QuickMana();
			}

			if (player.statMana < mana) return false;
			CombinedHooks.OnConsumeMana(player, Item, mana);
			player.statMana -= mana;
			return true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.lunaticsRune = true;
			ref int charge = ref originPlayer.lunaticsRuneCharge;
			if (player.SyncedKeybinds().LunaticsRune.Current && (charge >= ChargeThreshold || CheckMana(player))) {
				originPlayer.lunaticsRuneRotation += 0.02f;
				charge.Warmup(ChargeThreshold);
				float moveMult = 1 - float.Pow(charge / (float)ChargeThreshold, 2);
				player.velocity *= moveMult * moveMult;
				player.gravity *= moveMult;
				originPlayer.moveSpeedMult *= moveMult * moveMult;
				if (player.velocity.Y == 0) player.velocity.Y = float.Epsilon;
			} else {
				if (charge >= ChargeThreshold && player.whoAmI == Main.myPlayer) {
					RangeRandom random = new(Main.rand, 0, options.Count);
					for (int i = 0; i < options.Count; i++) {
						random.Multiply(i, i + 1, options[i].GetWeight(player));
					}
					if (random.AnyWeight) {
						options[random.Get()].Trigger(player);
					}
				}
				charge = 0;
			}
		}
		public class Duplicates : BuffOption {
			public override int BuffType => ModContent.BuffType<Lunatics_Rune_Duplicates_Buff>();
			public override int BuffTime => 8 * 60;
		}
		public class Healing : BuffOption {
			public override int BuffType => BuffID.RapidHealing;
			public override int BuffTime => 8 * 60;
		}
		public abstract class BuffOption : Option {
			public abstract int BuffType { get; }
			public abstract int BuffTime { get; }
			public override double GetWeight(Player player) => player.HasBuff(BuffType) ? 0 : 1;
			public override void Trigger(Player player) {
				player.AddBuff(BuffType, BuffTime);
			}
		}
		public abstract class Option : ILoadable {
			public void Load(Mod mod) => options.Add(this);
			public void Unload() {}
			public virtual double GetWeight(Player player) => 1;
			public abstract void Trigger(Player player);
		}
	}

	public class Lunatics_Rune_Duplicates_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Item_7";
		public override void Update(Player player, ref int buffIndex) {
			player.EnableShadow<Lunatic_Shadow>();
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.lunaticDuplicates = true;
			originPlayer.lunaticDuplicateOpacity++;
			Min(ref originPlayer.lunaticDuplicateOpacity, player.buffTime[buffIndex]);
		}
	}
	public class Lunatic_Shadow : ShadowType {
		public static float Offset => 64;
		public override IEnumerable<ShadowType> SortAbove() => [PartialEffects];
		public override IEnumerable<ShadowData> GetShadowData(Player player, ShadowData from) {
			Vector2 position = from.Position;
			//from.Shadow = 0.5f;
			from.Position = position + Vector2.UnitX * Offset;
			yield return from;
			from.Position = position - Vector2.UnitX * Offset;
			yield return from;
		}
		public override void TransformDrawData(ref PlayerDrawSet drawInfo) {
			float opacity = Math.Min(drawInfo.drawPlayer.OriginPlayer().lunaticDuplicateOpacity / 30f, 1) * 0.5f;

			for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
				DrawData drawData = drawInfo.DrawDataCache[i];
				drawData.color *= opacity;
				drawInfo.DrawDataCache[i] = drawData;
			}
		}
	}
}
