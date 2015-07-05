//
// Author:
//   Andreas Suter (andy@edelweissinteractive.com)
//
// Copyright (C) 2013-2014 Edelweiss Interactive (http://www.edelweissinteractive.com)
//

/// <summary>
/// Safe coroutine states.
/// </summary>
public enum SafeCoroutineState {

	/// <summary>
	/// Internal state to fill the gap between the creation and the running.
	/// </summary>
	Created,

	/// <summary>
	/// State for executing coroutines.
	/// </summary>
	Running,

	/// <summary>
	/// State for paused coroutines.
	/// </summary>
	Paused,

	/// <summary>
	/// State for regularly finished coroutines.
	/// </summary>
	Finished,

	/// <summary>
	/// State for terminated coroutines which was stopped through a method call on the coroutine.
	/// </summary>
	Stopped,

	/// <summary>
	/// State for coroutines in which an exception was thrown.
	/// </summary>
	ThrewException
}