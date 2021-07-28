﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using Speckle.Core.Models;
using Speckle.Newtonsoft.Json;
using Speckle.Newtonsoft.Json.Linq;
using Speckle.Newtonsoft.Json.Serialization;

namespace Speckle.Core.Serialisation
{
  internal static class SerializationUtilities
  {
    #region Getting Types

    private static Dictionary<string, Type> cachedTypes = new Dictionary<string, Type>();

    internal static Type GetType(string objFullType)
    {
      if (cachedTypes.ContainsKey(objFullType))
      {
        return cachedTypes[objFullType];
      }
      var type = GetAtomicType(objFullType);
      cachedTypes[objFullType] = type;
      return type;

    }

    private static Dictionary<string, Type> cachedAtomicTypes = new Dictionary<string, Type>();

    internal static Type GetAtomicType(string objFullType)
    {
      if (cachedAtomicTypes.ContainsKey(objFullType))
      {
        return cachedAtomicTypes[objFullType];
      }
      var objectTypes = objFullType.Split(':').Reverse();
      foreach (var typeName in objectTypes)
      {
        //TODO: rather than getting the type from the first loaded kit that has it, maybe 
        //we get it from a specific Kit
        var type = KitManager.Types.FirstOrDefault(tp => tp.FullName == typeName);
        if (type != null)
        {
          cachedAtomicTypes[objFullType] = type;
          return type;
        }
      }

      return typeof(Base);
    }

    internal static Type GetSytemOrSpeckleType(string typeName)
    {
      var systemType = Type.GetType(typeName);
      if (systemType != null)
      {
        return systemType;
      }
      return GetAtomicType(typeName);
    }

    /// <summary>
    /// Flushes kit's (discriminator, type) cache. Useful if you're dynamically loading more kits at runtime, that provide better coverage of what you're deserialising, and it's now somehow poisoned because the higher level types were not originally available.
    /// </summary>
    public static void FlushCachedTypes()
    {
      cachedTypes = new Dictionary<string, Type>();
    }

    private static Dictionary<JsonProperty, bool> IsChunkableCache = new Dictionary<JsonProperty, bool>();

    #endregion

    #region Value handling

    internal static object HandleValue(JToken value, JsonSerializer serializer, CancellationToken CancellationToken, JsonProperty jsonProperty = null, string TypeDiscriminator = "speckle_type")
    {
      if (CancellationToken.IsCancellationRequested)
      {
        return null; // Check for cancellation
      }

      if (value is JValue jVal)
      {
        return jVal.Value;
      }

      if (jsonProperty != null)
      {
        bool isChunkable;
        if (IsChunkableCache.ContainsKey(jsonProperty))
        {
          isChunkable = IsChunkableCache[jsonProperty];
        }
        else
        {
          isChunkable = jsonProperty.AttributeProvider != null ? jsonProperty.AttributeProvider.GetAttributes(typeof(Chunkable), true).Count != 0 : false;
          IsChunkableCache[jsonProperty] = isChunkable;
        }

        if(value is JObject myObj)
        {
            if (myObj.Property(TypeDiscriminator) != null)
            {
              return value.ToObject(GetAtomicType(((JObject)value).GetValue(TypeDiscriminator).ToString()), serializer);
            }
            else
            {
              var dict = Activator.CreateInstance(jsonProperty.PropertyType) as IDictionary;
              foreach (var prop in myObj)
              {
                if (CancellationToken.IsCancellationRequested)
                {
                  return null; // Check for cancellation
                }
                dict[prop.Key] = HandleValue(prop.Value, serializer, CancellationToken);
              }
              return dict;
            }
        }

        if(value is JArray myArr)
        {

          var totalCount = myArr.Count;
          if(isChunkable)
          {
            totalCount = 0;
            foreach (JObject x in myArr)
            {
              totalCount += (x.GetValue("data") as JArray).Count;
            }
          }

          var listInnerTypeOrArrType = jsonProperty.PropertyType.GenericTypeArguments.Length != 0 ? jsonProperty.PropertyType.GenericTypeArguments[0] : jsonProperty.PropertyType.GetElementType();

          Array values = Array.CreateInstance(listInnerTypeOrArrType, totalCount);

          int i = 0;
          var innerTypeJp = new JsonProperty() { PropertyType = listInnerTypeOrArrType };

          foreach (var val in myArr)
          {
            var temp = HandleValue(val, serializer, CancellationToken, innerTypeJp);
            if (!isChunkable)
            {
              values.SetValue(temp, i++);
            } else
            {
              var dc = temp as DataChunk;
              foreach(var el in dc.data)
              {
                values.SetValue(el, i++);
              }
            }
          }

          if (jsonProperty.PropertyType.IsArray)
          {
            return values;
          }
          else
          {
            var list = Activator.CreateInstance(jsonProperty.PropertyType, new object[] { values });
            return list;
          }

        }

        return value.ToObject(jsonProperty.PropertyType);
      }
    

      if(value is JArray jArr)
      {
        var arr = new List<object>();
        foreach (var val in jArr)
        {
          if (CancellationToken.IsCancellationRequested)
          {
            return null; // Check for cancellation
          }

          if (val == null)
          {
            continue;
          }

          var item = HandleValue(val, serializer, CancellationToken);

          if (item is DataChunk chunk)
          {
            arr.AddRange(chunk.data);
          }
          else
          {
            arr.Add(item);
          }
        }
        return arr;
      }

      if(value is JObject jObj)
      {
        // If it's an object with type discriminator, it means it's a base class derived object, 
        // so we need to pass in the current serialiser in. 
        if (jObj.Property(TypeDiscriminator) != null)
        {
          return value.ToObject<Base>(serializer);
        }
        
        var dict = new Dictionary<string, object>();
        foreach (var prop in jObj)
        {
          if (CancellationToken.IsCancellationRequested)
          {
            return null; // Check for cancellation
          }
          dict[prop.Key] = HandleValue(prop.Value, serializer, CancellationToken);
        }

        return dict;
      }

      return null;
    }

    #endregion

    #region handle value old

    internal static object HandleValue2(JToken value, JsonSerializer serializer, CancellationToken CancellationToken, JsonProperty jsonProperty = null, string TypeDiscriminator = "speckle_type")
    {
      if (CancellationToken.IsCancellationRequested)
      {
        return null; // Check for cancellation
      }

      if (value is JValue)
      {
        if (jsonProperty != null)
        {
          return value.ToObject(jsonProperty.PropertyType);
        }
        else
        {
          return ((JValue)value).Value;
        }
      }

      // Lists
      if (value is JArray)
      {
        if (CancellationToken.IsCancellationRequested)
        {
          return null; // Check for cancellation
        }

        if (jsonProperty != null && jsonProperty.PropertyType.GetConstructor(Type.EmptyTypes) != null)
        {
          var arr = Activator.CreateInstance(jsonProperty.PropertyType);

          var addMethod = arr.GetType().GetMethod("Add");
          var hasGenericType = jsonProperty.PropertyType.GenericTypeArguments.Count() != 0;

          foreach (var val in ((JArray)value))
          {
            if (CancellationToken.IsCancellationRequested)
            {
              return null; // Check for cancellation
            }

            if (val == null)
            {
              continue;
            }

            var item = HandleValue2(val, serializer, CancellationToken);

            if (item is DataChunk chunk)
            {
              foreach (var dataItem in chunk.data)
              {
                if (hasGenericType && !jsonProperty.PropertyType.GenericTypeArguments[0].IsInterface)
                {
                  if (jsonProperty.PropertyType.GenericTypeArguments[0].IsAssignableFrom(dataItem.GetType()))
                  {
                    addMethod.Invoke(arr, new object[] { dataItem });
                  }
                  else
                  {
                    addMethod.Invoke(arr, new object[] { Convert.ChangeType(dataItem, jsonProperty.PropertyType.GenericTypeArguments[0]) });
                  }
                }
                else
                {
                  addMethod.Invoke(arr, new object[] { dataItem });
                }
              }
            }
            else if (hasGenericType && !jsonProperty.PropertyType.GenericTypeArguments[0].IsInterface)
            {
              if (jsonProperty.PropertyType.GenericTypeArguments[0].IsAssignableFrom(item.GetType()))
              {
                addMethod.Invoke(arr, new object[] { item });
              }
              else
              {
                addMethod.Invoke(arr, new object[] { Convert.ChangeType(item, jsonProperty.PropertyType.GenericTypeArguments[0]) });
              }
            }
            else
            {
              addMethod.Invoke(arr, new object[] { item });
            }
          }
          return arr;
        }
        else if (jsonProperty != null)
        {

          if (CancellationToken.IsCancellationRequested)
          {
            return null; // Check for cancellation
          }

          var arr = Activator.CreateInstance(typeof(List<>).MakeGenericType(jsonProperty.PropertyType.GetElementType()));

          foreach (var val in ((JArray)value))
          {
            if (CancellationToken.IsCancellationRequested)
            {
              return null; // Check for cancellation
            }

            if (val == null)
            {
              continue;
            }

            var item = HandleValue2(val, serializer, CancellationToken);
            if (item is DataChunk chunk)
            {
              foreach (var dataItem in chunk.data)
              {
                if (!jsonProperty.PropertyType.GetElementType().IsInterface)
                {
                  ((IList)arr).Add(Convert.ChangeType(dataItem, jsonProperty.PropertyType.GetElementType()));
                }
                else
                {
                  ((IList)arr).Add(dataItem);
                }
              }
            }
            else
            {
              if (!jsonProperty.PropertyType.GetElementType().IsInterface)
              {
                ((IList)arr).Add(Convert.ChangeType(item, jsonProperty.PropertyType.GetElementType()));
              }
              else
              {
                ((IList)arr).Add(item);
              }
            }
          }
          var actualArr = Array.CreateInstance(jsonProperty.PropertyType.GetElementType(), ((IList)arr).Count);
          ((IList)arr).CopyTo(actualArr, 0);
          return actualArr;
        }
        else
        {
          if (CancellationToken.IsCancellationRequested)
          {
            return null; // Check for cancellation
          }

          var arr = new List<object>();
          foreach (var val in ((JArray)value))
          {
            if (CancellationToken.IsCancellationRequested)
            {
              return null; // Check for cancellation
            }

            if (val == null)
            {
              continue;
            }

            var item = HandleValue2(val, serializer, CancellationToken);

            if (item is DataChunk chunk)
            {
              arr.AddRange(chunk.data);
            }
            else
            {
              arr.Add(item);
            }
          }
          return arr;
        }
      }

      if (CancellationToken.IsCancellationRequested)
      {
        return null; // Check for cancellation
      }

      if (value is JObject)
      {
        if (((JObject)value).Property(TypeDiscriminator) != null)
        {
          return value.ToObject<Base>(serializer);
        }

        var dict = jsonProperty != null ? Activator.CreateInstance(jsonProperty.PropertyType) : new Dictionary<string, object>();
        foreach (var prop in ((JObject)value))
        {
          if (CancellationToken.IsCancellationRequested)
          {
            return null; // Check for cancellation
          }

          object key = prop.Key;
          if (jsonProperty != null)
          {
            key = Convert.ChangeType(prop.Key, jsonProperty.PropertyType.GetGenericArguments()[0]);
          } ((IDictionary)dict)[key] = HandleValue2(prop.Value, serializer, CancellationToken);
        }
        return dict;
      }
      return null;
    }

    #endregion

    #region Abstract Handling

    private static Dictionary<string, Type> cachedAbstractTypes = new Dictionary<string, Type>();

    internal static object HandleAbstractOriginalValue(JToken jToken, string assemblyQualifiedName, JsonSerializer serializer)
    {
      if (cachedAbstractTypes.ContainsKey(assemblyQualifiedName))
      {
        return jToken.ToObject(cachedAbstractTypes[assemblyQualifiedName]);
      }

      var pieces = assemblyQualifiedName.Split(',').Select(s => s.Trim()).ToArray();

      var myAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(ass => ass.GetName().Name == pieces[1]);
      if (myAssembly == null)
      {
        throw new SpeckleException("Could not load abstract object's assembly.", level : Sentry.SentryLevel.Error);
      }

      var myType = myAssembly.GetType(pieces[0]);
      if (myType == null)
      {
        throw new SpeckleException("Could not load abstract object's assembly.", level : Sentry.SentryLevel.Error);
      }

      cachedAbstractTypes[assemblyQualifiedName] = myType;

      return jToken.ToObject(myType);
    }

    #endregion
  }

  internal static class CallSiteCache
  {
    // Adapted from the answer to 
    // https://stackoverflow.com/questions/12057516/c-sharp-dynamicobject-dynamic-properties
    // by jbtule, https://stackoverflow.com/users/637783/jbtule
    // And also
    // https://github.com/mgravell/fast-member/blob/master/FastMember/CallSiteCache.cs
    // by Marc Gravell, https://github.com/mgravell

    private static readonly Dictionary<string, CallSite<Func<CallSite, object, object, object>>> setters = new Dictionary<string, CallSite<Func<CallSite, object, object, object>>>();

    public static void SetValue(string propertyName, object target, object value)
    {
      CallSite<Func<CallSite, object, object, object>> site;

      lock(setters)
      {
        if (!setters.TryGetValue(propertyName, out site))
        {
          var binder = Microsoft.CSharp.RuntimeBinder.Binder.SetMember(CSharpBinderFlags.None,
            propertyName, typeof(CallSiteCache),
            new List<CSharpArgumentInfo>
            {
              CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
              CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
            });
          setters[propertyName] = site = CallSite<Func<CallSite, object, object, object>>.Create(binder);
        }
      }

      site.Target(site, target, value);
    }
  }
}
