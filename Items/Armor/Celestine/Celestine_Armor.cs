using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Armor.Celestine {
    [AutoloadEquip(EquipType.Head)]
	public class Celestine_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Celestine Helmet");
			Tooltip.SetDefault("10% increased melee and magic critical strike chance");
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.defense = 8;
            Item.rare = ItemRarityID.Purple;
        }
        public override void UpdateEquip(Player player) {
            player.GetCritChance(DamageClass.Melee) += 10;
            player.GetCritChance(DamageClass.Magic) += 10;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Celestine_Breastplate>() && legs.type == ModContent.ItemType<Celestine_Greaves>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "Critical strikes spawn buff boosters";
            player.GetModPlayer<OriginPlayer>().celestineSet = true;
        }
    }
    [AutoloadEquip(EquipType.Body)]
	public class Celestine_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Celestine Breastplate");
			Tooltip.SetDefault("15% increased melee and magic damage");
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.defense = 24;
            Item.rare = ItemRarityID.Purple;
        }
        public override void UpdateEquip(Player player) {
            player.GetDamage(DamageClass.Melee) += 0.15f;
            player.GetDamage(DamageClass.Magic) += 0.15f;
        }
	}
    [AutoloadEquip(EquipType.Legs)]
	public class Celestine_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Celestine Greaves");
			Tooltip.SetDefault("20% increased melee speed\nIncreased mana regeneration\n10% increased movement speed");
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.defense = 16;
            Item.rare = ItemRarityID.Purple;
        }
        public override void UpdateEquip(Player player) {
            player.GetAttackSpeed(DamageClass.Melee) += 0.2f;
            player.manaRegenBuff = true;
            player.moveSpeed+=0.1f;
        }
    }
    public class Celestine_Mana_Booster : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Celestine Mana Booster");
            Origins.celestineBoosters[0] = Item.type;
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5,4));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
			ItemID.Sets.ItemIconPulse[Item.type] = true;
			ItemID.Sets.ItemNoGravity[Item.type] = true;
		}
        public override void SetDefaults() {
            Item.color = new Color(0,174,174);
        }
        public override bool OnPickup(Player player) {
            player.AddBuff(ModContent.BuffType<Celestine_Mana_Boost_Buff>(), 600);
            return false;
        }
    }
    public class Celestine_Life_Booster : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Celestine Life Booster");
            Origins.celestineBoosters[1] = Item.type;
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5,4));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
			ItemID.Sets.ItemIconPulse[Item.type] = true;
			ItemID.Sets.ItemNoGravity[Item.type] = true;
		}
        public override void SetDefaults() {
            Item.color = new Color(255,64,33);
        }
        public override bool OnPickup(Player player) {
            player.AddBuff(ModContent.BuffType<Celestine_Life_Boost_Buff>(), 600);
            return false;
        }
    }
    public class Celestine_Damage_Booster : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Celestine Damage Booster");
            Origins.celestineBoosters[2] = Item.type;
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5,4));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
			ItemID.Sets.ItemIconPulse[Item.type] = true;
			ItemID.Sets.ItemNoGravity[Item.type] = true;
		}
        public override void SetDefaults() {
            Item.color = new Color(255,255,255);
        }
        public override bool OnPickup(Player player) {
            player.AddBuff(ModContent.BuffType<Celestine_Damage_Boost_Buff>(), 600);
            return false;
        }
    }
    public class Celestine_Mana_Boost_Buff : ModBuff {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Celestine Mana Boost");
            Description.SetDefault("Increased mana regeneration");
        }
        public override void Update(Player player, ref int buffIndex) {
            player.manaRegenCount+=3;
        }
    }
    public class Celestine_Life_Boost_Buff : ModBuff {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Celestine Life Boost");
            Description.SetDefault("Increased life regeneration");
        }
        public override void Update(Player player, ref int buffIndex) {
            player.lifeRegenCount+=3;
        }
    }
    public class Celestine_Damage_Boost_Buff : ModBuff {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Celestine Damage Boost");
            Description.SetDefault("15% increased damage");
        }
        public override void Update(Player player, ref int buffIndex) {
            player.GetDamage(DamageClass.Generic) *= 1.15f;
        }
    }
}
