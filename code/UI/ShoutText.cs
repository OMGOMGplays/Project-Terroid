using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PTR.UI 
{
	public class ShoutText : WorldPanel 
	{
		public ShoutText() 
		{
			StyleSheet.Load("/ui/ShoutText.scss");
			Add.Label("I'M OVER HERE!");
		}
	}
}
