using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Accessories.Eyndum_Cores;
using Origins.Items.Armor.Vanity.Terlet.PlagueTexan;
using Origins.Items.Materials;
using Origins.Items.Other.Fish;
using Origins.Items.Other.Testing;
using Origins.Items.Weapons.Defiled;
using Origins.Items.Weapons.Explosives;
using Origins.Items.Weapons.Summon;
using Origins.Layers;
using Origins.Projectiles;
using Origins.Projectiles.Misc;
using Origins.World;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Origins.Items.OriginGlobalItem;
using static Origins.OriginExtensions;

namespace Origins {
    public class OriginPlayer : ModPlayer {
        public const float rivenMaxMult = 0.3f;
        public float rivenMult => (1f-rivenMaxMult)+Math.Max((Player.statLife/(float)Player.statLifeMax2)*(rivenMaxMult*2), rivenMaxMult);
        
        #region set bonuses
        public bool fiberglassSet = false;
        public bool cryostenSet = false;
        public bool cryostenHelmet = false;
        public bool felnumSet = false;
        public float felnumShock = 0;
        public float oldFelnumShock = 0;
        public bool celestineSet = false;
        public bool minerSet = false;
        public bool defiledSet = false;
        public bool reshapingChunk = false;
        public bool rivenSet = false;
        public bool riftSet = false;
        public bool eyndumSet = false;
        public bool mimicSet = false;
        public int mimicSetChoices = 0;
        public int setActiveAbility = 0;
        public int setAbilityCooldown = 0;
        #endregion set bonuses

        #region accessories
        public bool bombHandlingDevice = false;
        public bool dimStarlight = false;
        public byte dimStarlightCooldown = 0;
        public bool madHand = false;
        public bool fiberglassDagger = false;
        public bool advancedImaging = false;
        public bool rasterize = false;
        public bool decayingScale = false;
        #endregion

        #region explosive stats
        public float explosiveThrowSpeed = 1;
        public float explosiveSelfDamage = 1;
        #endregion

        #region biomes
        public bool ZoneVoid { get; internal set; } = false;
        public float ZoneVoidProgress = 0;
        public float ZoneVoidProgressSmoothed = 0;

        public bool ZoneDefiled { get; internal set; } = false;
        public float ZoneDefiledProgress = 0;
        public float ZoneDefiledProgressSmoothed = 0;

        public bool ZoneRiven { get; internal set; } = false;
        public float ZoneRivenProgress = 0;
        public float ZoneRivenProgressSmoothed = 0;

        public bool ZoneBrine { get; internal set; } = false;
        public float ZoneBrineProgress = 0;
        public float ZoneBrineProgressSmoothed = 0;
        #endregion

        #region buffs
        public int rapidSpawnFrames = 0;
        public int rasterizedTime = 0;
        public bool toxicShock = false;
        #endregion

        #region keybinds
        public bool controlTriggerSetBonus = false;
        public bool releaseTriggerSetBonus = false;
        #endregion

        public bool itemLayerWrench = false;
        public bool plagueSight = false;

        public Ref<Item> eyndumCore = null;

        internal static bool ItemChecking = false;
        public int cryostenLifeRegenCount = 0;
        internal byte oldBonuses = 0;
        public const int minionSubSlotValues = 3;
        public float[] minionSubSlots = new float[minionSubSlotValues];
        public int wormHeadIndex = -1;
        public int heldProjectile = -1;
        public override void ResetEffects() {
            oldBonuses = 0;
            if(fiberglassSet||fiberglassDagger)oldBonuses|=1;
            if(felnumSet)oldBonuses|=2;
            fiberglassSet = false;
            cryostenSet = false;
            cryostenHelmet = false;
            oldFelnumShock = felnumShock;
            if(!felnumSet) {
                felnumShock = 0;
            } else {
                if(felnumShock > Player.statLifeMax2) {
                    if(Main.rand.NextBool(20)) {
                        Vector2 pos = new Vector2(Main.rand.Next(4, Player.width-4), Main.rand.Next(4, Player.height-4));
                        Projectile proj = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.position + pos, Main.rand.NextVector2CircularEdge(3,3), Felnum_Shock_Leader.ID, (int)(felnumShock*0.1f), 0, Player.whoAmI, pos.X, pos.Y);
                        if(proj.ModProjectile is Felnum_Shock_Leader shock) {
                            shock.Parent = Player;
                            shock.OnStrike += () => felnumShock *= 0.9f;
                        }
                    }
                    felnumShock -= (felnumShock - Player.statLifeMax2) / Player.statLifeMax2 * 5 + 1;
                }
            }
            felnumSet = false;
            celestineSet = false;
            minerSet = false;
            defiledSet = false;
            reshapingChunk = false;
            rivenSet = false;
            riftSet = false;
            eyndumSet = false;
            mimicSet = false;
            setActiveAbility = 0;
            if (setAbilityCooldown > 0) {
                setAbilityCooldown--;
                if (setAbilityCooldown == 0) {
                    SoundEngine.PlaySound(SoundID.MaxMana.WithPitch(-1).WithVolume(0.5f));
                    for (int i = 0; i < 5; i++) {
                        int dust = Dust.NewDust(Player.position, Player.width, Player.height, DustID.PortalBoltTrail, 0f, 0f, 255, Color.Black, (float)Main.rand.Next(20, 26) * 0.1f);
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity *= 0.5f;
                    }
                }
			}
            bombHandlingDevice = false;
            dimStarlight = false;
            madHand = false;
            fiberglassDagger = false;
            advancedImaging = false;
            rasterize = false;
            decayingScale = false;
            toxicShock = false;
            explosiveThrowSpeed = 1f;
            explosiveSelfDamage = 1f;
            if(cryostenLifeRegenCount>0)
                cryostenLifeRegenCount--;
            if(dimStarlightCooldown>0)
                dimStarlightCooldown--;
            if(rapidSpawnFrames>0)
                rapidSpawnFrames--;
            int rasterized = Player.FindBuffIndex(Rasterized_Debuff.ID);
            if (rasterized >= 0) {
                rasterizedTime = Math.Min(Math.Min(rasterizedTime + 1, 8), Player.buffTime[rasterized] - 1);
            }
            Player.breathMax = 200;
            plagueSight = false;
            minionSubSlots = new float[minionSubSlotValues];
        }
        public override void PostUpdate() {
            heldProjectile = -1;
            if (rasterizedTime > 0) {
                Player.velocity = Vector2.Lerp(Player.velocity, Player.oldVelocity, rasterizedTime * 0.06f);
                Player.position = Vector2.Lerp(Player.position, Player.oldPosition, rasterizedTime * 0.06f);
            }
            Player.oldVelocity = Player.velocity;
        }
        public override void PostUpdateMiscEffects() {
            if(cryostenHelmet) {
                if(Player.statLife!=Player.statLifeMax2&&(int)Main.time%(cryostenLifeRegenCount>0 ? 5 : 15)==0)
                    for(int i = 0; i < 10; i++) {
                        int num6 = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Frost);
                        Main.dust[num6].noGravity = true;
                        Main.dust[num6].velocity *= 0.75f;
                        int num7 = Main.rand.Next(-40, 41);
                        int num8 = Main.rand.Next(-40, 41);
                        Main.dust[num6].position.X += num7;
                        Main.dust[num6].position.Y += num8;
                        Main.dust[num6].velocity.X = -num7 * 0.075f;
                        Main.dust[num6].velocity.Y = -num8 * 0.075f;
                    }
            }
        }
        public override void PostUpdateEquips() {
            if (eyndumSet) {
                ApplyEyndumSetBuffs();
                if (eyndumCore?.Value?.ModItem is ModItem equippedCore) {
                    equippedCore.UpdateEquip(Player);
                }
            }
            Player.buffImmune[Rasterized_Debuff.ID] = Player.buffImmune[BuffID.Cursed];
        }
        public override void ProcessTriggers(TriggersSet triggersSet) {
            releaseTriggerSetBonus = !controlTriggerSetBonus;
            controlTriggerSetBonus = triggersSet.KeyStatus["Origins: Trigger Set Bonus"];
			if (controlTriggerSetBonus && releaseTriggerSetBonus) {
                TriggerSetBonus();
			}
        }
        public void ApplyEyndumSetBuffs() {
            #region movement
            float speedMult = (Player.moveSpeed - 1) * 0.5f;
            Player.runAcceleration += (Player.runAcceleration / Player.moveSpeed) * speedMult;
            Player.maxRunSpeed += (Player.maxRunSpeed / Player.moveSpeed) * speedMult;
            Player.extraFall += Player.extraFall / 2;
            Player.wingTimeMax += Player.wingTimeMax / 2;
            Player.jumpSpeedBoost += Player.jumpSpeedBoost * 0.5f;
            if (Player.spikedBoots == 1) Player.spikedBoots = 2;
            #endregion
            #region defense
            Player.statLifeMax2 += (Player.statLifeMax2 - Player.statLifeMax) / 2;
            Player.statDefense += Player.statDefense / 2;
            Player.endurance += Player.endurance * 0.5f;
            Player.lifeRegen += Player.lifeRegen / 2;
            Player.thorns += Player.thorns * 0.5f;
            Player.lavaMax += Player.lavaMax / 2;
            #endregion
            #region damage
            List<DamageClass> damageClasses = (List<DamageClass>)(typeof(DamageClassLoader).GetField("DamageClasses", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null));
			foreach (DamageClass damageClass in damageClasses) {
                Player.GetArmorPenetration(damageClass) += Player.GetArmorPenetration(damageClass) * 0.5f;

                Player.GetDamage(damageClass) = Player.GetDamage(damageClass).MultiplyBonuses(1.5f);

                Player.GetAttackSpeed(damageClass) += (Player.GetAttackSpeed(damageClass) - 1) * 0.5f;
            }

            Player.arrowDamage = Player.arrowDamage.MultiplyBonuses(1.5f);
            Player.bulletDamage = Player.bulletDamage.MultiplyBonuses(1.5f);
            Player.rocketDamage = Player.rocketDamage.MultiplyBonuses(1.5f);

            //explosiveDamage += (explosiveDamage - 1) * 0.5f;
            //explosiveThrowSpeed += (explosiveThrowSpeed - 1) * 0.5f;
            //explosiveSelfDamage += (explosiveSelfDamage - 1) * 0.5f;
            #endregion
            #region resources
            Player.statManaMax2 += (Player.statManaMax2 - Player.statManaMax) / 2;
            Player.manaCost += (Player.manaCost - 1) * 0.5f;
            Player.maxMinions += (Player.maxMinions - 1) / 2;
            Player.maxTurrets += (Player.maxTurrets - 1) / 2;
            Player.manaRegenBonus += Player.manaRegenBonus / 2;
            Player.manaRegenDelayBonus += Player.manaRegenDelayBonus / 2;
            #endregion
            #region utility
            Player.wallSpeed += (Player.wallSpeed - 1) * 0.5f;
            Player.tileSpeed += (Player.tileSpeed - 1) * 0.5f;
            Player.pickSpeed *= (Player.pickSpeed - 1) * 0.5f;
            Player.aggro += Player.aggro / 2;
            Player.blockRange += Player.blockRange / 2;
            #endregion
        }
        public void TriggerSetBonus() {
            if (setAbilityCooldown > 0) return;
			switch (setActiveAbility) {
                case 1: {
					if (Player.CheckMana((int)(40 * Player.manaCost), true)) {
                        Vector2 speed = Vector2.Normalize(Main.MouseWorld - Player.MountedCenter) * 14;
                        int type = ModContent.ProjectileType<Infusion_P>();
						for (int i = -5; i < 6; i++) {
                            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.MountedCenter + speed.RotatedBy(MathHelper.PiOver2) * i * 0.25f + speed * (5 - Math.Abs(i)) * 0.75f, speed, type, 40, 7, Player.whoAmI);
                        }
                        setAbilityCooldown = 30;
                        if(Player.manaRegenDelay < 60) Player.manaRegenDelay = 60;
                    }
				}
                break;
                case 2: {
                    if (Player.CheckMana((int)(40 * Player.manaCost), true)) {
                        setAbilityCooldown = 1800;
                        Player.AddBuff(Mimic_Buff.ID, 600);
                        Player.AddBuff(BuffID.Heartreach, 30);
                    }
                }
                break;
                case 3:
                break;
                default:
                break;
			}
        }
        public override void UpdateLifeRegen() {
            if(cryostenHelmet)Player.lifeRegenCount+=cryostenLifeRegenCount>0 ? 180 : 1;
        }
        #region attacks
        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit) {
            if(felnumShock>29) {
                damage+=(int)(felnumShock/15);
                felnumShock = 0;
                SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), target.Center);
            }
        }
        public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            if (advancedImaging) {
                velocity *= 1.3f;
            }
            if (item.CountsAsClass(DamageClasses.Explosive)) {
                if (item.useAmmo == 0 && item.CountsAsClass(DamageClass.Throwing)) {
                    velocity *= explosiveThrowSpeed;
                }
            }
            if (item.shoot > ProjectileID.None && felnumShock > 29) {
                Projectile p = new Projectile();
                p.SetDefaults(type);
                OriginGlobalProj.felnumEffectNext = true;
                if (p.CountsAsClass(DamageClass.Melee) || p.aiStyle == 60) return;
                damage += (int)(felnumShock / 15);
                felnumShock = 0;
                SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), position);
            }
        }
		public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            if(item.CountsAsClass(DamageClasses.Explosive)) {
                if(riftSet) {
                    Fraction dmg = new Fraction(2, 2);
                    int c = (madHand ? 1 : 0) + (Main.rand.NextBool(2)? 1 : 0);
                    dmg.D+=c;
                    damage *= dmg;
                    double rot = Main.rand.NextBool(2)?-0.1:0.1;
                    Vector2 _position;
                    Vector2 _velocity;
                    int _type;
                    int _damage;
                    float _knockBack;
                    for(int i = c; i-->0;) {
                        _position = position;
                        _velocity = velocity.RotatedBy(rot);
                        _type = type;
                        _damage = damage;
                        _knockBack = knockback;
                        if(ItemLoader.Shoot(item, Player, source, _position, _velocity, _type, _damage, _knockBack)) {
                            Projectile.NewProjectile(source, _position, _velocity, _type, _damage, _knockBack, Player.whoAmI);
                        }
                        rot = -rot;
                    }
                }
            }
            return true;
        }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(Origins.DamageModOnHit[proj.type]) {
                bool shouldReplace = Origins.ExplosiveBaseDamage.TryGetValue(proj.type, out int dam);
                float baseDamage = Player.GetTotalDamage(proj.DamageType).ApplyTo(shouldReplace ? dam : damage);
                damage = shouldReplace ? Main.DamageVar(baseDamage) : (int)baseDamage;
            }
            if(proj.CountsAsClass(DamageClass.Melee) && felnumShock > 29) {
                damage+=(int)(felnumShock / 15);
                felnumShock = 0;
                SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), proj.Center);
            }
            if(proj.minion&&rivenSet) {
                damage = (int)(damage*rivenMult);
            }
        }
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) {
            if(crit) {
                if(celestineSet)
                    Item.NewItem(item.GetSource_OnHit(target, "SetBonus_Celestine"), target.Hitbox, Main.rand.Next(Origins.celestineBoosters));
                if(dimStarlight&&dimStarlightCooldown<1) {
                    Item.NewItem(item.GetSource_OnHit(target, "Accessory"), target.position, target.width, target.height, ItemID.Star);
                    dimStarlightCooldown = 90;
                }
            }
            if (rasterize) { 
                target.AddBuff(Rasterized_Debuff.ID, Rasterized_Debuff.duration);
            }
            if (decayingScale) {
                target.AddBuff(Toxic_Shock_Debuff.ID, Toxic_Shock_Debuff.default_duration);
                target.AddBuff(Solvent_Debuff.ID, 300);
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
            if(crit) {
                if(celestineSet)
                    Item.NewItem(proj.GetSource_OnHit(target, "SetBonus_Celestine"), target.Hitbox, Main.rand.Next(Origins.celestineBoosters));
                if(dimStarlight&&dimStarlightCooldown<1) {
                    Item.NewItem(proj.GetSource_OnHit(target, "Accessory"), target.position, target.width, target.height, ItemID.Star);
                    dimStarlightCooldown = 90;
                }
            }
            if (rasterize) { 
                target.AddBuff(Rasterized_Debuff.ID, Rasterized_Debuff.duration);
            }
            if (decayingScale) {
                target.AddBuff(Toxic_Shock_Debuff.ID, Toxic_Shock_Debuff.default_duration);
                target.AddBuff(Solvent_Debuff.ID, 300);
            }
        }
        public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit) {
            if(proj.owner == Player.whoAmI && proj.friendly && proj.CountsAsClass(DamageClasses.Explosive)) {
                float damageVal = damage;
                if(minerSet) {
                    explosiveSelfDamage-=0.2f;
                    float inverseDamage = Player.GetDamage(DamageClasses.Explosive).ApplyTo(damage);
                    damageVal -= inverseDamage - damage;
                    //damage = (int)(damage/explosiveDamage);
                    //damage-=damage/5;
                }
                damage = (int)(damageVal * explosiveSelfDamage);
            }
        }
        public override void OnHitByNPC(NPC npc, int damage, bool crit) {
            if(!Player.noKnockback && damage!=0) {
                Player.velocity.X *= MeleeCollisionNPCData.knockbackMult;
            }
            MeleeCollisionNPCData.knockbackMult = 1f;
        }
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
            if(Player.HasBuff(Solvent_Debuff.ID)&&Main.rand.Next(9)<3) {
                crit = true;
            }
            if(defiledSet) {
                float manaDamage = Math.Max(damage-Player.statDefense*(Main.expertMode?0.75f:0.5f), 1) * (reshapingChunk ? 0.25f : 0.15f);
                float costMult = 3;
                float costMult2 = 1/(Player.GetDamage(DamageClass.Magic).Additive/Player.GetDamage(DamageClass.Magic).Multiplicative);
                if(Player.statMana < manaDamage*costMult*costMult2) {
                    manaDamage = Player.statMana/(costMult*costMult2);
                }
                if(Player.magicCuffs) {
                    if(costMult2>1)
                        costMult2 = 1/costMult2;
                }
                if(manaDamage*costMult*costMult2>=1f)
                    Player.ManaEffect((int)-(manaDamage*costMult*costMult2));
                Player.CheckMana((int)Math.Floor(manaDamage*costMult*costMult2), true);
                damage = (int)(damage-manaDamage);
                Player.magicCuffs = false;
                Player.AddBuff(ModContent.BuffType<Defiled_Exhaustion_Buff>(), 50);
            }else if (reshapingChunk) {
                damage -= damage / 20;
            }
			if (toxicShock) {
                damage += Player.statDefense / 10;
			}
            return damage != 0;
        }
        #endregion
        public override void PostSellItem(NPC vendor, Item[] shopInventory, Item item) {
            if (vendor.type == NPCID.Demolitionist && item.type == ModContent.ItemType<Peat_Moss>()) {
                OriginSystem originWorld = ModContent.GetInstance<OriginSystem>();
                if (originWorld.peatSold < 20) {
                    if (item.stack >= 20 - originWorld.peatSold) {
                        item.stack -= 20 - originWorld.peatSold;
                        originWorld.peatSold = 20;
                        int nextSlot = 0;
                        for (; ++nextSlot < shopInventory.Length && !shopInventory[nextSlot].IsAir;);
                        if (nextSlot < shopInventory.Length) shopInventory[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Grenade>());
                        if (nextSlot < shopInventory.Length) shopInventory[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Bomb>());
                        if (nextSlot < shopInventory.Length) shopInventory[nextSlot].SetDefaults(ModContent.ItemType<Impact_Dynamite>());
                    } else {
                        originWorld.peatSold += item.stack;
                        item.TurnToAir();
                    }
                }
            }
        }
        public override void PreUpdateMovement() {
        }
        /*public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) {
            if(item.type==ModContent.ItemType<Peat_Moss>()) {

            }
        }*/
        public override void LoadData(TagCompound tag) {
            if (tag.SafeGet<Item>("EyndumCore") is Item eyndumCoreItem) {
                eyndumCore = new Ref<Item>(eyndumCoreItem);
            }
            if (tag.SafeGet<int>("MimicSetSelection") is int mimicSetSelection) {
                mimicSetChoices = mimicSetSelection;
            }
        }
        public override void SaveData(TagCompound tag)/* Edit tag parameter rather than returning new TagCompound */ {
            if (eyndumCore is not null) {
                tag.Add("EyndumCore", eyndumCore.Value);
            }
            tag.Add("MimicSetSelection", mimicSetChoices);
        }
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
            bool zoneDefiled = ZoneDefiled;
            bool zoneRiven = ZoneRiven;
			if (zoneDefiled && zoneDefiled) {
				if (Main.rand.NextBool()) {
                    zoneDefiled = false;
                } else {
                    zoneRiven = false;
				}
			}
            if (zoneDefiled) {
                if (attempt.legendary && Main.hardMode && Main.rand.NextBool(2)) {
                    itemDrop = ModContent.ItemType<Knee_Slapper>();
                } else if (attempt.uncommon && !attempt.rare) {
                    itemDrop = ModContent.ItemType<Prikish>();
                }
            }else if (zoneRiven) {
                if (attempt.legendary && Main.hardMode && Main.rand.NextBool(2)) {
                    itemDrop = ModContent.ItemType<Knee_Slapper>();
                } else if (attempt.uncommon && !attempt.rare) {
                    itemDrop = ModContent.ItemType<Prikish>();
                }
            }
        }
		public override bool PreItemCheck() {
            ItemChecking = true;
            return true;
        }
        public override void PostItemCheck() {
            ItemChecking = false;
        }
		public override void HideDrawLayers(PlayerDrawSet drawInfo) {
            Item item = drawInfo.heldItem;
            if (drawInfo.drawPlayer.ItemAnimationActive && 
                ((item.useStyle == ItemUseStyleID.Shoot &&item.ModItem is ICustomDrawItem) || 
                (item.useStyle == ItemUseStyleID.Swing && item.ModItem is AnimatedModItem))) PlayerDrawLayers.HeldItem.Hide();
		}
		/*public override void ModifyDrawLayers(List<PlayerLayer> layers) {
            if (Player.HeldItem.ModItem is Chocolate_Bar animator) {
                layers.Add(new PlayerLayer("Origins (debugging tool)", "animator", (v)=>animator.DrawAnimations(v)));
            }
        }*/
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            if(plagueSight) drawInfo.colorEyes = new Color(255,215,0);
            //if(drawInfo.drawPlayer.body==Origins.PlagueTexanJacketID) drawInfo.drawHands = true;
        }
        public override void FrameEffects() {
            for(int i = 13; i < 18+Player.extraAccessorySlots; i++) {
                if(Player.armor[i].type==Plague_Texan_Sight.ID)Plague_Texan_Sight.ApplyVisuals(Player);
            }
        }
        public void SetMimicSetChoice(int level, int choice) {
            mimicSetChoices = (mimicSetChoices & ~(3 << level * 2)) | ((choice & 3) << level * 2);
        }
        public int GetMimicSetChoice(int level) {
            return (mimicSetChoices >> level * 2) & 3;
        }
    }
}
