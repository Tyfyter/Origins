using MonoMod.Cil;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Abyssal_Anchor : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"ManaShielding",
			"MagicBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 12);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Binding_Book>())
			.AddIngredient(ModContent.ItemType<Celestial_Stone_Mask>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.OriginPlayer();
			player.manaMagnet = true;
			player.magicCuffs = true;
			player.statManaMax2 += 20;
			originPlayer.manaShielding += 0.25f;
			originPlayer.abyssalAnchor = true;

			player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
			player.GetDamage(DamageClass.Generic) += 0.1f;
			player.GetCritChance(DamageClass.Generic) += 4f;
			player.lifeRegen += 4;
			player.statDefense += 8;
			player.pickSpeed -= 0.25f;
			player.GetKnockback(DamageClass.Summon).Base += 1;

			player.moveSpeed *= 0.75f;
			player.jumpSpeedBoost -= 2.2f;
		}
		internal static void IL_Player_WaterCollision(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After,
				i => i.MatchLdfld<Entity>(nameof(Entity.velocity)),
				i => i.MatchLdcR4(0.5f),
				i => i.MatchCall<Vector2>("op_Multiply")
			);
			c.EmitLdarg0();
			c.EmitDelegate((Vector2 velocity, Player player) => {
				if (player.OriginPlayer().abyssalAnchor && player.velocity.Y > 0 && player.wet) velocity.Y *= 2.3f;
				return velocity;
			});
		}
	}
}
