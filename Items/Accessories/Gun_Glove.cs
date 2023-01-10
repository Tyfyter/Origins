using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.HandsOn)]
    public class Gun_Glove : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Gun Glove");
            Tooltip.SetDefault("shoots");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            sbyte handOnSlot = Item.handOnSlot;
            sbyte handOffSlot = Item.handOffSlot;
            Item.CloneDefaults(ItemID.YoYoGlove);
            Item.handOffSlot = handOffSlot;
            Item.handOnSlot = handOnSlot;
            Item.value = Item.buyPrice(gold: 10);
            Item.rare = ItemRarityID.Green;
            Item.damage = 10;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.shootSpeed = 5;
            Item.useAmmo = AmmoID.Bullet;
            Item.UseSound = SoundID.Item10;
        }
        public override void UpdateEquip(Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            originPlayer.gunGlove = true;
            originPlayer.gunGloveItem = Item;
        }
    }
}
