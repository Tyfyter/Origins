using Origins.Buffs;
using Origins.CrossMod;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Felnum_Pickaxe : ModItem {
		public override void SetStaticDefaults() {
			Origins.DamageBonusScale[Type] = 1.5f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.DeathbringerPickaxe);
			Item.damage = 21;
			Item.DamageType = DamageClass.Melee;
			Item.pick = 75;
			Item.width = 36;
			Item.height = 24;
			Item.useTime = 13;
			Item.useAnimation = 22;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 40);
			Item.UseSound = SoundID.Item1;
			Item.rare = ItemRarityID.Green;
		}
        public override void AddRecipes() {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Felnum_Bar>(), 20)
            .AddTile(TileID.Anvils)
            .Register();
        }
        public override float UseTimeMultiplier(Player player) {
			return (player.pickSpeed - 1) * 0.75f + 1;
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			Static_Shock_Debuff.Inflict(target, Main.rand.Next(120, 210));
		}
	}
}
