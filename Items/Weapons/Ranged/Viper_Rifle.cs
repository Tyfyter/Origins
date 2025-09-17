using Origins.Buffs;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Magic;
using Origins.Projectiles;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Viper_Rifle : ModItem, ICustomWikiStat {
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				global.viperEffect = true;
				global.SetUpdateCountBoost(proj, global.UpdateCountBoost + 2);
				if (contextArgs.Contains("barrel")) {
					proj.extraUpdates = 19;
					proj.timeLeft = 20;
				}
			});
			Origins.AddGlowMask(this);
			// Fixing things that Vanilla doesn't mark as debuffs because players can't get them, so that they cause crits.
			Main.debuff[BuffID.Frostburn2] = true;
			Main.debuff[BuffID.OnFire3] = true;
			Main.debuff[BuffID.Oiled] = true;
			Main.debuff[BuffID.Daybreak] = true;
			Main.debuff[BuffID.Midas] = true;
			Main.debuff[BuffID.DryadsWardDebuff] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Gatligator);
			Item.damage = 58;
			Item.crit = 11;
			Item.knockBack = 7.75f;
			Item.useAnimation = Item.useTime = 23;
			Item.width = 100;
			Item.height = 28;
			Item.autoReuse = false;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = Origins.Sounds.HeavyCannon;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 26)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override Vector2? HoldoutOffset() => new Vector2(-6, 0);
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 unit = Vector2.Normalize(velocity);
			position += unit * 16;
			float dist = 80 - velocity.Length();
			position += unit * dist;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Vector2 unit = Vector2.Normalize(velocity);
			float dist = 80 - velocity.Length();
			position -= unit * dist;
			EntitySource_ItemUse_WithAmmo barrelSource = new(source.Player, source.Item, source.AmmoItemIdUsed, OriginExtensions.MakeContext(source.Context, OriginGlobalProj.no_multishot_context, "barrel"));
			OriginGlobalProj.killLinkNext = Projectile.NewProjectile(barrelSource, position, unit * (dist / 20), type, damage, knockback, player.whoAmI);
			return true;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Name == "Tooltip1" && ModLoader.HasMod("CritRework")) tooltips.RemoveAt(i);
			}
		}
	}
	public class Viper_Rifle_Crit_Type : CritType<Viper_Rifle> {
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) {
			for (int i = 0; i < target.buffType.Length; i++) {
				if (Main.debuff[target.buffType[i]] && target.buffType[i] != Toxic_Shock_Debuff.ID && target.buffType[i] != Toxic_Shock_Strengthen_Debuff.ID) {
					return true;
				}
			}
			return false;
		}
		// high crit multiplier because it's losing random crits instead of gaining consistent crits, less than 2x because it's gaining crit damage bonuses
		public override float CritMultiplier(Player player, Item item) => 1.8f;
	}
}
