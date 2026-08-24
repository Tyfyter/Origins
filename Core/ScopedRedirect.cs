using System;
using System.Runtime.CompilerServices;

namespace Origins.Core;
public readonly ref struct ScopedRedirect<T> : IDisposable {
	private readonly ref T variable;
	private readonly ref T redirect;
	private readonly T original;
	public ScopedRedirect(FastStaticFieldInfo<T> variable, ref T redirect) : this(ref variable.Value, ref redirect) { }
	public ScopedRedirect(ref T variable, ref T redirect) {
		this.variable = ref variable;
		this.redirect = ref redirect;
		original = variable;
		variable = redirect;
	}
	public void Dispose() {
		if (!Unsafe.IsNullRef(in variable)) {
			redirect = variable;
			variable = original;
		}
	}
}
