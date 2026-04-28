/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PegasusLib.Content;

namespace Origins.Core;
public class Mana_Shielding : DamageRedirectionToMana, IBroken {
	static string IBroken.BrokenReason => "Needs balance testing";
	public override float CostMultiplier => Player.manaCost * 2;
	public override float CurrentStrength => Strength * strengthModifier;
	float strengthModifier = 1;
	public override void ResetEffects() {
		base.ResetEffects();
		if (Player.statMana > 0) MathUtils.LinearSmoothing(ref strengthModifier, 1, 1 / 10f);
	}
	public override void DamageMana(int manaDamage) {
		base.DamageMana(manaDamage);
		if (Player.statMana <= 0) strengthModifier = 0;
		player.AddBuff(ModContent.BuffType<Mana_Buffer_Debuff>(), 192);
	}
}*/
