using Origins.Dev;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Exo_Weapon_Mount : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public static int BackID { get; private set; }
		public static int BuffTime => 2 * 60;
		public override void SetStaticDefaults() {
			BackID = Item.backSlot;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 32);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.OriginPlayer();
			int oldSelected = originPlayer.exoWeaponMountCurrentWeapon;
			if (originPlayer.exoWeaponMountCurrentWeapon.TrySet(player.selectedItem)) {
				SoundEngine.PlaySound(SoundID.Item127.WithVolume(0.5f));
				SoundEngine.PlaySound(SoundID.Item113.WithPitch(2f).WithVolume(0.5f));
				SoundEngine.PlaySound(SoundID.Item89.WithVolume(0.5f));
				int buff = ModContent.BuffType<Exo_Weapon_Mount_Buff>();
				if (originPlayer.exoWeaponMountCurrentWeapon == originPlayer.exoWeaponMountLastWeapon) {
					int buffIndex = player.FindBuffIndex(buff);
					if (buffIndex != -1) {
						player.DelBuff(buffIndex);
						return;
					}
				}
				player.AddBuff(buff, BuffTime);
				originPlayer.exoWeaponMountLastWeapon = oldSelected;
			}
		}
	}
	public class Exo_Weapon_Mount_Buff : ModBuff {
		public override void Update(Player player, ref int buffIndex) {
			player.GetDamage(DamageClass.Generic) += 0.25f;
		}
	}
}
