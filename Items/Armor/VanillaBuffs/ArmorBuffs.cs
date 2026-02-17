using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.VanillaBuffs {
	public abstract class ArmorBuffs : GlobalItem, ILocalizedModType {
		public string LocalizationCategory => "VanillaBuffs";
		public override bool InstancePerEntity => base.InstancePerEntity;
		protected override bool CloneNewInstances => true;
		public override GlobalItem Clone(Item from, Item to) => this;
		public abstract int HeadItem { get; }
		public abstract int BodyItem { get; }
		public abstract int LegsItem { get; }
		public virtual bool Enabled => true;
		public LocalizedText HeadTooltip { get; private set; }
		public LocalizedText BodyTooltip { get; private set; }
		public LocalizedText LegsTooltip { get; private set; }
		public LocalizedText SetTooltip { get; private set; }
		public override void SetStaticDefaults() {
			SetTooltip = this.GetLocalization("Set");
			if (HeadItem > ItemID.None) HeadTooltip = this.GetLocalization("Head", () => "");
			if (BodyItem > ItemID.None) BodyTooltip = this.GetLocalization("Body", () => "");
			if (LegsItem > ItemID.None) LegsTooltip = this.GetLocalization("Legs", () => "");
		}
		public sealed override bool AppliesToEntity(Item item, bool lateInstantiation) {
			if (item.type == HeadItem) return true;
			if (item.type == BodyItem) return true;
			if (item.type == LegsItem) return true;
			return false;
		}
		public sealed override string IsArmorSet(Item head, Item body, Item legs) {
			if (!Enabled) return "";
			if (HeadItem > ItemID.None && head.type != HeadItem) return "";
			if (BodyItem > ItemID.None && body.type != BodyItem) return "";
			if (LegsItem > ItemID.None && legs.type != LegsItem) return "";
			return Name;
		}
		public sealed override void UpdateArmorSet(Player player, string set) {
			if (set != Name) return;
			if (!string.IsNullOrWhiteSpace(SetTooltip.Value)) player.setBonus += "\n" + SetTooltip.Value;
			UpdateArmorSet(player);
		}
		static Regex afterPrefix = new("^{after:(\\w+)}", RegexOptions.Compiled);
		public sealed override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (!Enabled) return;
			void AddTooltip(LocalizedText text) {
				string[] lines = text.Value.Split('\n');
				for (int i = 0; i < lines.Length; i++) {
					Match match = afterPrefix.Match(lines[i]);
					if (match.Success) {
						tooltips.Insert(match.Groups[1].Value, "OriginsBuff" + i, lines[i][match.Value.Length..]);
					} else {
						tooltips.Add("OriginsBuff" + i, lines[i]);
					}
				}
			}
			if (item.type == HeadItem) AddTooltip(HeadTooltip);
			else if (item.type == BodyItem) AddTooltip(BodyTooltip);
			else if (item.type == LegsItem) AddTooltip(LegsTooltip);
		}
		public sealed override void UpdateEquip(Item item, Player player) {
			if (item.type == HeadItem) UpdateEquip(ArmorPiece.Head, item, player);
			else if (item.type == BodyItem) UpdateEquip(ArmorPiece.Body, item, player);
			else if (item.type == LegsItem) UpdateEquip(ArmorPiece.Legs, item, player);
		}
		public virtual void UpdateArmorSet(Player player) { }
		public virtual void UpdateEquip(ArmorPiece piece, Item item, Player player) { }
		public enum ArmorPiece {
			Head,
			Body,
			Legs
		}
	}
}
