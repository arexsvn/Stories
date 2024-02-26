using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/**
 * CacheMap
 *
 * A Caching system for sharing data between services.  Supports caching 'single' objects or objects by a key. Looks up an object by
 *   class Type or class Type + key.
 *
 * USAGE :
 *
 * Working with multiple instances of the same type
 *   Cache instances by key. If instance implements ICacheable and it already exists in the cache 'Update' will be called on it instead of simply replacing the cached instance.
 *     _cacheMap.Cache(new ClubData("clubId1", "Cool Club"), "clubId1");
 *     _cacheMap.Cache(new ClubData("clubId2", "Fake Club"), "clubId2");
 *
 *   Get instance by key
 *     ClubData coolClub = _cacheMap.Get<ClubData>("clubId1");
 *
 * 
 * Working with single instances
 *   Cache single instance. If instance implements ICacheable and it already exists in the cache 'Update' will be called on it instead of simply replacing the cached instance.
 *     _cacheMap.Cache(new User("someId", "Joe"));
 *
 *   Get single instance
 *     User userJoe = _cacheMap.Get<User>();
 *
 *
 * Register a listener for a cache update of a specific datatype :
 *    Listener will fire anytime this type is updated in the cache.
 *      _cacheMap.Register<SomeData>(cacheUpdated);
 *
 *    Unregister listeners when cleaning up.
 *      _cacheMap.Unregister<SomeData>(cacheUpdated);
 *
 * 
 */
public class CacheMap : ICache
{
    private IDictionary<Type, IDictionary> _cache;
    private IDictionary<Type, object> _singleInstanceCache;
    private IDictionary<Type, ConcurrentDictionary<object, Action<object>>> _updateListeners;
    private Dictionary<string, string> _fullyQualifiedClassNames;
    public CacheMap()
    {
        Init();
    }

    private void Init()
    {
        _cache = new ConcurrentDictionary<Type, IDictionary>();
        _singleInstanceCache = new ConcurrentDictionary<Type, object>();
        _updateListeners = new ConcurrentDictionary<Type, ConcurrentDictionary<object, Action<object>>>();
        _fullyQualifiedClassNames = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// Cache an object instance by its Type. Used if we need to cache a single instance of a Type.
    /// <param name="instance">Instance to be cached.</param>
    /// </summary>
    public void Cache<T>(T instance)
    {
        if (instance == null)
        {
            Debug.LogError($"CacheMap :: Cache : Cannot cache null instance.");
            return;
        }

        Type type = instance.GetType();

        // If an instance implements ICacheable, let it handle to update itself, otherwise just replace.
        if (_singleInstanceCache.ContainsKey(type) && _singleInstanceCache[type] is ICacheable cacheable)
        {
            cacheable.Update(instance);
        }
        else
        {
            _singleInstanceCache[type] = instance;
        }

        cacheUpdated(instance);
    }
    
    /// <summary>
    /// Add an object instance to a collection of objects of a matching Type by key. It will overwrite
    ///   a object matching the key if it already exists.
    /// <param name="instance">Instance to be cached.</param>
    /// <param name="key">key used to lookup the instance in the collection (ex : id of the object.)</param>
    /// </summary>
    public void Cache<T>(T instance, string key)
    {
        if (instance == null || string.IsNullOrEmpty(key))
        {
            Debug.LogError($"CacheMap :: Cache : Cannot cache instance with key {key}.");
            return;
        }
        
        Type type = instance.GetType();
        IDictionary collection = GetCollection(type);
        
        // If an instance implements ICacheable, let it handle to update itself, otherwise just replace.
        if (collection.Contains(key) && collection[key] is ICacheable cacheable)
        {
            cacheable.Update(instance);
        }
        else
        {
            collection[key] = instance;
        }

        cacheUpdated(instance);
    }

    /// <summary>
    /// Cache all instances by string key to instance.
    /// <param name="instances">Instances to be cached.</param>
    /// </summary>
    public void CacheAll(IDictionary instances)
    {
        foreach (object key in instances.Keys)
        {
            string keyString;
   
            if (!(key is string))
            {
                keyString = key.ToString();
            }
            else
            {
                keyString = (string)key;
            }
            Cache(instances[key], keyString);
        }
    }

    /// <summary>
    /// Gets the single cached instance of a Type.
    /// </summary>
    public T Get<T>()
    {
        if (!_singleInstanceCache.ContainsKey(typeof(T)))
        {
            return default;
        }
            
        return (T)_singleInstanceCache[typeof(T)];
    }
        
    /// <summary>
    /// Get an instance of a Type by a key. If one doesn't exist it will return default for the Type.
    /// <param name="key">key used to lookup the instance in the collection (ex : id of the object.)</param>
    /// </summary>
    public T Get<T>(string key)
    {
        IDictionary<string, T> collection = GetCollection<T>();

        return collection.TryGetValue(key, out var instance) ? instance : default;
    }
    
    /// <summary>
    /// Get the full collection of instances of a Type from the cache. A new collection will be created if it doesn't exist for a Type.
    /// </summary>
    public IDictionary<string, T> GetCollection<T>()
    {
        if (!_cache.ContainsKey(typeof(T)))
        {
            _cache[typeof(T)] = new ConcurrentDictionary<string, T>();
        }
            
        return (IDictionary<string, T>)_cache[typeof(T)];
    }

    public IDictionary GetCollection(Type type)
    {
        if (!_cache.ContainsKey(type))
        {
            // Use Activator to create a new dictionary dynamically to preserve type information.
            Type dictType = typeof(ConcurrentDictionary<, >).MakeGenericType(typeof(string), type);
            _cache[type] = (IDictionary)Activator.CreateInstance(dictType);
        }
            
        return _cache[type];
    }
        
    /// <summary>
    /// Clear specific key from cache.
    /// </summary>
    public void Clear<T>(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            if (_cache.ContainsKey(typeof(T)) && _cache[typeof(T)].Contains(key))
            {
                _cache[typeof(T)].Remove(key);
            }
        }
    }
        
    /// <summary>
    /// Clear cache of Type.
    /// </summary>
    public void Clear<T>()
    {
        if (_cache.ContainsKey(typeof(T)))
        {
            _cache[typeof(T)].Clear();
        }
            
        if (_singleInstanceCache.ContainsKey(typeof(T)))
        {
            _singleInstanceCache.Remove(typeof(T));
        }
    }
        
    /// <summary>
    /// Store all instances on an object by Type.
    /// <param name="container">Container object with instances to cache stored in Properties.</param>
    /// <param name="exclusions">Any Types to exclude from caching.</param>
    /// </summary>
    public void CacheContainer<T>(T container, List<Type> exclusions = null)
    {
        FieldInfo[] fields = container.GetType().GetFields();
            
        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(container);
            if (value == null || value.GetType().IsPrimitive || value is string || field.DeclaringType != container.GetType())
            {
                continue;
            }
                
            if (exclusions != null)
            {
                Type type = value.GetType();
                if (exclusions.Contains(type) || type.IsGenericType && exclusions.Contains(type.BaseType))
                {
                    continue;
                }
            }
                
            if (value is IDictionary collection)
            {
                CacheAll(collection);
            }
            else
            {
                Cache(value);
            }
        }
    }
    
    /// <summary>
    /// Register a listener to get notified when a Type of cached data is updated.
    /// <param name="listener">A method that takes a parameter of an instance of the Type it is listening for.</param>
    /// </summary>
    public void Register<T>(Action<T> listener)
    {
        Type type = typeof(T);
        
        if (!_updateListeners.ContainsKey(type))
        {
            _updateListeners[type] = new ConcurrentDictionary<object, Action<object>>();
        }

        void OnUpdateListener(object instance)
        {
            listener?.Invoke((T)instance);
        }

        _updateListeners[type][listener] = OnUpdateListener;

        AddClass<T>();
    }

    public void Deregister<T>(Action<T> listener)
    {
        if (_updateListeners.TryGetValue(typeof(T), out var listeners))
        {
            listeners.TryRemove(listener, out _);
        }
    }

#if ENABLE_JSON_CACHING
    /// <summary>
    /// Convert instances from a map of Type names to json maps and store in cache. Any Types that need to be processed should first be added to the class name map by
    ///   calling 'AddClass<Class>();' or Registering a listener for the Type which will automatically add it to the list.
    /// <param name="cache">A map of Type name to a map of ids to json strings.</param>
    /// </summary>
    public void CacheFromJson(Dictionary<string, Dictionary<string, string>> cache)
    {
        foreach (var kvp in cache)
        {
            string className = kvp.Key;

            if (!_fullyQualifiedClassNames.ContainsKey(className))
            {
                Debug.LogError($"CacheMap :: CacheFromJson : Class Name '{className}' not found. Register a listener for this type or add it using 'AddClass<{className}>();'");
                continue;
            }
            
            Type type = Type.GetType(_fullyQualifiedClassNames[className]);
            if (type == null)
            {
                Debug.LogError($"CacheMap :: CacheFromJson : Type is null for class name '{className}'. Ensure class '{className}' exists on both the client and server.");
                continue;
            }
            
            foreach (var kvp2 in kvp.Value)
            {
                Newtonsoft.Json.JsonSerializerSettings set = new Newtonsoft.Json.JsonSerializerSettings
                {
                    TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                };
                var instance = Newtonsoft.Json.JsonConvert.DeserializeObject(kvp2.Value, type, set);
                
                Cache(instance, kvp2.Key);
            }
        }
    }
    
    /// <summary>
    /// Convert instances from a map of Type names to json and store in cache. Any Types that need to be processed should first be added to the class name map by
    ///   calling 'AddClass<Class>();' or Registering a listener for the Type which will automatically add it to the list.
    /// <param name="cache">A map of Type name to json strings.</param>
    /// </summary>
    public void CacheFromJson(Dictionary<string, string> cache)
    {
        foreach (var kvp in cache)
        {
            string className = kvp.Key;
            if (!_fullyQualifiedClassNames.ContainsKey(className))
            {
                Debug.LogError($"CacheMap :: CacheFromJson : Class Name '{className}' not found. Register a listener for this type or add it using 'AddClass<{className}>();'");
                return;
            }
            
            Type type = Type.GetType(_fullyQualifiedClassNames[className]);
            if (type == null)
            {
                Debug.LogError($"CacheMap :: CacheFromJson : Type is null for class name '{className}'. Ensure class '{className}' exists on both the client and server.");
                return;
            }
            
            Newtonsoft.Json.JsonSerializerSettings set = new Newtonsoft.Json.JsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            };
            var instance = Newtonsoft.Json.JsonConvert.DeserializeObject(kvp.Value, type, set);
                
            Cache(instance);
        }
    }
#endif

    /// <summary>
    /// Add a class of type 'T' to the map of simple class name to fully qualified class name so instances of that Type can be deserialized from json.
    /// </summary>
    public void AddClass<T>()
    {
        string fullName = typeof(T).AssemblyQualifiedName;
        _fullyQualifiedClassNames[typeof(T).Name] = fullName;
    }

    private void cacheUpdated<T>(T instance)
    {
        if (_updateListeners.TryGetValue(instance.GetType(), out var listeners))
        {
            foreach (var kvp in listeners)
            {
                kvp.Value.Invoke(instance);
            }
        }
    }
    
    private void CopyFieldValues<T>(T target, T source)
    {
        Type t = typeof(T);
        FieldInfo[] fields = t.GetFields();//.Where(prop => prop.IsPublic);

        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(source);
            if (value != null)
            {
                field.SetValue(target, value);
            }
        }
    }
        
    private void CopyPropertyValues<T>(T target, T source)
    {
        Type t = typeof(T);
        IEnumerable<PropertyInfo> properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

        foreach (PropertyInfo property in properties)
        {
            object value = property.GetValue(source, null);
            if (value != null)
            {
                property.SetValue(target, value, null);
            }
        }
    }
    
    private static CacheMap _instance;

    public static CacheMap Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CacheMap();
            }

            return _instance;
        }
    }
}
    
public interface ICache : ICacheMap, IInstanceCache
{
    public void CacheContainer<T>(T container, List<Type> exclusions = null);
}
    
public interface ICacheMap
{
    public void Cache<T>(T instance, string key);
    public T Get<T>(string key);
    public IDictionary<string, T> GetCollection<T>();
    public void CacheAll(IDictionary instances);
    public void Clear<T>(string key);
    public void Register<T>(Action<T> listener);
    public void Deregister<T>(Action<T> listener);
}
    
public interface IInstanceCache
{
    public void Cache<T>(T instance);
    public T Get<T>();
    public void Clear<T>();
    public void Register<T>(Action<T> listener);
    public void Deregister<T>(Action<T> listener);
}

public interface ICacheable
{
    public void Update<T>(T instance);
}