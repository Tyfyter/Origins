using Humanizer;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Origins.Items.Armor.Vanity.Dev.cher {
	public class First_Dream : ModItem {
		protected override bool CloneNewInstances => true;
		[CloneByReference]
		readonly List<First_Dream_Mode> modes = [];
		[CloneByReference]
		readonly List<(int id, Action<int> func)> setValues = [];
		int mode = 0;
		public override void Load() {
			On_ItemSlot.SwapVanityEquip += On_ItemSlot_SwapVanityEquip;
			On_Player.UpdateVisibleAccessory += On_Player_UpdateVisibleAccessory;
			int AddTexture(string name, EquipType equipType) {
				return EquipLoader.AddEquipTexture(Mod, "Origins/Items/Armor/Vanity/Dev/cher/" + name, equipType, name: name);
			}
			int AddTextureWithSets(string name, EquipType equipType, params Action<int>[] sets) {
				int id = EquipLoader.AddEquipTexture(Mod, "Origins/Items/Armor/Vanity/Dev/cher/" + name, equipType, name: name);
				for (int i = 0; i < sets.Length; i++) {
					setValues.Add((id, sets[i]));
				}
				return id;
			}
			modes.Add(new("Chrersis", new(
				headSlot: AddTextureWithSets("Chrersis_Helmet_Head", EquipType.Head, id => ArmorIDs.Head.Sets.DrawHead[id] = false),
				bodySlot: AddTexture("Chrersis_Breastplate_Body", EquipType.Body),
				legSlot: AddTexture("Chrersis_Greaves_Legs", EquipType.Legs)
			)));
			modes.Add(new("Diver", new(
				headSlot: AddTexture("Diver_Helmet_Head", EquipType.Head),
				bodySlot: AddTexture("Diver_Breastplate_Body", EquipType.Body),
				legSlot: AddTexture("Diver_Greaves_Legs", EquipType.Legs)
			)));

			//Registers localizations
			for (int i = 0; i < modes.Count; i++) this.GetLocalization($"Mode_{modes[i].Name}");
		}
		private static void On_Player_UpdateVisibleAccessory(On_Player.orig_UpdateVisibleAccessory orig, Player self, int itemSlot, Item item, bool modded) {
			if (item?.ModItem is not First_Dream firstDream) {
				orig(self, itemSlot, item, modded);
				return;
			}
			firstDream.modes[firstDream.mode].Slots.Apply(self);
		}

		private static void On_ItemSlot_SwapVanityEquip(On_ItemSlot.orig_SwapVanityEquip orig, Item[] inv, int context, int slot, Player player) {
			if (inv[slot]?.ModItem is not First_Dream firstDream) {
				orig(inv, context, slot, player);
				return;
			}
			firstDream.mode++;
			if (firstDream.mode >= firstDream.modes.Count) firstDream.mode = 0;
			if (Main.netMode != NetmodeID.SinglePlayer && player is not null) {
				int baseSlot;
				switch (player.CurrentLoadoutIndex) {
					default:
					baseSlot = PlayerItemSlotID.Loadout1_Armor_0;
					break;
					case 1:
					baseSlot = PlayerItemSlotID.Loadout2_Armor_0;
					break;
					case 2:
					baseSlot = PlayerItemSlotID.Loadout3_Armor_0;
					break;
				}
				NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, baseSlot + slot);
			}
		}
		public override void SetStaticDefaults() {
			for (int i = 0; i < setValues.Count; i++) {
				setValues[i].func(setValues[i].id);
			}
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.vanity = true;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		void SetNameOverride() {
			Item.SetNameOverride(Lang.GetItemName(Item.type).Format(this.GetLocalization($"Mode_{modes[mode].Name}")));
		}
		public override void UpdateInventory(Player player) => SetNameOverride();
		public override void UpdateVanity(Player player) => SetNameOverride();
		public override void UpdateAccessory(Player player, bool hideVisual) => SetNameOverride();
		public override void SaveData(TagCompound tag) {
			tag["mode"] = mode;
		}
		public override void LoadData(TagCompound tag) {
			tag.TryGet("mode", out mode);
		}
		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)mode);
		}
		public override void NetReceive(BinaryReader reader) {
			mode = reader.ReadByte();
		}
		public record struct First_Dream_Mode(string Name, ItemSlotSet Slots);
	}
}
