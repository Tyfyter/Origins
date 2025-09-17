using Origins.Dev;
using Origins.Items.Armor;
using Origins.Items.Vanity.Dev.KonoDioDa;
using Origins.Items.Pets;
using Origins.Items.Vanity.Dev;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Vanity.Dev.PlagueTexan {
	public class Plague_Texan_Set : DevSet<Plague_Texan_Mask> {
		public override IEnumerable<ItemTypeDropRuleWrapper> GetDrops() {
			yield return ModContent.ItemType<Plague_Texan_Mask>();
			yield return ModContent.ItemType<Plague_Texan_Jacket>();
			yield return ModContent.ItemType<Plague_Texan_Jeans>();
			yield return ModContent.ItemType<Plague_Texan_Sight>();
			yield return new(ItemDropRule.ByCondition(DropConditions.HardmodeBossBag, ModContent.ItemType<SceneYMK_Wings>()));
			yield return ModContent.ItemType<Chromatic_Scale>();
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Plague_Texan_Mask : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string ArmorSetName => "Plague_Texan_Vanity";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Plague_Texan_Jacket>();
		public int LegsItemID => ModContent.ItemType<Plague_Texan_Jeans>();
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
		}
		public override void SetDefaults() {
			Item.vanity = true;
			Item.rare = AltCyanRarity.ID;
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Plague_Texan_Jacket : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			ArmorIDs.Body.Sets.HidesHands[Item.bodySlot] = false;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.vanity = true;
			Item.rare = AltCyanRarity.ID;
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Plague_Texan_Jeans : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.vanity = true;
			Item.rare = AltCyanRarity.ID;
		}
	}
	public class Plague_Texan_Sight : ModItem {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			ID = Item.type;
		}
		public override void SetDefaults() {
			Item.accessory = true;
			Item.rare = AltCyanRarity.ID;
			Item.hasVanityEffects = true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.dangerSense = true;
			player.detectCreature = true;
			player.GetModPlayer<OriginPlayer>().plagueSightLight = true;
			if (!hideVisual) 				ApplyVisuals(player);
		}
		public static void ApplyVisuals(Player player) {
			player.GetModPlayer<OriginPlayer>().plagueSight = true;
			Color color = Color.Gold;
			if (OriginExtensions.IsDevName(player.name, 1)) 				color = new Color(43, 185, 255);
			Lighting.AddLight(player.Center + new Vector2(3 * player.direction, -6), color.ToVector3() / 3f);
		}
	}
}
