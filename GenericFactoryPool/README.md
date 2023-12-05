![Screw Apple Logo](https://github.com/MConlisk/GenericFactoryPool/blob/master/GenericFactoryPool/Assets/Logo.png?raw=true)
# GenericFactoryPool

## Introduction
The GenericFactoryPool is a C# class that provides a generic object pool for creating and recycling objects of various types. Object pooling helps improve performance by reusing objects instead of repeatedly creating and destroying them. The GenericFactoryPool has a maximum size that can be set to limit the number of objects in the pool.

## Key Concepts
- **Object Pooling:**
  Object pooling is a technique that involves reusing objects instead of creating and destroying them repeatedly. This helps improve performance by reducing the overhead of object creation and garbage collection.

- **Factory Pattern:**
  The factory pattern is a creational design pattern that provides an interface for creating objects without specifying their concrete classes. In the context of the GenericFactoryPool, a factory method is used to create objects of a specific type.

- **ConcurrentDictionary:**
  The ConcurrentDictionary class is a thread-safe collection that provides methods for adding, removing, and retrieving items. In the GenericFactoryPool, a ConcurrentDictionary is used to store and manage the object pools for different types.

- **ObjectPool:**
  The ObjectPool class is an internal class within the GenericFactoryPool that represents an ordered object pool for a specific type. It is responsible for storing and managing the objects in the pool.

## Code Structure
The GenericFactoryPool class is a static class that provides several methods for creating, recycling, and managing object pools. It also contains an internal ObjectPool class that represents the object pool for a specific type.

The main methods provided by the GenericFactoryPool class are:

- **Create\<T>(Func\<T> factoryMethod):**
  This method checks if an object pool exists for the specified type T. If an object pool exists, it retrieves a recycled object from the pool. Otherwise, it creates a new object pool and retrieves an object from it using the provided factory method.

- **Recycle\<T>(T item):**
  This method recycles an object of the specified type T back into the object pool if a pool exists. If the object implements the IRecyclable interface, its state is reset before recycling.

- **SetMaxCapacity(int newCapacity):**
  This method sets the maximum capacity of the object pool. The new capacity can be any positive integer, including 0, which indicates no capacity limit.

- **SetPoolResetAction\<T>(Action\<T> resetAction):**
  This method sets a custom reset action to be invoked when an object is returned to the pool. The reset action is an action that resets the state of an object when it is retrieved from the pool.

- **ClearPool\<T>():**
  This method clears the object pool associated with the specified type, removing all objects currently in the pool.

- **GetPoolCount\<T>():**
  This method retrieves the count of objects currently in the object pool associated with the specified type.

- **SetPoolSize\<T>(int size, Func\<T> factoryMethod):**
  This method sets the size of the object pool associated with the specified type and specifies a factory method for creating new objects when the pool needs to be expanded.

- **PrepopulatePool\<T>(int count, Func\<T> factoryMethod):**
  This method prepopulates the object pool associated with the specified type with a specified number of objects, using the provided factory method.

## Code Examples
Here are some code examples that demonstrate how to use the GenericFactoryPool:

csharp
// Set the maximum capacity of the object pool
GenericFactoryPool.SetMaxCapacity(10);

// Create a factory method for creating a list of strings
Func<List<string>> listFactory = () => new List<string>();

// Create an object pool for the list of strings
var listPool = GenericFactoryPool.Create(listFactory);

// Set a reset action for the list pool
listPool.SetResetAction(list => list.Clear());

// Set the size of the list pool
GenericFactoryPool.SetPoolSize<List<string>>(10, listFactory);

// Get an object from the list pool
var myList = listPool.GetObject(listFactory);

// Use the object
myList.Add("Item 1");
myList.Add("Item 2");

// Get the count of objects in the list pool
int count = GenericFactoryPool.GetPoolCount<List<string>>();

// Recycle the object back to the list pool
GenericFactoryPool.Recycle(myList);

// Clear the list pool
GenericFactoryPool.ClearPool<List<string>>();
