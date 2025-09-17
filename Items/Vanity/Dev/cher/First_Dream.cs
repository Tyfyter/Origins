using Origins.Items.Other.Dyes;
using Origins.Layers;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Origins.Items.Vanity.Dev.cher {
	public class First_Dream : ModItem {
		protected override bool CloneNewInstances => true;
		[CloneByReference]
		readonly List<First_Dream_Mode> modes = [];
		[CloneByReference]
		readonly List<(int id, Action<int> func)> setValues = [];
		static readonly List<VisualEffect> visualEffects = [];
		int mode = 0;
		public override void Load() {
			On_ItemSlot.SwapVanityEquip += On_ItemSlot_SwapVanityEquip;
			On_Player.UpdateVisibleAccessory += On_Player_UpdateVisibleAccessory;
			int AddTexture(string name, EquipType equipType, params Action<int>[] sets) {
				string path = "Origins/Items/Vanity/Dev/cher/" + name;
				int id = EquipLoader.AddEquipTexture(Mod, path, equipType, name: name);
				for (int i = 0; i < sets.Length; i++) setValues.Add((id, sets[i]));
				if (ModContent.HasAsset(path + "_Glow")) setValues.Add((id, id => Accessory_Glow_Layer.AddGlowMask(equipType, id, path + "_Glow")));
				return id;
			}
			modes.Add(new("Chrersis", new(
				headSlot: AddTexture("Chrersis_Helmet_Head", EquipType.Head, id => ArmorIDs.Head.Sets.DrawHead[id] = false),
				bodySlot: AddTexture("Chrersis_Breastplate_Body", EquipType.Body, id => ArmorIDs.Body.Sets.HidesTopSkin[id] = true),
				legSlot: AddTexture("Chrersis_Greaves_Legs", EquipType.Legs, id => ArmorIDs.Legs.Sets.HidesBottomSkin[id] = true)
			)));
			modes.Add(new("Diver", new(
				headSlot: AddTexture("Diver_Helmet_Head", EquipType.Head),
				bodySlot: AddTexture("Diver_Breastplate_Body", EquipType.Body),
				legSlot: AddTexture("Diver_Greaves_Legs", EquipType.Legs)
			)));
			modes.Add(new("ByteLoss", new(
				headSlot: AddTexture("Bytelost_Ears_Head", EquipType.Head, id => ArmorIDs.Head.Sets.DrawFullHair[id] = true),
				faceSlot: AddTexture("Bytelost_Glasses_Head", EquipType.Face, id => ArmorIDs.Face.Sets.DrawInFaceUnderHairLayer[id] = true),
				tailSlot: AddTexture("Bytelost_Tail", EquipType.Back, id => ArmorIDs.Back.Sets.DrawInTailLayer[id] = true)
			),
			static player => {
				if (Main.rand.NextBool(180)) Main.rand.Next(visualEffects).Enable(player);
			}
			));

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
			if (inv[slot]?.ModItem is not First_Dream firstDream || ItemSlot.ShiftInUse) {
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
			for (int i = 0; i < setValues.Count; i++) setValues[i].func(setValues[i].id);
			setValues.Clear();
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.vanity = true;
			Item.rare = ItemRarityID.Cyan;
			Item.value = Item.sellPrice(gold: 1);
		}
		void SetNameOverride() {
			Item.SetNameOverride($"{DisplayName.Value} ({this.GetLocalization($"Mode_{modes[mode].Name}")})");
		}
		public override bool CanRightClick() => !ItemSlot.ShiftInUse;
		public override void RightClick(Player player) {
			Item.stack++;
			mode++;
			if (mode >= modes.Count) mode = 0;
		}
		public override void UpdateInventory(Player player) => SetNameOverride();
		public override void UpdateVanity(Player player) {
			SetNameOverride();
			modes[mode].UpdateVanity?.Invoke(player);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			SetNameOverride();
			if (!hideVisual) UpdateVanity(player);
		}
		public override void Update(ref float gravity, ref float maxFallSpeed) => SetNameOverride();
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].FullName == "Terraria/SocialDesc") {
					tooltips.RemoveAt(i);
					string[] lines = Tooltip.Value.Split('\n');
					for (int j = 0; j < lines.Length; j++) {
						tooltips.Insert(i + j, new(Mod, "Tooltip" + j, lines[j]));
					}
					break;
				}
			}
		}
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
		public record struct First_Dream_Mode(string Name, PlayerSlotSet Slots, Action<Player> UpdateVanity = null);
		public abstract class VisualEffect : VisualEffectPlayer.VisualEffect, ILoadable {
			public void Load(Mod mod) {
				visualEffects.Add(this);
			}
			public void Unload() { }
			public int time;
			protected abstract void Setup(Player player);
			public void Enable(Player player) {
				VisualEffect newEffect = (VisualEffect)MemberwiseClone();
				newEffect.Setup(player);
				newEffect.active = true;
				player.GetModPlayer<VisualEffectPlayer>().effects.Add(newEffect);
			}
			public override void ResetEffects() {
				if (time.Cooldown()) active = false;
			}
		}
		public class RasterizeVisualEffect : VisualEffect {
			protected override void Setup(Player player) {
				time = Main.rand.Next(30, 180);
			}
			public override void FrameEffects(Player player) {
				player.OriginPlayer().visualRasterizedTime = 15;
			}
		}
		public class RandomShaderVisualEffect : VisualEffect {
			int shader;
			protected override void Setup(Player player) {
				time = Main.rand.Next(60, 240);
				shader = Main.rand.Next(1, OriginsSets.Misc.ArmorShaderDataCount);
			}
			public override bool SetForcedShader() {
				Origins.forcePlayerShader = shader;
				Origins.keepPlayerShader = Anti_Gray_Dye.ShaderID;
				return true;
			}
			public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
				drawInfo.cHead = Anti_Gray_Dye.ShaderID;
				drawInfo.cFace = Anti_Gray_Dye.ShaderID;
				drawInfo.cTail = Anti_Gray_Dye.ShaderID;
			}
		}
		public class FlipVisualEffect : VisualEffect {
			protected override void Setup(Player player) {
				time = Main.rand.Next(60, 240);
			}
			public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
				drawInfo.playerEffect ^= Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically;
			}
		}
		public class LagVisualEffect : VisualEffect {
			Vector2 pos;
			protected override void Setup(Player player) {
				time = Main.rand.Next(3, 15);
				pos = player.position;
			}
			public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
				drawInfo.Position = pos;
			}
		}
	}
}
