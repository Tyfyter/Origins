using Microsoft.Xna.Framework;
using Origins.Dev;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Petrified_Prickly_Pear : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "ManaShielding"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(180, 180, 180),
				new Color(81, 52, 161),
				new Color(22, 18, 33)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(
				32, 24,
				BuffID.WellFed,
				60 * 60 * 5
			);
		}
        public override bool ConsumeItem(Player player) {
            player.AddBuff(Petrified_Prickly_Pear_Buff.ID, Item.buffTime);
            return true;
        }
    }
    public class Petrified_Prickly_Pear_Buff : ModBuff {
        public override string Texture => "Origins/Buffs/Food/Petrified_Prickly_Pear_Buff";
        public static int ID { get; private set; }
        public override void SetStaticDefaults() {
            ID = Type;
        }
        public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().manaShielding += 0.25f;
        }
    }
}
