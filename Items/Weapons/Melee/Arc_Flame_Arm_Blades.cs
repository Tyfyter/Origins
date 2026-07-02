using Humanizer;
using Origins.Buffs;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Vanity.Dev;
using Origins.Items.Weapons.Melee;
using Origins.NPCs;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Rei_Set : DevSet<Arc_Flame_Arm_Blades> {
		public override IEnumerable<ItemTypeDropRuleWrapper> GetDrops() {/*
			yield return ModContent.ItemType<First_Dream>();
			yield return ModContent.ItemType<Chew_Toy>();*/
			yield return new(ItemDropRule.ByCondition(DropConditions.HardmodeBossBag, ModContent.ItemType<Arc_Flame_Arm_Blades>()));
		}
	}
	public class Arc_Flame_Arm_Blades : ModItem, ICustomWikiStat {
		public override string Texture => typeof(Broken_Fiberglass_Sword).GetDefaultTMLName();
		public string[] Categories => [
			WikiCategories.Sword
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Terragrim);/*
			Item.damage = 18;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;*/
			Item.width = 24;
			Item.height = 26;/*
			Item.useTime = 14;
			Item.useAnimation = 14;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6;
			Item.autoReuse = true;
			Item.useTurn = true;*/
			//Item.shootSpeed = 50;
			Item.crit = 14;
			if (CritType.ModEnabled) Item.crit += 4;
			Item.shoot = ModContent.ProjectileType<Arc_Flame_Arm_Blades_Slash>();
			Item.rare = ItemRarityID.Cyan;
			//Item.UseSound = SoundID.Item1;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				switch (tooltips[i].Name) {
					case "CritChance":
					tooltips[i] = new(Mod, "DebuffChance", this.GetLocalizedValue("CritTooltip").FormatWith(tooltips[i].Text.Split(' ')[0]));
					break;
					case "PrefixCritChance":
					tooltips[i].Text = this.GetLocalizedValue("CritTooltip").FormatWith(tooltips[i].Text.Split(' ')[0]);
					break;
				}
			}
			if (!CritType.ModEnabled) tooltips.Insert("Tooltip1", Language.GetText("Mods.Origins.CritType.Arc_Flame_Arm_Blades_Crit_Type"), "Tooltip2");
		}
	}
	public class Arc_Flame_Arm_Blades_Slash : ModProjectile {
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Terragrim}";
		static int[] debuffs = [];
		static RangeRandom rand;
		public override void SetStaticDefaults() {
			debuffs = [ModContent.BuffType<Arc_Burn>(), BuffID.OnFire3, BuffID.ShadowFlame]; // think of a fitting 4th debuff
			rand = new(Main.rand, 0, debuffs.Length);
			Main.projFrames[Type] = 28;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Terragrim);
		}
		public override void AI() {
			float num = 0f;
			if (Projectile.spriteDirection == -1)
				num = (float)Math.PI;

			if (++Projectile.frame >= Main.projFrames[Type])
				Projectile.frame = 0;

			Projectile.soundDelay--;
			if (Projectile.soundDelay <= 0) {
				SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
				Projectile.soundDelay = 12;
			}

			if (Main.myPlayer == Projectile.owner && Projectile.TryGetOwner(out Player player)) {
				Vector2 vector = player.RotatedRelativePoint(player.MountedCenter);
				if (player.channel && !player.noItems && !player.CCed) {
					float num44 = 1f;
					if (player.inventory[player.selectedItem].shoot == Type)
						num44 = player.inventory[player.selectedItem].shootSpeed * Projectile.scale;

					Vector2 vec2 = Main.MouseWorld - vector;
					vec2.Normalize();
					if (vec2.HasNaNs())
						vec2 = Vector2.UnitX * player.direction;

					vec2 *= num44;
					if (vec2.X != Projectile.velocity.X || vec2.Y != Projectile.velocity.Y)
						Projectile.netUpdate = true;

					Projectile.velocity = vec2;
				} else {
					Projectile.Kill();
				}

				Projectile.position = player.RotatedRelativePoint(player.MountedCenter, addGfxOffY: false) - Projectile.Size / 2f;
				Projectile.rotation = Projectile.velocity.ToRotation() + num;
				Projectile.spriteDirection = Projectile.direction;
				Projectile.timeLeft = 2;
				player.ChangeDir(Projectile.direction);
				player.heldProj = Projectile.whoAmI;
				player.SetDummyItemTime(2);
				player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction));
			}

			Vector2 vector21 = Projectile.Center + Projectile.velocity * 3f;
			Lighting.AddLight(vector21, 0.8f, 0.8f, 0.8f);
			if (Main.rand.NextBool(3)) {
				int num45 = Dust.NewDust(vector21 - Projectile.Size / 2f, Projectile.width, Projectile.height, DustID.Terragrim, Projectile.velocity.X, Projectile.velocity.Y, 100, Scale: 2f);
				Main.dust[num45].noGravity = true;
				Main.dust[num45].position -= Projectile.velocity;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (!CritType.ModEnabled) {
				if (target.HasBuff<Arc_Burn>()) {
					modifiers.CritDamage *= 5;
					modifiers.SetCrit();
				} else modifiers.DisableCrit();
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.HasBuff(Arc_Burn.ID)) target.DelBuff(target.FindBuffIndex(Arc_Burn.ID));
			else if (Main.rand.Next(100) > 100 - Projectile.CritChance) {
				rand.Reset();
				rand.Multiply(0, 1, 0.3);
				for (int i = 0; i < debuffs.Length; i++) {
					if (target.HasBuff(debuffs[i])) rand.Multiply(i, i + 1, 0.3);
				}
				target.AddBuff(debuffs[rand.Get()], 3 * 60);
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (Main.rand.Next(100) > 100 - Projectile.CritChance) {
				rand.Reset();
				rand.Multiply(0, 1, 0.3);
				for (int i = 0; i < debuffs.Length; i++) {
					if (target.HasBuff(debuffs[i])) rand.Multiply(i, i + 1, 0.3);
				}
				target.AddBuff(debuffs[rand.Get()], 3 * 60);
			}
		}
	}
	public class Arc_Flame_Arm_Blades_Crit_Type : CritType<Arc_Flame_Arm_Blades> {
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) => target.HasBuff(Arc_Burn.ID);
		public override float CritMultiplier(Player player, Item item) => 10;
	}
}
namespace Origins.Buffs {
	public class Arc_Burn : ModBuff {
		public override string Texture => typeof(Broken_Fiberglass_Sword).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) => npc.GetGlobalNPC<OriginGlobalNPC>().arcBurn = true;
		public override void Update(Player player, ref int buffIndex) => player.OriginPlayer().arcBurn = true;
	}
}
