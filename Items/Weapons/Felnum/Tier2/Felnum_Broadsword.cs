using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.World;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Items.Weapons.Felnum.Tier2 {
    //this took seven and a half hours to make
	public class Felnum_Broadsword : IAnimatedItem {
        public const int baseDamage = 58;

        public override bool CloneNewInstances => true;
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
			DisplayName.SetDefault("Tyrfing"/*"Felnum Broadsword"*/);
			Tooltip.SetDefault("Behold\nHold right click to stab");
            animation = new DrawAnimationManual(6);
            animation.Frame = 5;
			Main.RegisterItemAnimation(item.type, animation);
		}
		public override void SetDefaults() {
			item.damage = baseDamage;
			item.melee = true;
			item.width = 42;
			item.height = 42;
			item.useTime = 48;
			item.useAnimation = 16;
			item.useStyle = 1;
			item.knockBack = 9;
			item.value = 5000;
			item.autoReuse = true;
            item.useTurn = false;
			item.rare = ItemRarityID.Lime;
			item.UseSound = SoundID.Item1;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 12);
            recipe.AddIngredient(ItemID.OrichalcumBar, 7);
            recipe.SetResult(this);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.AddRecipe();
            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 12);
            recipe.AddIngredient(ItemID.MythrilBar, 7);
            recipe.SetResult(this);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.AddRecipe();
        }

        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override void GetWeaponKnockback(Player player, ref float knockback) {
            if(player.altFunctionUse == 2) knockback *= 2.1111111111111111111111111111111f;
        }

        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse == 2) {
			    //item.useTime = 1;
			    //item.useAnimation = 16;
			    item.useStyle = 5;
			    //item.knockBack = 19;
                item.shoot = ModContent.ProjectileType<Felnum_Broadsword_Stab>();
                item.shootSpeed = 3.4f;
                item.noUseGraphic = true;
                item.noMelee = true;
			    item.UseSound = null;
            } else {
			    //item.useTime = 16;
			    //item.useAnimation = 16;
			    item.useStyle = 1;
			    //item.knockBack = 9;
                item.shoot = ModContent.ProjectileType<Felnum_Broadsword_Shard>();
                item.shootSpeed = 6.5f;
                item.noUseGraphic = false;
                item.noMelee = false;
			    item.UseSound = SoundID.Item1;
            }
            return base.CanUseItem(player);
        }

        public override void HoldItem(Player player) {
            if(player.itemAnimation!=0 && player.altFunctionUse != 2) {
                player.GetModPlayer<OriginPlayer>().ItemLayerWrench = true;
            }
        }

        public override void Load(TagCompound tag) {
            frame = tag.GetInt("frame");
        }
        public override TagCompound Save() {
            return new TagCompound() {{"frame", frame}};
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
            OriginExtensions.FixedUseItemHitbox(item, player, ref hitbox, ref noHitbox);
            if(frame==5/*!ModContent.GetInstance<OriginWorld>().felnumBroadswordStab*/) {
                hitbox = new Rectangle(0,0,0,0);
                //if(animation.Frame==0) ModContent.GetInstance<OriginWorld>().felnumBroadswordStab = true;
            }
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
            if(player.altFunctionUse == 2) {
                if(player.controlUseTile && (charge >= 15 || frame == 0 || player.CheckMana(7, true))) {
                    player.itemTime = 0;
                    player.itemAnimation = 5;
                    if(charge < 15) {
                        if(++charge >= 15)
                            for(int i = 0; i < 3; i++) {
                                int a = Dust.NewDust(position - new Vector2(speedX, speedY), 0, 0, 92);
                                Main.dust[a].noGravity = true;
                            }
                    } else if(Main.GameUpdateCount % 12 <= 1) {
                        int a = Dust.NewDust(position - new Vector2(speedX, speedY), 0, 0, 92);
                        Main.dust[a].noGravity = true;
                    }
                    return false;
                }
                if(charge >= 15) {
                    Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI, ai1: animation.Frame > 0 ? 0 : -1);
                    charge = 0;
                    player.itemAnimation = 16;
                    player.itemAnimationMax = 16;
                    if(frame == 5) {
                        Main.PlaySound(2, (int)position.X, (int)position.Y, 122, 0.75f, 1f);
                    }
                }
            } else {
                Main.PlaySound(2, (int)position.X, (int)position.Y, 122, 0.25f, 1f);
                int prev = -1;
                int curr = -1;
                Vector2 speed = new Vector2(speedX, speedY);
                Vector2 perp = speed.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero);
                for(int i = 3; --i> -3;) {
                    curr = Projectile.NewProjectile(position+perp*i*4, speed.RotatedBy(i/16d)*(1.5f-System.Math.Abs(i/6f)), type, damage/3, knockBack, player.whoAmI, prev);
                    if(prev>0) {
                        Main.projectile[prev].ai[1] = curr;
                    }
                    prev = curr;
                }
            }
            return false;
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            Texture2D texture = Main.itemTexture[item.type];
            spriteBatch.Draw(texture, position, Animation.GetFrame(texture), drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
        /*public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
            Texture2D texture = Main.itemTexture[item.type];
            spriteBatch.Draw(texture, item.position-Main.screenPosition, Animation.GetFrame(texture), lightColor, 0f, default(Vector2), scale, SpriteEffects.None, 0f);
            return false;
        }*/
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) {
            damage+=(damage-baseDamage)/2;
        }
        public override void GetWeaponDamage(Player player, ref int damage) {
            if(!OriginPlayer.ItemChecking)damage+=(damage-baseDamage)/2;
        }
    }
    public class Felnum_Broadsword_Shard : ModProjectile {
        public const float magRange = 16*7.5f;
		public const float speed = 16f;
		public const float inertia = 1f;

        public override string Texture => "Origins/Items/Weapons/Felnum/Tier2/Felnum_Broadsword_Shard";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Broadsword");
            Main.projFrames[projectile.type] = 3;
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.melee = true;
            projectile.ranged = false;
            projectile.aiStyle = 0;
            projectile.extraUpdates = 1;
            projectile.timeLeft = 60;
			projectile.width = 10;
			projectile.height = 10;
            projectile.penetrate = -1;
            //projectile.usesLocalNPCImmunity = true;
            //projectile.localNPCHitCooldown = 12;
            projectile.tileCollide = false;
            projectile.frame = Main.rand.Next(3);
            projectile.spriteDirection = Main.rand.NextBool()?1:-1;
            projectile.ai[0] = -1f;
            projectile.ai[1] = -1f;
        }
		public override void AI() {
            projectile.rotation += projectile.spriteDirection*0.3f;
            if(projectile.localAI[1] > 0) {
                projectile.localAI[1]--;
                projectile.timeLeft = 61;
                if(projectile.localAI[1] <= 0) {
                    projectile.localAI[1] = -projectile.localAI[0];
                    projectile.localAI[0] = 0;
                }
                return;
            }
            if(projectile.timeLeft < 57)projectile.tileCollide = true;
			Vector2 targetCenter = projectile.Center;
			bool foundTarget = false;
            float rangeMult = 1f;
            if(projectile.localAI[1] < 0) rangeMult = -projectile.localAI[1];
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (npc.CanBeChasedBy() && npc.HasBuff(Mag_Debuff.ID)) {
					float distance = Vector2.Distance(npc.Center, projectile.Center);
					bool closest = Vector2.Distance(projectile.Center, targetCenter) > distance;
                    bool inRange = distance < magRange*rangeMult;
					if ((!foundTarget || closest) && inRange) {
						targetCenter = npc.Center;
						foundTarget = true;
					}
				}
			}
            if(foundTarget) {
				Vector2 direction = targetCenter - projectile.Center;
				direction.Normalize();
				direction *= speed;
				projectile.velocity = (projectile.velocity * (inertia - 1) + direction) / inertia;
                if(direction.Length()<=projectile.velocity.Length()) {
                    //projectile.Center = targetCenter;
                    projectile.velocity = direction;
                    projectile.localAI[0] = 1;
                }
            }
		}
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(target.CanBeChasedBy()) target.buffImmune[Mag_Debuff.ID] = false;
            target.AddBuff(Mag_Debuff.ID, 180);
            target.immune[projectile.owner] = 1;
            if(projectile.localAI[0] == 1) {
                projectile.localAI[0] = -1;
                projectile.position.X += projectile.width / 2;
                projectile.position.Y += projectile.height / 2;
                projectile.width = 64;
                projectile.height = 64;
                projectile.position.X -= projectile.width / 2;
                projectile.position.Y -= projectile.height / 2;
                target.immune[projectile.owner] = 0;
                projectile.damage *= 2;
                projectile.Damage();
                projectile.Kill();
            } else if(projectile.localAI[0] == -1) {
                return;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            if(projectile.localAI[1] > 0)return true;
            Dust dust;
            for(int i = 0; i < 2; i++) {
                for(int i2 = 1; i2 < 3; i2++) {
                    if(projectile.ai[i] >= 0) {
                        Projectile proj = Main.projectile[(int)projectile.ai[i]];
                        if(proj.active && proj.type == projectile.type) {
                            dust = Dust.NewDustPerfect(Vector2.Lerp(projectile.Center, proj.Center, 0.33f*i2), 226, Vector2.Lerp(projectile.velocity, proj.velocity, 0.33f), 200, Scale: 0.25f);
                            dust.noGravity = true;
                            dust.noLight = true;
                        } else {
                            projectile.ai[i] = -1f;
                        }
                    }
                }
            }
            dust = Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity, 200, Scale:0.25f);
            dust.noGravity = true;
            //dust.noLight = true;
            //spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 10*projectile.frame, 10, 10), lightColor, projectile.rotation, new Vector2(5, 5), 1f, SpriteEffects.None, 0f);
            return true;
        }
    }
    public class Mag_Debuff : ModBuff {
        public static int ID { get; private set; }
        public override bool Autoload(ref string name, ref string texture) {
            texture = "Terraria/Buff_160";
            return true;
        }
        public override void SetDefaults() {
            DisplayName.SetDefault("Magnetized");
            ID = Type;
        }
    }
    public class Felnum_Broadsword_Stab : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Felnum/Tier2/Felnum_Broadsword_B";
        public override bool CloneNewInstances => true;
        int stabee = -1;
        bool noGrow = false;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Broadsword");
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Spear);
            projectile.timeLeft = 16;
			projectile.width = 32;
			projectile.height = 32;
        }
        public float movementFactor{
			get => projectile.ai[0];
			set => projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			projectile.direction = projOwner.direction;
			projOwner.heldProj = projectile.whoAmI;
			projOwner.itemTime = projOwner.itemAnimation;
			projectile.position.X = ownerMountedCenter.X - (projectile.width / 2);
			projectile.position.Y = ownerMountedCenter.Y - (projectile.height / 2);
			if (!projOwner.frozen) {
				if (movementFactor == 0f){
                    movementFactor = 4.7f;
                    if(projectile.ai[1]==-1)noGrow = true;
                    projectile.ai[1] = 4;
					projectile.netUpdate = true;
				}
				if (projOwner.itemAnimation < 3){
					movementFactor-=1.7f;
				} else if (projectile.ai[1]>0){
					movementFactor+=1.3f;
                    projectile.ai[1]--;
                }
			}
			projectile.position += projectile.velocity * movementFactor;
			if (projOwner.itemAnimation == 0) {
				projectile.Kill();
			}
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
            if (projectile.spriteDirection == 1) {
				projectile.rotation -= MathHelper.Pi/2f;
			}
            if(stabee >= 0) {
                if(!Main.npc[stabee].active) {
                    stabee = -2;
                    return;
                }
                NPC victim = Main.npc[stabee];
                victim.AddBuff(Impaled_Debuff.ID, 2);
                victim.position+=projectile.position-projectile.oldPosition;
                victim.Center = Vector2.Lerp(victim.Center,projectile.Center+projectile.velocity, 0.035f);
                victim.oldPosition = victim.position;
            }
		}
        public override bool? CanHitNPC(NPC target) {
			Player player = Main.player[projectile.owner];
            if(stabee >= 0) {
                return target.whoAmI == stabee && player.itemAnimation == 3;
            }
            return base.CanHitNPC(target);
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(stabee == -1) {
                knockback = 0;
            } else {
                crit = true;
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(stabee >= 0) {
                target.AddBuff(Mag_Debuff.ID, 180);
                int proj;
                bool bias = Main.rand.NextBool();
                for(int i = 8; --i>0;) {
                    proj = Projectile.NewProjectile(projectile.Center, projectile.velocity.RotatedBy(MathHelper.PiOver2*((bias^i%2==0)?-1:1)).RotatedByRandom(1f)*Main.rand.NextFloat(0.25f,0.3f), ModContent.ProjectileType<Felnum_Broadsword_Shard>(), damage/6, projectile.knockBack, projectile.owner);
                    Main.projectile[proj].localAI[1] = 45;
                    Main.projectile[proj].localAI[0] = 3;
                    Main.projectile[proj].tileCollide = false;
                }
                target.velocity += projectile.velocity * target.knockBackResist * 2;
                target.DelBuff(target.FindBuffIndex(Impaled_Debuff.ID));
                target.AddBuff(Stunned_Debuff.ID, 5);
                stabee = -2;
                return;
            }
            if(target.boss || target.type == NPCID.TargetDummy || stabee == -2) {
                stabee = -2;
                return;
            }
            Player player = Main.player[projectile.owner];
            player.itemAnimation+=player.itemAnimationMax;
            player.itemAnimationMax*=2;
            projectile.timeLeft = player.itemAnimation;
            stabee = target.whoAmI;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            if(noGrow) {
                spriteBatch.Draw(mod.GetTexture("Items/Weapons/Felnum/Tier2/Felnum_Broadsword_B"), (projectile.Center - projectile.velocity*2) - Main.screenPosition, new Rectangle(0, 0, 40, 40), lightColor, projectile.rotation, new Vector2(20, 20), 1f, SpriteEffects.None, 0f);
                return false;
            }
            Texture2D texture = mod.GetTexture("Items/Weapons/Felnum/Tier2/Felnum_Broadsword");
            if(Main.player[projectile.owner].HeldItem.modItem is Felnum_Broadsword sword) {
                Rectangle frame = sword.Animation.GetFrame(texture);
                if(sword.frame>0)sword.frame--;
                spriteBatch.Draw(texture, (projectile.Center - projectile.velocity*2) - Main.screenPosition, frame, lightColor, projectile.rotation, new Vector2(20, 20), 1f, SpriteEffects.None, 0f);
                return false;
            }
            return true;
        }
    }
    public class Impaled_Debuff : ModBuff {
        public static int ID { get; private set; }
        public override bool Autoload(ref string name, ref string texture) {
            texture = "Terraria/Buff_160";
            return true;
        }
        public override void SetDefaults() {
            DisplayName.SetDefault("Impaled");
            ID = Type;
        }
    }
    public class Stunned_Debuff : ModBuff {
        public static int ID { get; private set; }
        public override bool Autoload(ref string name, ref string texture) {
            texture = "Terraria/Buff_160";
            return true;
        }
        public override void SetDefaults() {
            DisplayName.SetDefault("Stunned");
            ID = Type;
        }
    }
}
