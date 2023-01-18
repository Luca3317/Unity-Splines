using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
	[System.Serializable]
	public abstract class Singleton<T> where T : Singleton<T>
	{
		private static readonly System.Lazy<T> Lazy =
			new(() => (System.Activator.CreateInstance(typeof(T), true) as T)!);

		public static T Instance => Lazy.Value;
	}
}