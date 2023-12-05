using GenericFactoryPool.Interfaces;
using System;
using System.Collections.Concurrent;

namespace GenericFactoryPool;

/// <summary>
/// Provides a generic object pool for creating and recycling objects of various types.
/// Object pooling helps improve performance by reusing objects instead of repeatedly
/// creating and destroying them.The Object Pool has a max size set to 64 by default.
/// </summary>
/// <remarks>
/// <br>The Following is an Example of how to use this Factory Pool.</br>
/// <br>The Example demonstrates a Pool of a List of strings. Though you can create any type you need. </br>
/// <br>Complete Usage Example: </br>
/// <example> <br> </br>
/// <br> <c>GenericFactoryPool.SetMaxCapacity(10);</c> </br>
/// <br> <c>Func&lt;List&lt;string&gt;&gt; listFactory = () => new List&lt;string&gt;();</c> </br>
/// <br> <c>var listPool = GenericFactoryPool.Create(listFactory);</c> </br>
/// <br> <c>listPool.SetResetAction(list => list.Clear());</c> </br>
/// <br> <c>GenericFactoryPool.SetPoolSize&lt;List&lt;string&gt;&gt;(10, listFactory);</c> </br>
/// <br> <c>var myList = listPool.GetObject(listFactory);</c> </br>
/// <br> <c>myList.Add("Item 1");</c> </br>
/// <br> <c>myList.Add("Item 2");</c> </br>
/// <br> <c>int count = GenericFactoryPool.GetPoolCount&lt;List&lt;string&gt;&gt;();</c> </br>
/// <br> <c>GenericFactoryPool.Recycle(myList);</c> </br>
/// <br> <c>GenericFactoryPool.ClearPool&lt;List&lt;string&gt;&gt;();</c> </br>
/// </example> </remarks>
public static class GenericFactoryPool
{
	private static readonly ConcurrentDictionary<Type, object> _pools = new();
	private static int _maxCapacity;

	/// <summary>
	/// <br>Checks the <seealso cref="ObjectPool{T}"/> or creates an object of the specified type using a factory pattern.</br>
	/// <br>If an <seealso cref="ObjectPool{T}"/> exists for the specified type, it will be used to retrieve
	/// a recycled object; otherwise, a new <seealso cref="ObjectPool{T}"/> will be created.</br>
	/// </summary>
	/// <remarks>
	/// <br>Example Usage:</br>
	/// <example><br> </br>
	/// <br> <c>Func&lt;List&lt;string&gt;&gt; listFactory = () => new List&lt;string&gt;();</c> </br>
	/// <br> <c>var listPool = GenericFactoryPool.Create(listFactory);</c> </br>
	/// </example> </remarks>
	/// <typeparam name="T" >"The generic object type."</typeparam>
	/// <param name="factoryMethod">A function that creates and returns an object of type T. </param>
	/// <returns>An object of the specified type.</returns>
	public static T Create<T>(Func<T> factoryMethod)
	{
		ArgumentNullException.ThrowIfNull(factoryMethod);

		if (_pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool)
		{
			return objectPool.GetObject(factoryMethod);
		}
		else
		{
			ObjectPool<T> newObjectPool = new();
			_ = _pools.TryAdd(typeof(T), newObjectPool);
			return newObjectPool.GetObject(factoryMethod);
		}
	}

	/// <summary>
	/// Recycles an object of the specified type back into the object pool if a pool exists.
	/// If the object implements the `IRecyclable` interface, its state is reset before recycling.
	/// </summary>
	/// <remarks>
	/// <br>Example Usage:</br>
	/// <example><br> </br>
	/// <br> <c>GenericFactoryPool.Recycle(myList);</c> </br>
	/// </example> </remarks>
	/// <typeparam name="T">The type of object to recycle.</typeparam>
	/// <param name="item">The object to recycle.</param>
	public static void Recycle<T>(T item)
	{
		ArgumentNullException.ThrowIfNull(item);

		if (_pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool)
		{
			if (GetPoolCount<T>() < _maxCapacity || _maxCapacity == 0)
			{
				objectPool.ReturnObject(item);
			}
		}
	}

	/// <summary>
	/// Sets the Maximum capacity a pool can be,.
	/// The new capacity can be any positive integer including 0 which indicates no capacity limit.
	/// </summary>
	/// <remarks>
	/// <br>Example Usage:</br>
	/// <example><br> </br>
	/// <br> <c>GenericFactoryPool.SetMaxCapacity(10);</c> </br>
	/// </example> </remarks>
	/// <param name="newCapacity"></param>
	public static void SetMaxCapacity(int newCapacity)
	{
		if (newCapacity < 0)
		{
			throw new ArgumentException($"{nameof(newCapacity)} must be a positive integer.");
		}

		_maxCapacity = newCapacity;
	}

	/// <summary>
	/// Sets a custom reset action to be invoked when an object is returned to the pool.
	/// </summary>
	/// <remarks>
	/// <br>Example Usage:</br>
	/// <example><br> </br>
	/// <br> <c>listPool.SetResetAction(list => list.Clear());</c> </br>
	/// </example> </remarks>
	/// <typeparam name="T">The type of object for which to set the reset action.</typeparam>
	/// <param name="resetAction">An action to reset the state of an object when it is retrieved from the pool.</param>
	public static void SetPoolResetAction<T>(Action<T> resetAction)
	{
		ArgumentNullException.ThrowIfNull(resetAction);

		if (_pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool)
		{
			objectPool.SetResetAction(resetAction);
		}
	}

	/// <summary>
	/// Clears the object pool associated with the specified type, if it exists. This operation removes all objects
	/// currently in the pool.
	/// </summary>
	/// <remarks>
	/// <br>Example Usage:</br>
	/// <example><br> </br>
	/// <br> <c>GenericFactoryPool.ClearPool&lt;List&lt;string&gt;&gt;();</c> </br>
	/// </example> </remarks>
	/// <typeparam name="T">The type for which the object pool should be cleared.</typeparam>
	public static void ClearPool<T>()
	{
		if (_pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool)
		{
			objectPool.Clear();
		}
	}

	/// <summary>
	/// Retrieves the count of objects currently in the object pool associated with the specified type, if it exists.
	/// </summary>
	/// <remarks>
	/// <br>Example Usage:</br>
	/// <example><br> </br>
	/// <br> <c>int count = GenericFactoryPool.GetPoolCount&lt;List&lt;string&gt;&gt;();</c> </br>
	/// </example> </remarks>
	/// <typeparam name="T">The type for which to retrieve the object pool count.</typeparam>
	/// <returns>The number of objects currently in the object pool, or 0 if the pool does not exist.</returns>
	public static int GetPoolCount<T>()
	{
		return _pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool ? objectPool.Count : 0;
	}

	/// <summary>
	/// Sets the size of the object pool associated with the specified type and specifies a factory method for creating new objects
	/// when the pool needs to be expanded. The size argument needs to be a positive integer no larger than the max capacity. 
	/// </summary>
	/// <remarks>
	/// <br>Example Usage:</br>
	/// <example><br> </br>
	/// <br> <c>GenericFactoryPool.SetPoolSize&lt;List&lt;string&gt;&gt;(10, listFactory);</c> </br>
	/// </example> </remarks>
	/// <typeparam name="T">The type for which to set the object pool size.</typeparam>
	/// <param name="size">The desired size of the object pool.</param>
	/// <param name="factoryMethod">A delegate that creates new instances of type T when needed.</param>
	public static void SetPoolSize<T>(int size, Func<T> factoryMethod)
	{
		ArgumentNullException.ThrowIfNull(factoryMethod);
		if (size > _maxCapacity && _maxCapacity > 0)
		{
			throw new ArgumentException($"{nameof(size)} cannot exceed the max capacity set by {nameof(_maxCapacity)} while a max capacity is set. ");
		}

		if (_pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool)
		{
			objectPool.SetSize(size, factoryMethod);
		}
	}

	/// <summary>
	/// Prepopulates the object pool associated with the specified type with a specified number of objects, using the provided factory method.
	/// </summary>
	/// <remarks>
	/// <br>Example Usage:</br>
	/// <example><br> </br>
	/// <br> <c>GenericFactoryPool.PrepopulatePool&lt;List&lt;string&gt;&gt;(5, listFactory);</c> </br>
	/// </example> </remarks>
	/// <typeparam name="T">The type for which to prepopulate the object pool.</typeparam>
	/// <param name="count">The number of objects to prepopulate the pool with.</param>
	/// <param name="factoryMethod">A delegate that creates new instances of type T for prepopulating the pool.</param>
	public static void PrepopulatePool<T>(int count, Func<T> factoryMethod)
	{
		ArgumentNullException.ThrowIfNull(factoryMethod);
		if (count > _maxCapacity && _maxCapacity > 0)
		{
			throw new ArgumentException($"{nameof(count)} cannot exceed the max capacity set by {nameof(_maxCapacity)} while a max capacity is set. ");
		}

		if (_pools.TryGetValue(typeof(T), out object pool) && pool is ObjectPool<T> objectPool)
		{
			objectPool.Prepopulate(count, factoryMethod);
		}
	}

	/// <summary>
	/// An Ordered object pool of the specified type <typeparamref name="T"/>, used by the Generic Factory Pool to store object types.
	/// Not for outside use. 
	/// </summary>
	/// <typeparam name="T">The type of object this pool holds.</typeparam>
	private sealed class ObjectPool<T>
	{
		/// <summary>
		/// The pool will hold the items ready to be used.
		/// if no items are available one will be made.
		/// all returned items will be added to the pool if the max size has not been reached.
		/// </summary>
		private readonly ConcurrentBag<T> _pool = new();

		/// <summary>
		/// The ResetAction allow for a custom reset action to be invoked when an item is returned. 
		/// </summary>
		private Action<T> _resetAction;

		/// <summary>
		/// Gets an object from the pool, either by reusing a recycled object or by creating a new one.
		/// </summary>
		/// <param name="factoryMethod">A function that creates and returns an object of type T.</param>
		/// <returns>An object of type T from the pool.</returns>
		internal T GetObject(Func<T> factoryMethod)
		{
			return _pool.TryTake(out T pooledItem) ? pooledItem : factoryMethod();
		}

		/// <summary>
		/// Returns an object back to the pool for potential reuse.
		/// If the object implements the `IRecyclable` interface, its state is reset before recycling.
		/// </summary>
		/// <param name="item">The object to return to the pool.</param>
		internal void ReturnObject(T item)
		{
			if (item is IRecyclable recyclable)
			{
				recyclable.ResetState();
			}

			_resetAction?.Invoke(item); // Invoke the reset action if set
			_pool.Add(item);
		}

		/// <summary>
		/// Clears all objects from the object pool by dequeuing them until the pool is empty.
		/// </summary>
		internal void Clear()
		{
			_pool.Clear();
			while (!_pool.IsEmpty && _pool.TryTake(out _)) { }
		}

		/// <summary>
		/// Gets the current count of objects in the object pool.
		/// </summary>
		internal int Count => _pool.Count;

		/// <summary>
		/// Sets the size of the object pool, adjusting it to the specified size while using the provided factory method
		/// to create new objects as needed or removing excess objects.
		/// </summary>
		/// <param name="size">The desired size of the object pool.</param>
		/// <param name="factoryMethod">A delegate that creates new instances of type T when needed.</param>
		internal void SetSize(int size, Func<T> factoryMethod)
		{
			while (_pool.Count > size && _pool.TryTake(out _)) { }

			while (_pool.Count < size)
			{
				_pool.Add(factoryMethod());
			}
		}

		/// <summary>
		/// Prepopulates the object pool with a specified number of objects, using the provided factory method.
		/// </summary>
		/// <param name="count">The number of objects to prepopulate the pool with.</param>
		/// <param name="factoryMethod">A delegate that creates new instances of type T for prepopulating the pool.</param>
		internal void Prepopulate(int count, Func<T> factoryMethod)
		{
			for (int i = 0; i < count; i++)
			{
				_pool.Add(factoryMethod());
			}
		}

		/// <summary>
		/// Sets a custom reset action to be invoked when an object is returned to the pool.
		/// </summary>
		/// <param name="resetAction">An action to reset the state of an object when it is retrieved from the pool.</param>
		internal void SetResetAction(Action<T> resetAction)
		{
			_resetAction = resetAction;
		}

	}
}