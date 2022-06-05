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
        public float explosiveDamage = 1;
        public int explosiveCrit = 4;
        public float explosiveThrowSpeed = 1;
        public float explosiveSelfDamage = 1;
        #endregion

        #region biomes
        public bool ZoneVoid { get; private set; } = false;
        public float ZoneVoidProgress = 0;
        public float ZoneVoidProgressSmoothed = 0;

        public bool ZoneDefiled { get; private set; } = false;
        public float ZoneDefiledProgress = 0;
        public float ZoneDefiledProgressSmoothed = 0;

        public bool ZoneRiven { get; private set; } = false;
        public float ZoneRivenProgress = 0;
        public float ZoneRivenProgressSmoothed = 0;

        public bool ZoneBrine { get; private set; } = false;
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

        public bool drawShirt = false;
        public bool drawPants = false;
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
            if(!Player.frozen) {
                drawShirt = false;
                drawPants = false;
            }
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
                    SoundEngine.PlaySound(SoundID.MaxMana.WithPitch(-1));
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
            explosiveDamage = 1f;
            explosiveCrit = 4;
            explosiveThrowSpeed = 1f;
            explosiveSelfDamage = 1f;
            if(IsExplosive(Player.HeldItem)) {
                explosiveCrit += Player.HeldItem.crit;
            }
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
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
            if(IsExplosive(item))add+=explosiveDamage-1;
            bool ammoBased = item.useAmmo != AmmoID.None || (item.ammo != AmmoID.None && Player.HeldItem.useAmmo == item.ammo);
            if(fiberglassSet) {
                damage.Flat+=ammoBased?2:4;
            }
            if(fiberglassDagger) {
                damage.Flat += ammoBased?4:8;
            }
            if(rivenSet&&item.CountsAsClass(DamageClass.Summon)&&!ItemChecking) {
                damage *= rivenMult;
            }
        }
        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit) {
            if(felnumShock>29) {
                damage+=(int)(felnumShock/15);
                felnumShock = 0;
                SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), target.Center);
            }
        }
		public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		    if(advancedImaging) {
                velocity*=1.3f;
            }
            if(IsExplosive(item)) {
                if(item.useAmmo == 0) {
                    velocity *= explosiveThrowSpeed;
                }
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
            if(item.shoot>ProjectileID.None&&felnumShock>29) {
                Projectile p = new Projectile();
                p.SetDefaults(type);
                OriginGlobalProj.felnumEffectNext = true;
                if(p.CountsAsClass(DamageClass.Melee) || p.aiStyle == 60)
                    return true;
                damage+=(int)(felnumShock/15);
                felnumShock = 0;
                SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), position);
            }
            return true;
        }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(Origins.ExplosiveModOnHit[proj.type]) {
                damage = (int)(damage*(Player.allDamage+explosiveDamage-1)*0.7f);
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
            if(proj.owner == Player.whoAmI && proj.friendly && OriginGlobalProj.IsExplosiveProjectile(proj)) {
                if(minerSet) {
                    explosiveSelfDamage-=0.2f;
                    explosiveSelfDamage*=1/explosiveDamage;
                    //damage = (int)(damage/explosiveDamage);
                    //damage-=damage/5;
                }
                damage = (int)(damage*explosiveSelfDamage);
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
                Player.AddBuff(ModContent.BuffType<Defiled_Exhaustion_Buff>(), 10);
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
                OriginWorld originWorld = ModContent.GetInstance<OriginWorld>();
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
        #region biomes
        public override void UpdateBiomes() {
            ZoneVoid = OriginWorld.voidTiles > 300;
            ZoneVoidProgress = Math.Min(OriginWorld.voidTiles - 200, 200)/300f;

            ZoneDefiled = OriginWorld.defiledTiles > DefiledWastelands.NeededTiles;
            ZoneDefiledProgress = Math.Min(OriginWorld.defiledTiles - (DefiledWastelands.NeededTiles-DefiledWastelands.ShaderTileCount), DefiledWastelands.ShaderTileCount)/DefiledWastelands.ShaderTileCount;

            ZoneRiven = OriginWorld.rivenTiles > RivenHive.NeededTiles;
            ZoneRivenProgress = Math.Min(OriginWorld.rivenTiles - (RivenHive.NeededTiles-RivenHive.ShaderTileCount), RivenHive.ShaderTileCount)/RivenHive.ShaderTileCount;

            ZoneBrine = OriginWorld.brineTiles > BrinePool.NeededTiles;

            LinearSmoothing(ref ZoneVoidProgressSmoothed, ZoneVoidProgress, OriginWorld.biomeShaderSmoothing);
            LinearSmoothing(ref ZoneDefiledProgressSmoothed, ZoneDefiledProgress, OriginWorld.biomeShaderSmoothing);
            LinearSmoothing(ref ZoneRivenProgressSmoothed, ZoneRivenProgress, OriginWorld.biomeShaderSmoothing*0.1f);
        }
        public override bool CustomBiomesMatch(Player other) {
            OriginPlayer modOther = other.GetModPlayer<OriginPlayer>();
            return !((ZoneVoid^modOther.ZoneVoid) || (ZoneDefiled ^ modOther.ZoneDefiled) || (ZoneRiven ^ modOther.ZoneRiven) || (ZoneBrine ^ modOther.ZoneBrine));
        }
        public override void SendCustomBiomes(BinaryWriter writer) {
            byte flags = 0;
            if (ZoneVoid)
                flags |= 1;
            if (ZoneDefiled)
                flags |= 2;
            if (ZoneRiven)
                flags |= 4;
            if (ZoneBrine)
                flags |= 8;
            writer.Write(flags);
        }
        public override void ReceiveCustomBiomes(BinaryReader reader) {
            byte flags = reader.ReadByte();
            ZoneVoid = ((flags & 1) != 0);
            ZoneDefiled = ((flags & 2) != 0);
            ZoneRiven = ((flags & 4) != 0);
            ZoneBrine = ((flags & 8) != 0);
        }
        public override void CopyCustomBiomesTo(Player other) {
            OriginPlayer modOther = other.GetModPlayer<OriginPlayer>();
            //modOther.ZoneVoidTime = ZoneVoidTime;
            modOther.ZoneVoid = ZoneVoid;
            modOther.ZoneDefiled = ZoneDefiled;
            modOther.ZoneRiven = ZoneRiven;
            modOther.ZoneBrine = ZoneBrine;
        }
        public override void UpdateBiomeVisuals() {
            if(ZoneVoidProgressSmoothed > 0)Filters.Scene["Origins:ZoneDusk"].GetShader().UseProgress(ZoneVoidProgressSmoothed);
            if(ZoneDefiledProgressSmoothed > 0)Filters.Scene["Origins:ZoneDefiled"].GetShader().UseProgress(ZoneDefiledProgressSmoothed);
            if(ZoneRivenProgressSmoothed > 0)Filters.Scene["Origins:ZoneRiven"].GetShader().UseProgress(ZoneRivenProgressSmoothed);
            Player.ManageSpecialBiomeVisuals("Origins:ZoneDusk", ZoneVoidProgressSmoothed>0, Player.Center);
            Player.ManageSpecialBiomeVisuals("Origins:ZoneDefiled", ZoneDefiledProgressSmoothed>0, Player.Center);
            Player.ManageSpecialBiomeVisuals("Origins:ZoneRiven", ZoneRivenProgressSmoothed>0, Player.Center);
        }
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
            int num7 = 300 / poolSize;
            int num8 = 1050 / poolSize;
            int num10 = 4500 / poolSize;
            if (num7 < 3) {
                num7 = 3;
            }
            if (num8 < 4) {
                num8 = 4;
            }
            if (num10 < 6) {
                num10 = 6;
            }
            bool flag4 = false;
            bool flag5 = false;
            bool flag7 = false;
            if (Main.rand.NextBool(num7)) {
                flag4 = true;
            }
            if (Main.rand.NextBool(num8)) {
                flag5 = true;
            }
            if (Main.rand.NextBool(num10)) {
                flag7 = true;
            }
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
                if (flag7 && Main.hardMode && Main.rand.NextBool(2)) {
                    caughtType = ModContent.ItemType<Knee_Slapper>();
                } else if (flag4 && !flag5) {
                    caughtType = ModContent.ItemType<Prikish>();
                }
            }
		}
		#endregion
		public override bool PreItemCheck() {
            ItemChecking = true;
            return true;
        }
        public override void PostItemCheck() {
            ItemChecking = false;
        }
        public override void ModifyDrawLayers(List<PlayerLayer> layers) {
            if(drawShirt) {
                int itemindex = layers.IndexOf(PlayerLayer.HeldItem);
                PlayerLayer itemlayer = layers[itemindex];
                layers.RemoveAt(itemindex);
                layers.Insert(layers.IndexOf(PlayerLayer.MountFront), itemlayer);
                layers.Insert(layers.IndexOf(PlayerLayer.MountFront), PlayerShirt);
                PlayerShirt.visible = true;
            }
            if(drawPants) {
                layers.Insert(layers.IndexOf(PlayerLayer.Legs), PlayerPants);
                PlayerPants.visible = true;
            }
            if(felnumShock>0) {
                layers.Add(FelnumGlow);
                FelnumGlow.visible = true;
            }
            if (eyndumSet) {
                if (eyndumCore?.Value?.ModItem is Eyndum_Core equippedCore) {
                    layers.Insert(layers.IndexOf(PlayerLayer.Body) + 1, CreateEyndumCoreLayer(equippedCore.CoreGlowColor));
                }
            }
            if (Origins.HelmetGlowMasks.TryGetValue(Player.head, out Texture2D helmetMask)) {
                layers.Insert(layers.IndexOf(PlayerLayer.Head) + 1, CreateHeadGlowmask(helmetMask));
            }
            if (Origins.BreastplateGlowMasks.TryGetValue(Player.Male ? Player.body : -Player.body, out Texture2D breastplateMask)) {
                layers.Insert(layers.IndexOf(PlayerLayer.Body) + 1, CreateBodyGlowmask(breastplateMask));
            } else if (Origins.BreastplateGlowMasks.TryGetValue(Player.Male ? -Player.body : Player.body, out Texture2D fBreastplateMask)) {
                layers.Insert(layers.IndexOf(PlayerLayer.Body) + 1, CreateBodyGlowmask(fBreastplateMask));
            }
            if (Origins.LeggingGlowMasks.TryGetValue(Player.legs, out Texture2D leggingMask)) {
                layers.Insert(layers.IndexOf(PlayerLayer.Legs) + 1, CreateLegsGlowmask(leggingMask));
            }
            if (Player.itemAnimation != 0 && Player.HeldItem.ModItem is ICustomDrawItem) {
                switch(Player.HeldItem.useStyle) {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    break;

                    default:
                    case 5:
                    layers[layers.IndexOf(PlayerLayer.HeldItem)] = CustomShootLayer;
                    CustomShootLayer.visible = true;
                    break;
                    /*default:
                    layers[layers.IndexOf(PlayerLayer.HeldItem)] = SlashWrenchLayer;
                    SlashWrenchLayer.visible = true;
                    break;*/
                }
            }
            if(itemLayerWrench && !Player.HeldItem.noUseGraphic) {
                switch(Player.HeldItem.useStyle) {
                    case 5:
                    layers[layers.IndexOf(PlayerLayer.HeldItem)] = ShootWrenchLayer;
                    ShootWrenchLayer.visible = true;
                    break;
                    default:
                    layers[layers.IndexOf(PlayerLayer.HeldItem)] = SlashWrenchLayer;
                    SlashWrenchLayer.visible = true;
                    break;
                }
            }
            itemLayerWrench = false;
            if (Player.HeldItem.ModItem is Chocolate_Bar animator) {
                layers.Add(new PlayerLayer("Origins (debugging tool)", "animator", (v)=>animator.DrawAnimations(v)));
            }
        }
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
        //public static PlayerLayer PlagueEyes = new PlayerLayer("Origins", "PlagueEyes", null, (drawInfo)=> {drawInfo.eyeColor = Color.Goldenrod;});
        public static PlayerLayer PlayerShirt = new PlayerLayer("Origins", "PlayerShirt", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            Vector2 Position = drawInfo2.position;
            SpriteEffects spriteEffects = drawInfo2.spriteEffects;
            int skinVariant = drawPlayer.skinVariant;
            DrawData drawData;
            drawData = new DrawData(Main.playerTextures[skinVariant, 14], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo2.legOrigin, new Rectangle?(drawPlayer.legFrame), drawInfo2.shirtColor, drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
            Main.playerDrawData.Add(drawData);
            if(!drawPlayer.Male) {
                drawData = new DrawData(Main.playerTextures[skinVariant, 4], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.underShirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                Main.playerDrawData.Add(drawData);
                drawData = new DrawData(Main.playerTextures[skinVariant, 6], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.shirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                Main.playerDrawData.Add(drawData);
            } else {
                drawData = new DrawData(Main.playerTextures[skinVariant, 4], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.underShirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                Main.playerDrawData.Add(drawData);
                drawData = new DrawData(Main.playerTextures[skinVariant, 6], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.shirtColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                Main.playerDrawData.Add(drawData);
            }
            drawData = new DrawData(Main.playerTextures[skinVariant, 5], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + new Vector2(drawPlayer.bodyFrame.Width / 2, drawPlayer.bodyFrame.Height / 2), new Rectangle?(drawPlayer.bodyFrame), drawInfo2.bodyColor, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
            Main.playerDrawData.Add(drawData);
        });
        public static PlayerLayer PlayerPants = new PlayerLayer("Origins", "PlayerPants", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            Vector2 Position = drawInfo2.position;
            SpriteEffects spriteEffects = drawInfo2.spriteEffects;
            int skinVariant = drawPlayer.skinVariant;
            DrawData drawData;
            drawData = new DrawData(Main.playerTextures[skinVariant, 11], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo2.legOrigin, new Rectangle?(drawPlayer.legFrame), drawInfo2.pantsColor, drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
            Main.playerDrawData.Add(drawData);
            drawData = new DrawData(Main.playerTextures[skinVariant, 12], new Vector2((int)(Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2 + drawPlayer.width / 2), (int)(Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo2.legOrigin, new Rectangle?(drawPlayer.legFrame), drawInfo2.shoeColor, drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
            Main.playerDrawData.Add(drawData);
        });
        public static PlayerLayer ShootWrenchLayer = new PlayerLayer("Origins", "FiberglassBowLayer", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            Item item = drawPlayer.inventory[drawPlayer.selectedItem];
            Texture2D itemTexture = TextureAssets.Item[item.type].Value;
            AnimatedModItem aItem = (AnimatedModItem)item.ModItem;
            int drawXPos = 10;
            Vector2 ItemCenter = new Vector2(itemTexture.Width / 2, itemTexture.Height / 2);
            Vector2 drawItemPos = OriginExtensions.DrawPlayerItemPos(drawPlayer.gravDir, item.type);
            drawXPos = (int)drawItemPos.X;
            ItemCenter.Y = drawItemPos.Y;
            Vector2 drawOrigin = new Vector2(-drawXPos, itemTexture.Height / 2);
            if(drawPlayer.direction == -1) {
                drawOrigin = new Vector2(itemTexture.Width + drawXPos, itemTexture.Height / 2);
            }
            drawOrigin.X-=drawPlayer.width/2;
            Vector4 col = drawInfo2.faceColor.ToVector4()/drawPlayer.skinColor.ToVector4();
            DrawData value = new DrawData(itemTexture, new Vector2((int)(drawInfo2.itemLocation.X - Main.screenPosition.X + ItemCenter.X), (int)(drawInfo2.itemLocation.Y - Main.screenPosition.Y + ItemCenter.Y)), aItem.Animation.GetFrame(itemTexture), item.GetAlpha(new Color(col.X, col.Y, col.Z, col.W)), drawPlayer.itemRotation, drawOrigin, item.scale, drawInfo2.spriteEffects, 0);
            Main.playerDrawData.Add(value);
            if(drawPlayer.inventory[drawPlayer.selectedItem].glowMask != -1) {
                value = new DrawData(TextureAssets.GlowMask[item.glowMask].Value, new Vector2((int)(drawInfo2.itemLocation.X - Main.screenPosition.X + ItemCenter.X), (int)(drawInfo2.itemLocation.Y - Main.screenPosition.Y + ItemCenter.Y)), aItem.Animation.GetFrame(itemTexture), item.GetAlpha(aItem.GlowmaskTint??new Color(col.X, col.Y, col.Z, col.W)), drawPlayer.itemRotation, drawOrigin, item.scale, drawInfo2.spriteEffects, 0);
                Main.playerDrawData.Add(value);
            }
        });
        public static PlayerLayer SlashWrenchLayer = new PlayerLayer("Origins", "FelnumBroadswordLayer", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            float num77 = drawPlayer.itemRotation + MathHelper.PiOver4 * drawPlayer.direction;
            Item item = drawPlayer.inventory[drawPlayer.selectedItem];
            Texture2D itemTexture = TextureAssets.Item[item.type].Value;
            AnimatedModItem aItem = (AnimatedModItem)item.ModItem;
            Rectangle frame = aItem.Animation.GetFrame(itemTexture);
            Color currentColor = Lighting.GetColor((int)(drawInfo2.position.X + drawPlayer.width * 0.5) / 16, (int)((drawInfo2.position.Y + drawPlayer.height * 0.5) / 16.0));
            SpriteEffects spriteEffects = (drawPlayer.direction==1 ? 0 : SpriteEffects.FlipHorizontally) | (drawPlayer.gravDir==1f ? 0 : SpriteEffects.FlipVertically);
            DrawData value = new DrawData(itemTexture, new Vector2((int)(drawInfo2.itemLocation.X - Main.screenPosition.X), (int)(drawInfo2.itemLocation.Y - Main.screenPosition.Y)), frame, drawPlayer.inventory[drawPlayer.selectedItem].GetAlpha(currentColor), drawPlayer.itemRotation, new Vector2(frame.Width * 0.5f - frame.Width * 0.5f * drawPlayer.direction, frame.Height), drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
            Main.playerDrawData.Add(value);
            if(drawPlayer.inventory[drawPlayer.selectedItem].color != default) {
                value = new DrawData(itemTexture, new Vector2((int)(drawInfo2.itemLocation.X - Main.screenPosition.X), (int)(drawInfo2.itemLocation.Y - Main.screenPosition.Y)), frame, drawPlayer.inventory[drawPlayer.selectedItem].GetColor(currentColor), drawPlayer.itemRotation, new Vector2(frame.Width * 0.5f - frame.Width * 0.5f * drawPlayer.direction, frame.Height), drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
                Main.playerDrawData.Add(value);
            }
            if(drawPlayer.inventory[drawPlayer.selectedItem].glowMask != -1) {
                value = new DrawData(TextureAssets.GlowMask[drawPlayer.inventory[drawPlayer.selectedItem].glowMask].Value, new Vector2((int)(drawInfo2.itemLocation.X - Main.screenPosition.X), (int)(drawInfo2.itemLocation.Y - Main.screenPosition.Y)), frame, aItem.GlowmaskTint??new Color(250, 250, 250, item.alpha), drawPlayer.itemRotation, new Vector2(frame.Width * 0.5f - frame.Width * 0.5f * drawPlayer.direction, frame.Height), drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
                Main.playerDrawData.Add(value);
            }
        });
        public static PlayerLayer FelnumGlow = new PlayerLayer("Origins", "FelnumGlow", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            Vector2 Position;
            Rectangle? Frame;
            Texture2D Texture;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if(drawPlayer.direction == -1) {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }
            if(drawPlayer.gravDir == -1f) {
                spriteEffects |= SpriteEffects.FlipVertically;
            }
            DrawData item;
            int a = (int)Math.Max(Math.Min((drawPlayer.GetModPlayer<OriginPlayer>().felnumShock*255)/drawPlayer.statLifeMax2, 255), 1);
            if(drawPlayer.head == Origins.FelnumHeadArmorID) {
                Position = new Vector2(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f, drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.headPosition + drawInfo2.headOrigin;
                Frame = new Rectangle?(drawPlayer.bodyFrame);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Head");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.headRotation, drawInfo2.headOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
                Main.playerDrawData.Add(item);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Eye");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.headRotation, drawInfo2.headOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
                Main.playerDrawData.Add(item);
            } else if(drawInfo2.drawHair||drawInfo2.drawAltHair||drawPlayer.head == ArmorIDs.Head.FamiliarWig) {
                Position = new Vector2(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f, drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.headPosition + drawInfo2.headOrigin;
                Frame = new Rectangle?(drawPlayer.bodyFrame);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Eye");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.headRotation, drawInfo2.headOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
                Main.playerDrawData.Add(item);
            }
            if(drawPlayer.body == Origins.FelnumBodyArmorID) {
                Position = new Vector2(((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo2.bodyOrigin;
                Frame = new Rectangle?(drawPlayer.bodyFrame);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Arms");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
                Main.playerDrawData.Add(item);

                Position = new Vector2(((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo2.bodyOrigin;
                Frame = new Rectangle?(drawPlayer.bodyFrame);
                Texture = ModContent.GetTexture(drawPlayer.Male ? "Origins/Items/Armor/Felnum/Felnum_Glow_Body" : "Origins/Items/Armor/Felnum/Felnum_Glow_FemaleBody");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
                Main.playerDrawData.Add(item);
            }
            if(drawPlayer.legs == Origins.FelnumLegsArmorID) {
                Position = new Vector2((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo2.legOrigin;
                Frame = new Rectangle?(drawPlayer.legFrame);
                Texture = ModContent.GetTexture("Origins/Items/Armor/Felnum/Felnum_Glow_Legs");
                item = new DrawData(Texture, Position, Frame, new Color(a, a, a, a), drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[2].type);
                Main.playerDrawData.Add(item);
            }
        });
        public static PlayerLayer CreateHeadGlowmask(Texture2D texture) => new PlayerLayer("Origins", "HeadGlowmask", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (drawPlayer.direction == -1) {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }
            if (drawPlayer.gravDir == -1f) {
                spriteEffects |= SpriteEffects.FlipVertically;
            }
            Vector2 Position = new Vector2((int)(drawInfo2.position.X + (float)drawPlayer.width / 2f - (float)drawPlayer.bodyFrame.Width / 2f - Main.screenPosition.X), (int)(drawInfo2.position.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f - Main.screenPosition.Y)) + drawPlayer.headPosition + drawInfo2.headOrigin;
            //Vector2 Position = new Vector2(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f, drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f) + drawPlayer.headPosition + drawInfo2.headOrigin;
            Rectangle? Frame = new Rectangle?(drawPlayer.bodyFrame);
            DrawData item = new DrawData(texture, Position, Frame, Color.White, drawPlayer.headRotation, drawInfo2.headOrigin, 1f, spriteEffects, 0);
            item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[0].type);
            Main.playerDrawData.Add(item);
        }) {visible = true};
        public static PlayerLayer CreateBodyGlowmask(Texture2D texture) => new PlayerLayer("Origins", "BodyGlowmask", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (drawPlayer.direction == -1) {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }
            if (drawPlayer.gravDir == -1f) {
                spriteEffects |= SpriteEffects.FlipVertically;
            }
            Vector2 Position = new Vector2(((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo2.bodyOrigin;
            Rectangle? Frame = new Rectangle?(drawPlayer.bodyFrame);
            DrawData item = new DrawData(texture, Position, Frame, Color.White, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
            item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
            Main.playerDrawData.Add(item);
        }) { visible = true };
        public static PlayerLayer CreateLegsGlowmask(Texture2D texture) => new PlayerLayer("Origins", "LegsGlowmask", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (drawPlayer.direction == -1) {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }
            if (drawPlayer.gravDir == -1f) {
                spriteEffects |= SpriteEffects.FlipVertically;
            }
            Vector2 Position = new Vector2((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo2.legOrigin;
            Rectangle? Frame = new Rectangle?(drawPlayer.legFrame);
            DrawData item = new DrawData(texture, Position, Frame, Color.White, drawPlayer.legRotation, drawInfo2.legOrigin, 1f, spriteEffects, 0);
            item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[2].type);
            Main.playerDrawData.Add(item);
        }) { visible = true };
        public static PlayerLayer CreateEyndumCoreLayer(Color color) => new PlayerLayer("Origins", "EyndumCore", null, delegate (PlayerDrawInfo drawInfo2) {
            Player drawPlayer = drawInfo2.drawPlayer;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (drawPlayer.direction == -1) {
                spriteEffects |= SpriteEffects.FlipHorizontally;
            }
            if (drawPlayer.gravDir == -1f) {
                spriteEffects |= SpriteEffects.FlipVertically;
            }
            Vector2 Position = new Vector2(((int)(drawInfo2.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f)), (int)(drawInfo2.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.bodyPosition + drawInfo2.bodyOrigin;
            Rectangle? Frame = new Rectangle?(drawPlayer.bodyFrame);
            DrawData item = new DrawData(Origins.eyndumCoreTexture, Position, Frame, color, drawPlayer.bodyRotation, drawInfo2.bodyOrigin, 1f, spriteEffects, 0);
            item.shader = GameShaders.Armor.GetShaderIdFromItemId(drawPlayer.dye[1].type);
            Main.playerDrawData.Add(item);
        }) { visible = true };
    }
}
