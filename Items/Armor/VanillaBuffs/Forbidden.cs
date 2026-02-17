using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.VanillaBuffs {
	public class Forbidden : ArmorBuffs {
		public override bool Enabled => OriginConfig.Instance.ForbiddenArmor;
		public override int HeadItem => ItemID.AncientBattleArmorHat;
		public override int BodyItem => ItemID.AncientBattleArmorShirt;
		public override int LegsItem => ItemID.AncientBattleArmorPants;
		public override void UpdateArmorSet(Player player) {
			player.OriginPlayer().buffedForbiddenSet = true;
		}
		public override void UpdateEquip(ArmorPiece piece, Item item, Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			switch (piece) {
				case ArmorPiece.Head:
				originPlayer.incantationProjSpeedBoost += 0.15f;
				break;
				case ArmorPiece.Body:
				player.GetCritChance(DamageClasses.Incantation) += 15f;
				break;
				case ArmorPiece.Legs:
				break;
			}
		}
		public class Ancient_Storm : GlobalProjectile {
			public override bool AppliesToEntity(Projectile projectile, bool lateInstantiation) => projectile.type == ProjectileID.SandnadoFriendly;
			public override void SetDefaults(Projectile projectile) {
				if (!OriginConfig.Instance.ForbiddenArmor) return;
				projectile.DamageType = DamageClass.MagicSummonHybrid;
			}
		}
	}
}
