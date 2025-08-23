using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod.ColoredDamageTypes {
	[Autoload(false)]
	public class SentryExplosive : DamageClass { }
	public class ColoredDamageTypes : ModSystem {
		private static DamageClass explosiveSentry;
		public static DamageClass ExplosiveSentry => explosiveSentry ??= ModContent.GetInstance<SentryExplosive>();
		public static Mod ColoredTypesMod;
		public static Color novaTipColor = new(234, 56, 103);
		public static Color novaDmgColor = new(234, 56, 103);
		public static Color novaCritColor = new(235, 0, 59);
		public static Color explTipColor = new(234, 105, 91);
		public static Color explDmgColor = new(234, 105, 91);
		public static Color explCritColor = new(173, 78, 67);
		public static Dictionary<DamageClass, (Color tipColor, Color dmgColor, Color critColor)> colors = [];

		public override bool IsLoadingEnabled(Mod mod) {
			return ModLoader.HasMod("ColoredDamageTypes");
		}
		public override void Load() {
			Mod.AddContent(explosiveSentry = new SentryExplosive());
			if (ModLoader.TryGetMod("ColoredDamageTypes", out ColoredTypesMod)) {
				static bool PushesDamageClass(ILContext il, Instruction instruction) {
					if (instruction.MatchLdarg(out int arg)) return il.Method.Parameters[arg].ParameterType.FullName == il.Import(typeof(DamageClass)).FullName;
					if (instruction.MatchLdloc(out int loc)) return il.Body.Variables[loc].VariableType.FullName == il.Import(typeof(DamageClass)).FullName;
					if (instruction.MatchCallOrCallvirt(out MethodReference method)) return method.ReturnType.FullName == il.Import(typeof(DamageClass)).FullName;
					if (instruction.MatchLdfld(out FieldReference field) || instruction.MatchLdsfld(out field)) return field.FieldType.FullName == il.Import(typeof(DamageClass)).FullName;
					return false;
				}
				static void FixMethods(ILContext il) {
					ILCursor c = new(il);
					while (c.TryGotoNext(MoveType.Before,
						i => i.MatchCallOrCallvirt<object>(nameof(ToString)) && PushesDamageClass(il, i.Previous)
					)) {
						c.Remove();
						c.EmitCallvirt(typeof(ModType).GetMethod("get_" + nameof(ModType.FullName)));
					}
				}
				foreach (MethodInfo method in ColoredTypesMod.GetType().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
					MonoModHooks.Modify(method, FixMethods);
				}
				foreach (MethodInfo method in ColoredTypesMod.Code.GetType("ColoredDamageTypes.DamageTypes")?.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) ?? []) {
					MonoModHooks.Modify(method, FixMethods);
				}
				//MonoModHooks.Modify(coloredDamageTypes.GetType().GetMethod("LoadModdedDamageTypes", BindingFlags.Public | BindingFlags.Static), FixMethods);
			}
		}
		public override void PostSetupContent() {
			Type DamageConfig = ColoredTypesMod.Code.GetType("ColoredDamageTypes.DamageConfig");
			Type TooltipConfig = ColoredTypesMod.Code.GetType("ColoredDamageTypes.TooltipsConfig");

			FieldInfo VanillaDmg = DamageConfig.GetField("VanillaDmg");
			FieldInfo VanillaTT = TooltipConfig.GetField("VanillaTT");

			object VanillaInstance = VanillaDmg.GetValue(DamageConfig.GetField("Instance").GetValue(null));
			object VanillaTTInstance = VanillaTT.GetValue(TooltipConfig.GetField("Instance").GetValue(null));

			FieldInfo VanillaMeleeDmg = VanillaDmg.FieldType.GetField("MeleeDmg");
			FieldInfo VanillaRangeDmg = VanillaDmg.FieldType.GetField("RangedDmg");
			FieldInfo VanillaMagicDmg = VanillaDmg.FieldType.GetField("MagicDmg");
			FieldInfo VanillaThrowDmg = VanillaDmg.FieldType.GetField("ThrowingDmg");
			FieldInfo VanillaSummonDmg = VanillaDmg.FieldType.GetField("SummonDmg");
			FieldInfo VanillaSentryDmg = VanillaDmg.FieldType.GetField("SentryDmg");

			Color MeleeTTColor = (Color)VanillaTT.FieldType.GetField("TooltipMelee").GetValue(VanillaTTInstance);
			Color RangeTTColor = (Color)VanillaTT.FieldType.GetField("TooltipRanged").GetValue(VanillaTTInstance);
			Color MagicTTColor = (Color)VanillaTT.FieldType.GetField("TooltipMagic").GetValue(VanillaTTInstance);
			Color ThrowTTColor = (Color)VanillaTT.FieldType.GetField("TooltipThrowing").GetValue(VanillaTTInstance);
			Color SummonTTColor = (Color)VanillaTT.FieldType.GetField("TooltipSummon").GetValue(VanillaTTInstance);
			Color SentryTTColor = (Color)VanillaTT.FieldType.GetField("TooltipSentry").GetValue(VanillaTTInstance);

			Color MeleeColor = (Color)VanillaMeleeDmg.FieldType.GetField("MeleeDamage").GetValue(VanillaMeleeDmg.GetValue(VanillaInstance));
			Color MeleeCritColor = (Color)VanillaMeleeDmg.FieldType.GetField("MeleeDamageCrit").GetValue(VanillaMeleeDmg.GetValue(VanillaInstance));
			Color RangeColor = (Color)VanillaRangeDmg.FieldType.GetField("RangedDamage").GetValue(VanillaRangeDmg.GetValue(VanillaInstance));
			Color RangeCritColor = (Color)VanillaRangeDmg.FieldType.GetField("RangedDamageCrit").GetValue(VanillaRangeDmg.GetValue(VanillaInstance));
			Color MagicColor = (Color)VanillaMagicDmg.FieldType.GetField("MagicDamage").GetValue(VanillaMagicDmg.GetValue(VanillaInstance));
			Color MagicCritColor = (Color)VanillaMagicDmg.FieldType.GetField("MagicDamageCrit").GetValue(VanillaMagicDmg.GetValue(VanillaInstance));
			Color ThrowColor = (Color)VanillaThrowDmg.FieldType.GetField("ThrowingDamage").GetValue(VanillaThrowDmg.GetValue(VanillaInstance));
			Color ThrowCritColor = (Color)VanillaThrowDmg.FieldType.GetField("ThrowingDamageCrit").GetValue(VanillaThrowDmg.GetValue(VanillaInstance));
			Color SummonColor = (Color)VanillaSummonDmg.FieldType.GetField("SummonDamage").GetValue(VanillaSummonDmg.GetValue(VanillaInstance));
			Color SummonCritColor = (Color)VanillaSummonDmg.FieldType.GetField("SummonDamageCrit").GetValue(VanillaSummonDmg.GetValue(VanillaInstance));
			Color SentryColor = (Color)VanillaSentryDmg.FieldType.GetField("SentryDamage").GetValue(VanillaSentryDmg.GetValue(VanillaInstance));
			Color SentryCritColor = (Color)VanillaSentryDmg.FieldType.GetField("SentryDamageCrit").GetValue(VanillaSentryDmg.GetValue(VanillaInstance));

			DamageClass sentryClass = ColoredTypesMod.Find<DamageClass>("SentryClass");
			colors.Add(DamageClass.Melee, (MeleeTTColor, MeleeColor, MeleeCritColor));
			colors.Add(DamageClass.MeleeNoSpeed, (MeleeTTColor, MeleeColor, MeleeCritColor));
			colors.Add(DamageClass.Ranged, (RangeTTColor, RangeColor, RangeCritColor));
			colors.Add(DamageClass.Magic, (MagicTTColor, MagicColor, MagicCritColor));
			colors.Add(DamageClass.Throwing, (ThrowTTColor, ThrowColor, ThrowCritColor));
			colors.Add(DamageClass.Summon, (SummonTTColor, SummonColor, SummonCritColor));
			colors.Add(DamageClass.SummonMeleeSpeed, (SummonTTColor, SummonColor, SummonCritColor));
			colors.Add(sentryClass, (SentryTTColor, SentryColor, SentryCritColor));

			Type CustomConfig = ColoredTypesMod.Code.GetType("ColoredDamageTypes.zCrossModConfig");
			FieldInfo CrossDmg = CustomConfig.GetField("CrossModDamageConfig");
			IDictionary CustomInstance = (IDictionary)CrossDmg.GetValue(CustomConfig.GetField("Instance").GetValue(null));
			Type DictValue = CrossDmg.FieldType.GenericTypeArguments[1];
			FieldInfo TooltipColor = DictValue.GetField("TooltipColor");
			FieldInfo DamageColor = DictValue.GetField("DamageColor");
			FieldInfo CritDamageColor = DictValue.GetField("CritDamageColor");

			foreach (DictionaryEntry obj in CustomInstance) {
				DamageClass dClass = ModContent.Find<DamageClass>((string)obj.Key);
				if (DamageClasses.ExplosiveVersion.ContainsKey(dClass)) {
					Color tipColor = (Color)TooltipColor.GetValue(obj.Value);
					Color dmgColor = (Color)DamageColor.GetValue(obj.Value);
					Color critColor = (Color)CritDamageColor.GetValue(obj.Value);
					colors.Add(dClass, (tipColor, dmgColor, critColor));
				}
			}

			if (DamageClasses.ExplosiveVersion.ContainsKey(sentryClass)) DamageClasses.ExplosiveVersion[sentryClass] = ExplosiveSentry;
			else DamageClasses.ExplosiveVersion.Add(sentryClass, ExplosiveSentry);

			ColoredTypesMod.Call("AddDamageType",
				DamageClasses.Explosive,
				explTipColor,
				explDmgColor,
				explCritColor
			);
			ColoredTypesMod.Call("AddDamageType",
				DamageClasses.Incantation,
				colors[DamageClass.Summon].dmgColor,
				colors[DamageClass.Summon].dmgColor,
				colors[DamageClass.Summon].critColor
			);
			ColoredTypesMod.Call("AddDamageType",
				DamageClasses.MeleeMagic,
				colors[DamageClass.Melee].dmgColor.MultiplyRGB(colors[DamageClass.Magic].dmgColor),
				colors[DamageClass.Melee].dmgColor.MultiplyRGB(colors[DamageClass.Magic].dmgColor),
				colors[DamageClass.Melee].critColor.MultiplyRGB(colors[DamageClass.Magic].critColor)
			);
			ColoredTypesMod.Call("AddDamageType",
				DamageClasses.RangedMagic,
				colors[DamageClass.Ranged].dmgColor.MultiplyRGB(colors[DamageClass.Magic].dmgColor),
				colors[DamageClass.Ranged].dmgColor.MultiplyRGB(colors[DamageClass.Magic].dmgColor),
				colors[DamageClass.Ranged].critColor.MultiplyRGB(colors[DamageClass.Magic].critColor)
			);
			ColoredTypesMod.Call("AddDamageType",
				ModContent.GetInstance<Chambersite_Mine_Launcher_Damage>(),
				colors[DamageClass.Ranged].dmgColor.MultiplyRGB(explTipColor),
				colors[DamageClass.Ranged].dmgColor.MultiplyRGB(explDmgColor),
				colors[DamageClass.Ranged].critColor.MultiplyRGB(explCritColor)
			);

			foreach (KeyValuePair<DamageClass, (Color tipColor, Color dmgColor, Color critColor)> item in colors) {
				ColoredTypesMod.Call("AddDamageType",
					DamageClasses.ExplosiveVersion[item.Key],
					item.Value.tipColor.MultiplyRGB(explTipColor),
					item.Value.dmgColor.MultiplyRGB(explDmgColor),
					item.Value.critColor.MultiplyRGB(explCritColor)
				);
			}

			/*static void ModifyGlobalItem(ILContext il) {
				ILCursor c = new(il);
				if (c.TryGotoNext(MoveType.After,
					i => i.M
				))
			}*/
			/*static void UsePillarColors(ILContext il) {
				ILCursor c = new(il);
				if (c.TryGotoNext(MoveType.After,
					i => i.MatchStfld(out FieldReference field) && field.Name == "ThrowingDamageCrit"
				)) {
					Type CustomConfig = ColoredTypesMod.Code.GetType("ColoredDamageTypes.zCrossModConfig");
					FieldInfo CrossDmg = CustomConfig.GetField("CrossModDamageConfig");
					IDictionary CustomInstance = (IDictionary)CrossDmg.GetValue(CustomConfig.GetField("Instance").GetValue(null));
					Type DictValue = CrossDmg.FieldType.GenericTypeArguments[1];
					FieldInfo TooltipColor = DictValue.GetField("TooltipColor");
					FieldInfo DamageColor = DictValue.GetField("DamageColor");
					FieldInfo CritDamageColor = DictValue.GetField("CritDamageColor");
					foreach (DictionaryEntry obj in CustomInstance) {
						DamageClass dClass = ModContent.Find<DamageClass>((string)obj.Key);
						if (colors.ContainsKey(dClass) && DamageClasses.ExplosiveVersion.ContainsKey(dClass)) {
							Color tipColor = (Color)TooltipColor.GetValue(obj.Value);
							Color dmgColor = (Color)DamageColor.GetValue(obj.Value);
							Color critColor = (Color)CritDamageColor.GetValue(obj.Value);
							colors[dClass] = (tipColor,  dmgColor, critColor);
						}
					}
				}
			}*/
			/*MethodInfo UpdateTooltips = ColoredTypesMod.Code.GetType("ColoredDamageTypes.ItemChanges")?.GetMethod("UpdateToolTips");
			MonoModHooks.Modify(UpdateTooltips, ModifyGlobalItem);*/
			/*MethodInfo OnChanged = ColoredTypesMod.Code.GetType("ColoredDamageTypes.Config")?.GetMethod("OnChanged");
			MonoModHooks.Modify(OnChanged, UsePillarColors);*/
		}
	}
}
