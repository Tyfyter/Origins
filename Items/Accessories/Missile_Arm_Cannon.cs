using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    //[AutoloadEquip(EquipType.HandsOn)]
    public class Missile_Armcannon : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Missile Armcannon");
            Tooltip.SetDefault("30% increased explosive throwing velocity\nIncreases attack speed of thrown explosives\nEnables autouse for all explosive weapons\nShoots rockets as you swing\n'Payload not included'");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            sbyte handOnSlot = Item.handOnSlot;
            sbyte handOffSlot = Item.handOffSlot;
            Item.CloneDefaults(ItemID.YoYoGlove);
            /*Item.handOffSlot = handOffSlot;
            Item.handOnSlot = handOnSlot;*/
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightRed;

            Item.damage = 10;
            Item.DamageType = DamageClasses.Explosive;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.shootSpeed = 5;
            Item.useAmmo = AmmoID.Rocket; //Doesnt work for some reason
            Item.UseSound = SoundID.Item10;
        }
        public override void UpdateEquip(Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            player.GetAttackSpeed(DamageClasses.Explosive) += 0.1f;
            originPlayer.destructiveClaws = true;
            originPlayer.explosiveThrowSpeed += 0.3f;
            originPlayer.gunGlove = true;
            originPlayer.gunGloveItem = Item;
        }
    }
}
