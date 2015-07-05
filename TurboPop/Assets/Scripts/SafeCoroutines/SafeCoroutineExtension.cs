//
// Author:
//   Andreas Suter (andy@edelweissinteractive.com)
//
// Copyright (C) 2013-2014 Edelweiss Interactive (http://www.edelweissinteractive.com)
//

using UnityEngine;
using System.Collections;

/// <summary>
/// Safe coroutine extension methods.
/// </summary>
public static class SafeCoroutineExtension {

	/// <summary>
	/// Starts the safe coroutine.
	/// </summary>
	/// <returns>The safe coroutine.</returns>
	/// <param name="a_MonoBehaviour">A mono behaviour.</param>
	/// <param name="a_SafeCoroutine">A safe coroutine.</param>
	public static SafeCoroutine StartSafeCoroutine (this MonoBehaviour a_MonoBehaviour, IEnumerator a_SafeCoroutine) {
		SafeCoroutine l_Result = new SafeCoroutine (a_SafeCoroutine, a_MonoBehaviour.gameObject);
		a_MonoBehaviour.StartCoroutine (l_Result.WrappedCoroutine);
		return (l_Result);
	}

	/// <summary>
	/// Starts the safe coroutine.
	/// </summary>
	/// <returns>The safe coroutine.</returns>
	/// <param name="a_MonoBehaviour">A mono behaviour.</param>
	/// <param name="a_SafeCoroutine">A safe coroutine.</param>
	/// <typeparam name="TResult">The result type parameter.</typeparam>
	public static SafeCoroutine <TResult> StartSafeCoroutine <TResult> (this MonoBehaviour a_MonoBehaviour, IEnumerator a_SafeCoroutine) {
		SafeCoroutine <TResult> l_Result;
		try {
			l_Result = new SafeCoroutine <TResult> (a_SafeCoroutine, a_MonoBehaviour.gameObject);
		} catch {
			throw;
		}
		a_MonoBehaviour.StartCoroutine (l_Result.WrappedCoroutine);
		return (l_Result);
	}
}
