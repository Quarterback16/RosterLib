using System;
using System.Collections;

namespace EricUtility.Iterators
{

	/// <summary>
	/// Isolate this the iteration from the collection. Allows you to 
	/// modify the underlying collection while in the middle of a foreach
	/// </summary>
	public class IterIsolate: IEnumerable
	{
		internal class IterIsolateEnumerator: IEnumerator
		{
			protected ArrayList items;
			protected int currentItem;

			internal IterIsolateEnumerator(IEnumerator enumerator)
			{
					// if this is the enumerator from another iterator, we
					// don't have to enumerate it; we'll just steal the arraylist
					// to use for ourselves. 
				IterIsolateEnumerator chainedEnumerator = 
					enumerator as IterIsolateEnumerator;

				if (chainedEnumerator != null)
				{
					items = chainedEnumerator.items;					
				}
				else
				{
					items = new ArrayList();
					while (enumerator.MoveNext() != false)
					{
						items.Add(enumerator.Current);
					}
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				currentItem = -1;
			}

			public void Reset()
			{
				currentItem = -1;
			}

			public bool MoveNext()
			{
				currentItem++;
				if (currentItem == items.Count)
					return false;

				return true;
			}

			public object Current
			{
				get
				{
					return items[currentItem];
				}
			}
		}

		/// <summary>
		/// Create an instance of the IterIsolate Class
		/// </summary>
		/// <param name="enumerable">A class that implements IEnumerable</param>
		public IterIsolate(IEnumerable enumerable)
		{
			this.enumerable = enumerable;
		}

		public IEnumerator GetEnumerator()
		{
			return new IterIsolateEnumerator(enumerable.GetEnumerator());
		}

		protected IEnumerable enumerable;
	}

//	public class IterIsolateTest
//	{
//		public static void Main()
//		{
//			Console.WriteLine();
//			Console.WriteLine("Testing IterIsolate");
//			Hashtable hash = new Hashtable();
//			hash.Add("A", 1);
//			hash.Add("B", 0);
//			hash.Add("C", 1);
//
//			foreach (string s in new IterIsolate(hash.Keys))
//			{
//				if ((int) hash[s] == 0)
//					hash.Remove(s);
//			}
//
//			foreach (string s in hash.Keys)
//			{
//				Console.WriteLine("Value: {0}", s);
//			}
//
//				// test that chaining them together works...
//			foreach (string s in new IterIsolate(new IterIsolate(hash.Keys)))
//			{
//				Console.WriteLine("Value: {0}", s);
//			}
//		}
//	}
}