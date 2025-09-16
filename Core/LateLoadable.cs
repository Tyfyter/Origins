using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Origins.Core {
	internal abstract class LateLoadable : ILoadable {
		public Mod Mod { get; private set; }
		void ILoadable.Load(Mod mod) {
			Mod = mod;
			Origins.lateLoadables.Add(this);
		}
		public virtual bool IsLoadingEnabled(Mod mod) => true;
		public abstract void Load();
		public virtual void Unload() { }
	}
}
