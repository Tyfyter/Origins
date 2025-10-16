using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Journal;
using Origins.LootConditions;
using Origins.Music;
using Origins.NPCs.Brine.Boss;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Other.Consumables {
	public class Crown_Jewel : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string[] Categories => [
			WikiCategories.PermaBoost
		];
		public string EntryName => "Origins/" + typeof(Crown_Jewel_Entry).Name;
		public class Crown_Jewel_Entry : JournalEntry {
			public override string TextKey => "Crown_Jewel";
			public override JournalSortIndex SortIndex => new("Brine_Pool_And_Lost_Diver", 7);
		}
		static List<int> bosses = [];
		public override void Unload() => bosses = null;
		public static int ShaderID { get; private set; }
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this, "");
			GameShaders.Armor.BindShader(Type, new ArmorShaderData(
				Mod.Assets.Request<Effect>("Effects/Item_Caustics"),
				"Crown_Jewel"
			))
			.UseImage(TextureAssets.Extra[193]);
			ShaderID = GameShaders.Armor.GetShaderIdFromItemId(Type);
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Lime;
			Item.value = Item.buyPrice(gold: 5);
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.width = 16;
			Item.height = 26;
			Item.accessory = false;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = SoundID.Item29.WithPitch(-1f).WithVolume(0.2f);
			Item.consumable = true;
		}
		public override void GrabRange(Player player, ref int grabRange) {
			if (Item.newAndShiny) grabRange = 0;
		}
		public override void Update(ref float gravity, ref float maxFallSpeed) {
			if (Item.newAndShiny) {
				Item.velocity.X *= 0.97f;
				if (Item.velocity.Y < 0) Item.velocity.Y *= 0.97f;
				gravity = 0.04f;
				maxFallSpeed = 2.5f;
			}
		}
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
			SpriteBatchState state = spriteBatch.GetState();
			try {
				spriteBatch.Restart(state, sortMode: SpriteSortMode.Immediate);
				Texture2D texture = TextureAssets.Extra[193].Value;
				DrawData data = new() {
					texture = texture,
					position = Item.Center - Main.screenPosition,
					color = Color.Lime,
					rotation = 0f,
					scale = new Vector2(scale),
					shader = Item.dye,
					origin = texture.Size() * 0.5f
				};
				GameShaders.Armor.ApplySecondary(Item.dye, null, data);
				data.Draw(spriteBatch);
			} finally {
				spriteBatch.Restart(state);
			}
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
					if (OriginsSets.NPCs.BossKillCounterOverrider.GetIfInRange(bosses[i]) is Func<bool> predicate) {
						if (predicate()) result++;
					} else if (Main.BestiaryTracker.Kills.GetKillCount(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[bosses[i]]) > 0) {
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
	public class CJ_Music_Scene_Effect : CoolItemMusicSceneEffect<Crown_Jewel> {
		public override int Music => Origins.Music.CrownJewel;
	}
	public class Crown_Jewel_Toggle : BuilderToggle {
		public override string HoverTexture => Texture;
		public override bool Active() => OriginPlayer.LocalOriginPlayer?.crownJewel ?? false;
		public override string DisplayValue() {
			string baseValue = Language.GetOrRegister($"Mods.Origins.Items.{nameof(Crown_Jewel)}.Toggle_" + (CurrentState == 0 ? "On" : "Off")).Value;
			int slainBossesCount = Crown_Jewel.SlainBossesCount;
			baseValue += "\n" + Language.GetOrRegister($"Mods.Origins.Items.{nameof(Crown_Jewel)}.ScaledEffectTooltip").Format(slainBossesCount);
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
	public class Crown_Jewel_Drop() : DropInstancedPerClient(ModContent.ItemType<Crown_Jewel>(), 1, 1, 1, null) {
		//*
		public override bool CanDropForPlayer(Player player) => !player.OriginPlayer().crownJewel;
		/*/
		public override bool CanDropForPlayer(Player player) => true;
		//*/
	}
}
