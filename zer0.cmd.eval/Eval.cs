using Microsoft.CodeAnalysis.CSharp.Scripting;

using zer0.core;

namespace zer0.cmd.eval
{
	internal sealed class Eval : ActionBase
	{
		private readonly string _args;
		public string Command => _args;

		public Eval(string args)
		{
			_args = args;
		}

		public override string Log() => $"{Command} --> {CSharpScript.EvaluateAsync(Command).Result}";
	}

	// HINT: example of constructor param injection
	//[Export(nameof(Eval), typeof(IAction))]
	//public class Eval : Action
	//{
	//	public Arguments Arg;

	//	// фабрика

	//	[ImportingConstructor]
	//	public Eval(
	//		[Import(typeof(Arguments), AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.NonShared)]
	//		Arguments args)
	//	{
	//		Arg = args;
	//	}
	//}
}
