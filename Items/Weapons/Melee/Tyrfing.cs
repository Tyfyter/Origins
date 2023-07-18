using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Items.Weapons.Melee {
	//this took seven and a half hours to make, now that's dedication
	public class Tyrfing : AnimatedModItem {

		protected override bool CloneNewInstances => true;
		internal static DrawAnimationManual animation;
		public override DrawAnimation Animation {
			get {
				animation.Frame = frame;
				return animation;
			}
		}
		public int charge = 0;
		internal int frame = 5;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Tyrfing");
			// Tooltip.SetDefault("Receives 50% higher damage bonuses\nHold right click to stab\n'Behold'");
			animation = new DrawAnimationManual(6);
			animation.Frame = 5;
			Main.RegisterItemAnimation(Item.type, animation);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 88;
			Item.DamageType = DamageClass.Melee;
			Item.width = 42;
			Item.height = 42;
			Item.useTime = 48;
			Item.useAnimation = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 9;
			Item.autoReuse = true;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Lime;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Valkyrum_Bar>(), 12);
			recipe.AddIngredient(ItemID.Excalibur, 1);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}

		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) {
			if (player.altFunctionUse == 2) knockback *= 2.1111111111111111111111111111112f;
		}

		public override bool CanUseItem(Player player) {
			if (player.altFunctionUse == 2) {
				Item.useStyle = ItemUseStyleID.Shoot;
				Item.shoot = ModContent.ProjectileType<Tyrfing_Stab>();
				Item.shootSpeed = 3.4f;
				Item.noUseGraphic = true;
				Item.noMelee = true;
				Item.UseSound = null;
			} else {
				Item.useStyle = ItemUseStyleID.Swing;
				Item.shoot = ModContent.ProjectileType<Tyrfing_Shard>();
				Item.shootSpeed = 6.5f;
				Item.noUseGraphic = false;
				Item.noMelee = false;
				Item.UseSound = SoundID.Item1;
			}
			return base.CanUseItem(player);
		}

		public override void HoldItem(Player player) {
			if (player.itemAnimation != 0 && player.altFunctionUse != 2) {
				player.GetModPlayer<OriginPlayer>().itemLayerWrench = true;
			}
		}

		public override void LoadData(TagCompound tag) {
			frame = tag.GetInt("frame");
		}
		public override void SaveData(TagCompound tag) {
			tag.Add("frame", frame);
		}

		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
			OriginExtensions.FixedUseItemHitbox(Item, player, ref hitbox, ref noHitbox);
			if (frame == 5) {
				hitbox = new Rectangle(0, 0, 0, 0);
			}
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage = damage.Scale(1.5f);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				Item.useStyle = ItemUseStyleID.Shoot;
				if (player.controlUseTile && (charge >= 15 || frame == 0 || player.CheckMana(7, true))) {
					player.itemTime = 0;
					player.itemAnimation = 5;
					if (charge < 15) {
						if (++charge >= 15)
							for (int i = 0; i < 3; i++) {
								int a = Dust.NewDust(position - velocity, 0, 0, DustID.Frost);
								Main.dust[a].noGravity = true;
							}
					} else if (Main.GameUpdateCount % 12 <= 1) {
						int a = Dust.NewDust(position - velocity, 0, 0, DustID.Frost);
						Main.dust[a].noGravity = true;
					}
					return false;
				}
				if (charge >= 15) {
					Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai1: animation.Frame > 0 ? 0 : -1);
					charge = 0;
					player.itemAnimation = 16;
					player.itemAnimationMax = 16;
					if (frame == 5) {
						SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(0.75f), position);
					}
				}
			} else {
				SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(0.25f), position);
				int prev = -1;
				int curr = -1;
				Vector2 perp = velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero);
				for (int i = 3; --i > -3;) {
					curr = Projectile.NewProjectile(source, position + perp * i * 4, velocity.RotatedBy(i / 16d) * (1.5f - System.Math.Abs(i / 6f)), type, damage / 3, knockback, player.whoAmI, prev);
					if (prev > 0) {
						Main.projectile[prev].ai[1] = curr;
					}
					prev = curr;
				}
			}
			return false;
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Texture2D texture = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(texture, position, Animation.GetFrame(texture), drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
			return false;
		}
	}
	public class Tyrfing_Shard : ModProjectile {
		public const float magRange = 16 * 7.5f;
		public const float speed = 16f;
		public const float inertia = 1f;

		public override string Texture => "Origins/Items/Weapons/Melee/Tyrfing_Shard";
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Tyrfing");
			Main.projFrames[Projectile.type] = 3;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClass.Melee;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 1;
			Projectile.timeLeft = 60;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.frame = Main.rand.Next(3);
			Projectile.spriteDirection = Main.rand.NextBool() ? 1 : -1;
			Projectile.ai[0] = -1f;
			Projectile.ai[1] = -1f;
		}
		public override void AI() {
			Projectile.rotation += Projectile.spriteDirection * 0.3f;
			if (Projectile.localAI[1] > 0) {
				Projectile.localAI[1]--;
				Projectile.timeLeft = 61;
				if (Projectile.localAI[1] <= 0) {
					Projectile.localAI[1] = -Projectile.localAI[0];
					Projectile.localAI[0] = 0;
				}
				return;
			}
			if (Projectile.timeLeft < 57) Projectile.tileCollide = true;
			Vector2 targetCenter = Projectile.Center;
			bool foundTarget = false;
			float rangeMult = 1f;
			if (Projectile.localAI[1] < 0) rangeMult = -Projectile.localAI[1];
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (npc.CanBeChasedBy() && npc.HasBuff(Mag_Debuff.ID)) {
					float distance = Vector2.Distance(npc.Center, Projectile.Center);
					bool closest = Vector2.Distance(Projectile.Center, targetCenter) > distance;
					bool inRange = distance < magRange * rangeMult;
					if ((!foundTarget || closest) && inRange) {
						targetCenter = npc.Center;
						foundTarget = true;
					}
				}
			}
			if (foundTarget) {
				Vector2 direction = targetCenter - Projectile.Center;
				direction.Normalize();
				direction *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
				if (direction.Length() <= Projectile.velocity.Length()) {
					Projectile.velocity = direction;
					Projectile.localAI[0] = 1;
				}
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			Projectile.ArmorPenetration += (int)(target.defense * 0.3f);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.CanBeChasedBy()) target.buffImmune[Mag_Debuff.ID] = false;
			target.AddBuff(Mag_Debuff.ID, 180);
			target.immune[Projectile.owner] = 1;
			if (Projectile.localAI[0] == 1) {
				Projectile.localAI[0] = -1;
				Projectile.position.X += Projectile.width / 2;
				Projectile.position.Y += Projectile.height / 2;
				Projectile.width = 64;
				Projectile.height = 64;
				Projectile.position.X -= Projectile.width / 2;
				Projectile.position.Y -= Projectile.height / 2;
				target.immune[Projectile.owner] = 0;
				Projectile.damage *= 2;
				Projectile.Damage();
				Projectile.Kill();
			} else if (Projectile.localAI[0] == -1) {
				return;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.localAI[1] > 0) return true;
			Dust dust;
			for (int i = 0; i < 2; i++) {
				for (int i2 = 1; i2 < 3; i2++) {
					if (Projectile.ai[i] >= 0) {
						Projectile proj = Main.projectile[(int)Projectile.ai[i]];
						if (proj.active && proj.type == Projectile.type) {
							dust = Dust.NewDustPerfect(Vector2.Lerp(Projectile.Center, proj.Center, 0.33f * i2), 226, Vector2.Lerp(Projectile.velocity, proj.velocity, 0.33f), 200, Scale: 0.25f);
							dust.noGravity = true;
							dust.noLight = true;
						} else {
							Projectile.ai[i] = -1f;
						}
					}
				}
			}
			dust = Dust.NewDustPerfect(Projectile.Center, 226, Projectile.velocity, 200, Scale: 0.25f);
			dust.noGravity = true;
			return true;
		}
	}
	public class Mag_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Magnetized");
			ID = Type;
		}
	}
	public class Tyrfing_Stab : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Tyrfing_B";
		protected override bool CloneNewInstances => true;
		int stabee = -1;
		bool noGrow = false;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Tyrfing");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Spear);
			Projectile.timeLeft = 16;
			Projectile.width = 32;
			Projectile.height = 32;
		}
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			Projectile.direction = projOwner.direction;
			projOwner.heldProj = Projectile.whoAmI;
			projOwner.itemTime = projOwner.itemAnimation;
			Projectile.position.X = ownerMountedCenter.X - (Projectile.width / 2);
			Projectile.position.Y = ownerMountedCenter.Y - (Projectile.height / 2);
			if (!projOwner.frozen) {
				if (movementFactor == 0f) {
					movementFactor = 4.7f;
					if (Projectile.ai[1] == -1) noGrow = true;
					Projectile.ai[1] = 4;
					Projectile.netUpdate = true;
				}
				if (projOwner.itemAnimation < 3) {
					movementFactor -= 1.7f;
				} else if (Projectile.ai[1] > 0) {
					movementFactor += 1.3f;
					Projectile.ai[1]--;
				}
			}
			Projectile.position += Projectile.velocity * movementFactor;
			if (projOwner.itemAnimation == 0) {
				Projectile.Kill();
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
			if (Projectile.spriteDirection == 1) {
				Projectile.rotation -= MathHelper.Pi / 2f;
			}
			if (stabee >= 0) {
				if (!Main.npc[stabee].active) {
					stabee = -2;
					return;
				}
				NPC victim = Main.npc[stabee];
				victim.AddBuff(Impaled_Debuff.ID, 2);
				victim.position += Projectile.position - Projectile.oldPosition;
				victim.Center = Vector2.Lerp(victim.Center, Projectile.Center + Projectile.velocity, 0.035f);
				victim.oldPosition = victim.position;
			}
		}
		public override bool? CanHitNPC(NPC target) {
			Player player = Main.player[Projectile.owner];
			if (stabee >= 0) {
				return target.whoAmI == stabee && player.itemAnimation == 3;
			}
			return base.CanHitNPC(target);
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (stabee == -1) {
				modifiers.Knockback *= 0;
			} else {
				modifiers.SetCrit();
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (stabee >= 0) {
				target.AddBuff(Mag_Debuff.ID, 180);
				int proj;
				bool bias = Main.rand.NextBool();
				for (int i = 8; --i > 0;) {
					proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.PiOver2 * ((bias ^ i % 2 == 0) ? -1 : 1)).RotatedByRandom(1f) * Main.rand.NextFloat(0.25f, 0.3f), ModContent.ProjectileType<Tyrfing_Shard>(), damageDone / 6, Projectile.knockBack, Projectile.owner);
					Main.projectile[proj].localAI[1] = 45;
					Main.projectile[proj].localAI[0] = 3;
					Main.projectile[proj].tileCollide = false;
				}
				target.velocity += Projectile.velocity * target.knockBackResist * 2;
				target.DelBuff(target.FindBuffIndex(Impaled_Debuff.ID));
				target.AddBuff(Stunned_Debuff.ID, 5);
				stabee = -2;
				return;
			}
			if (target.boss || target.type == NPCID.TargetDummy || stabee == -2) {
				stabee = -2;
				return;
			}
			Player player = Main.player[Projectile.owner];
			player.itemAnimation += player.itemAnimationMax;
			player.itemAnimationMax *= 2;
			Projectile.timeLeft = player.itemAnimation;
			stabee = target.whoAmI;
		}
		public override bool PreDraw(ref Color lightColor) {
			if (noGrow) {
				Main.EntitySpriteDraw(Mod.Assets.Request<Texture2D>("Items/Weapons/Melee/Tyrfing_B").Value, (Projectile.Center - Projectile.velocity * 2) - Main.screenPosition, new Rectangle(0, 0, 40, 40), lightColor, Projectile.rotation, new Vector2(20, 20), 1f, SpriteEffects.None, 0);
				return false;
			}
			if (Main.player[Projectile.owner].HeldItem.ModItem is Tyrfing sword) {
				Texture2D texture = Mod.Assets.Request<Texture2D>("Items/Weapons/Melee/Tyrfing").Value;
				Rectangle frame = sword.Animation.GetFrame(texture);
				if (sword.frame > 0) sword.frame--;
				Main.EntitySpriteDraw(texture, (Projectile.Center - Projectile.velocity * 2) - Main.screenPosition, frame, lightColor, Projectile.rotation, new Vector2(20, 20), 1f, SpriteEffects.None, 0);
				return false;
			}
			return true;
		}
	}
	public class Impaled_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Impaled");
			ID = Type;
		}
	}
	public class Stunned_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Stunned");
			ID = Type;
		}
	}
}
