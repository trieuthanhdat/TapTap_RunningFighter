using System.Collections.Generic;
using UnityEngine;


namespace TD.Utilities
{
    public static class ComponentCache
    {
        private static readonly Dictionary<GameObject, Dictionary<System.Type, Component>> cache = new Dictionary<GameObject, Dictionary<System.Type, Component>>();

        public static T Get<T>(GameObject gameObject, bool clearEmpty = false) where T : Component
        {
            if (gameObject == null) return null;

            if (cache.TryGetValue(gameObject, out var typeDictionary) &&
                typeDictionary.TryGetValue(typeof(T), out var cachedComponent) &&
                cachedComponent != null)
            {
                return (T)cachedComponent;
            }

            if (clearEmpty)
            {
                // Remove the GameObject from the cache if it exists and the dictionary is empty
                if (cache.TryGetValue(gameObject, out typeDictionary) &&
                    typeDictionary.Count == 0)
                {
                    cache.Remove(gameObject);
                }
            }

            T component = gameObject.GetComponent<T>();
            if (component != null)
            {
                AddToCache(gameObject, component);
            }

            return component;
        }

        private static void AddToCache<T>(GameObject gameObject, T component) where T : Component
        {
            if (!cache.TryGetValue(gameObject, out var typeDictionary))
            {
                typeDictionary = new Dictionary<System.Type, Component>();
                cache[gameObject] = typeDictionary;
            }

            typeDictionary[typeof(T)] = component;
        }

        public static T LazyGet<T>(this Component comp, ref T backingField) where T : Component
        {
            if (backingField == null || backingField.Equals(null))
            {
                backingField = comp.GetComponent<T>();
            }
            return backingField;
        }

        public static T LazyGetChild<T>(this Component comp, ref T backingField) where T : Component
        {
            if (backingField == null || backingField.Equals(null))
            {
                backingField = comp.GetComponentInChildren<T>();
            }
            return backingField;
        }

        public static T LazyGetParent<T>(this Component comp, ref T backingField) where T : Component
        {
            if (backingField == null || backingField.Equals(null))
            {
                backingField = comp.GetComponentInParent<T>();
            }
            return backingField;
        }

        public static T LazyNew<T>(this Component _, ref T backingField) where T : new()
        {
            if (backingField == null || backingField.Equals(null))
            {
                backingField = new T();
            }
            return backingField;
        }
        // Method to add a component to the cache
        private static void AddToCache(GameObject gameObject, Component component)
        {
            // If the cache does not contain the GameObject, add it to the cache
            if (!cache.ContainsKey(gameObject))
            {
                cache.Add(gameObject, new Dictionary<System.Type, Component>());
            }

            // If the cache does not contain the component type for the GameObject, add it to the cache
            if (!cache[gameObject].ContainsKey(component.GetType()))
            {
                cache[gameObject].Add(component.GetType(), component);
            }
        }


    }

}
