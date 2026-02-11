using Origins.Items.Weapons.Demolitionist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Origins.Core {
	public abstract class AutonomousModType<TSelf> : ModType<TSelf, TSelf> where TSelf : AutonomousModType<TSelf> {
		protected sealed override TSelf CreateTemplateEntity() => null;
	}
}
