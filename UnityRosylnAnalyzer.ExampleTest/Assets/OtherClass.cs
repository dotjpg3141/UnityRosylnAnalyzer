using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

static class OtherClass
{

	public static IEnumerator IEnumeratorCoroutine()
	{
		yield break;
	}

	public static IEnumerator<object> IEnumeratorGenericCoroutine()
	{
		yield break;
	}

}
