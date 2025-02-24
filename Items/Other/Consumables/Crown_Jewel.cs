using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Other.Consumables {
	public class Crown_Jewel : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"PermaBoost"
		];
		static List<int> bosses = [];
		public override void Unload() => bosses = null;
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this, "");
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.buyPrice(gold: 5);
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.width = 16;
			Item.height = 26;
			Item.accessory = false;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = SoundID.Item3;
			Item.consumable = true;
		}
		public override bool? UseItem(Player player) {
			ref bool crownJewel = ref player.GetModPlayer<OriginPlayer>().crownJewel;
			if (crownJewel) return false;
			crownJewel = true;
			return true;
		}
		public override void AddRecipes() {
			for (int i = 0; i < NPCLoader.NPCCount; i++) {
				if (ContentSamples.NpcsByNetId.TryGetValue(i, out NPC npc) && (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[i])) {
					List<DropRateInfo> drops = Main.ItemDropsDB.GetRulesForNPCID(i).GetDropRates();
					for (int j = 0; j < drops.Count; j++) {
						if (ItemID.Sets.BossBag[drops[j].itemId] && !ItemID.Sets.PreHardmodeLikeBossBag[drops[j].itemId]) {
							bosses.Add(i);
							break;
						}
					}
				}
			}
		}
		public static int SlainBossesCount {
			get {
				int result = 0;
				for (int i = 0; i < bosses.Count; i++) {
					if (Main.BestiaryTracker.Kills.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[bosses[i]]) > 0) {
						result++;
					}
				}
				return result;
			}
		}
		public static void UpdateEffect(OriginPlayer originPlayer) {
			Player player = originPlayer.Player;
			int slainBossesCount = SlainBossesCount;

			//stats here
			originPlayer.explosiveSelfDamage -= 0.01f * slainBossesCount;
			player.GetDamage(DamageClass.Generic) += 0.01f * slainBossesCount;
			player.GetCritChance(DamageClass.Generic) += 1f * slainBossesCount;
		}
	}
	public class Crown_Jewel_Toggle : BuilderToggle {
		public override string HoverTexture => Texture;
		public override bool Active() => OriginPlayer.LocalOriginPlayer?.crownJewel ?? false;
		public override string DisplayValue() {
			string baseValue = Language.GetOrRegister($"Mods.Origins.Items.{nameof(Crown_Jewel)}.Toggle_" + (CurrentState == 0 ? "On" : "Off")).Value;
			int slainBossesCount = Crown_Jewel.SlainBossesCount;
			baseValue += "\n" + Language.GetOrRegister($"Mods.Origins.Items.{nameof(Crown_Jewel)}.BossesSlain").Format(slainBossesCount);
			return baseValue;
		}
		public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) {
			drawParams.Frame.Y = 0;
			drawParams.Frame.Height = 18;
			switch (CurrentState) {
				case 1://disabled
				drawParams.Color = drawParams.Color.MultiplyRGB(Color.Gray);
				break;
			}
			return true;
		}
		public override bool DrawHover(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) {
			drawParams.Frame.Y = 20;
			drawParams.Frame.Height = 18;
			return true;
		}
	}
}
