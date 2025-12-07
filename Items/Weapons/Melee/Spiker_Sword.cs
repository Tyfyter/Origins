using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Microsoft.Xna.Framework;
using Origins.Projectiles.Weapons;
namespace Origins.Items.Weapons.Melee {
	public class Spiker_Sword : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => [
            "Sword"
        ];
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 30;
			Item.DamageType = DamageClass.Melee;
			Item.width = 52;
			Item.height = 64;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 7.5f;
			Item.useTurn = true;
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			int buffType = ModContent.BuffType<Spiker_Counter_Debuff>();
			target.AddBuff(buffType, 120);
			int buffIndex = target.FindBuffIndex(buffType);
			if (buffIndex > -1 && target.buffTime[buffIndex] > 120 * 2) {
				Projectile.NewProjectileDirect(
					player.GetSource_OnHit(target, nameof(Spiker_Sword)),
					target.Center,
					Vector2.Zero,
					ModContent.ProjectileType<Defiled_Spike_Explosion>(),
					damageDone,
					hit.Knockback,
					player.whoAmI,
				7);
				target.DelBuff(buffIndex);
			}
		}
	}
	public class Spiker_Counter_Debuff : ModBuff {
		public override string Texture => typeof(Spiker_Sword).GetDefaultTMLName();
		public override bool ReApply(NPC npc, int time, int buffIndex) {
			npc.buffTime[buffIndex] += time;
			return true;
		}
	}
}
