﻿// LICENSE:
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// AUTHORS:
//
//  Moritz Eberl <moritz@semiodesk.com>
//  Sebastian Faubel <sebastian@semiodesk.com>
//
// Copyright (c) Semiodesk GmbH 2015-2019

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Semiodesk.Trinity
{
    /// <summary>
    /// This static class is responsible for discovering mapped classes.
    /// Every assembly that defines mapping classes needs to register them with this service.
    /// </summary>
    public static class MappingDiscovery
    {
        #region Types

        /// <summary>
        /// A class containing information about a RDF class mapped to c#.
        /// </summary>
        public class MappingClass
        {
            #region Members

            /// <summary>
            /// The .NET type of the class.
            /// </summary>
            public readonly Type MappingClassType;

            /// <summary>
            /// RDF classes that are mapped to this class.
            /// </summary>
            public readonly Class[] RdfClasses;

            /// <summary>
            /// Inferenced RDF classes mapped to this class. Currently not used.
            /// </summary>
            public readonly Class[] RdfBaseClasses;

            /// <summary>
            /// The number of classes that are not sub class of any other class.
            /// </summary>
            public readonly uint BaseClassCount;

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor to create a new MappingClass
            /// </summary>
            /// <param name="mappingClassType">The c# type</param>
            /// <param name="rdfClasses">The mapped rdf classes.</param>
            /// <param name="rdfBaseClasses">The rdf base classes.</param>
            public MappingClass(Type mappingClassType, IEnumerable<Class> rdfClasses, IEnumerable<Class> rdfBaseClasses )
            {
                MappingClassType = mappingClassType;
                RdfClasses = rdfClasses.ToArray();
                RdfBaseClasses = rdfBaseClasses.ToArray();
                BaseClassCount = 0;

                var t = mappingClassType;

                while( t.BaseType != typeof(object))
                {
                    BaseClassCount++;
                    t = t.BaseType;
                }
            }

            #endregion
        }

        #endregion

        #region Members

        /// <summary>
        /// The list of all registered assemblies.
        /// </summary>
        public static List<string> RegisteredAssemblies = new List<string>();

        /// <summary>
        /// The list of all registered mapped classes.
        /// </summary>
        public static List<MappingClass> MappingClasses = new List<MappingClass>();

        #endregion

        #region Constructors

        static MappingDiscovery()
        {
            AddMappingClasses(new List<Type> { typeof(Resource) });
            RegisteredAssemblies.Add(Assembly.GetExecutingAssembly().GetName().FullName);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a collection of mapped classes to the registration.
        /// </summary>
        /// <param name="list"></param>
        public static void AddMappingClasses(IList<Type> list)
        {
            foreach (var o in list)
            {
                AddMappingClass(o);
            }
        }

        /// <summary>
        /// Adds a mapped class to the registration.
        /// </summary>
        /// <param name="_class"></param>
        public static void AddMappingClass(Type _class)
        {
            try
            {
                if (_class.IsAbstract)
                    return; 

                var r = (Resource)Activator.CreateInstance(_class, new UriRef("semio:empty"));

                var baseTypes = new List<Class>(r.GetTypes());

                GetBaseTypes(_class, ref baseTypes);

                var c = new MappingClass(_class, r.GetTypes(), baseTypes);

                if (MappingClasses.Contains(c)) return;

                MappingClasses.Add(c);
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Initialisation of mapping class {_class.ToString()} failed. For the reason please consult the inner exception.", e);
            }
        }

        internal static IEnumerable<IPropertyMapping> ListMappings(Type _class)
        {
            var propertyMappingType = typeof(IPropertyMapping);
            
            Resource resource;
            
            try
            {
                resource = (Resource)Activator.CreateInstance(_class, new UriRef("semio:empty"));
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Initialisation of mapping class {_class.ToString()} failed. For the reason please consult the inner exception.", e);
            }
            
            foreach (var x in _class.GetFields())
            {
                if (propertyMappingType.IsAssignableFrom(x.FieldType))
                {
                    yield return x.GetValue(resource) as IPropertyMapping;
                }
            }
        }

        /// <summary>
        /// Add the super classes of a given .NET type to a given list.
        /// </summary>
        /// <param name="type">A .NET type.</param>
        /// <param name="baseTypes">List where the base types will be added to.</param>
        public static void GetBaseTypes(Type type, ref List<Class> baseTypes)
        {
            if (type.BaseType == typeof(Resource) || type.BaseType == typeof(Object))
            {
                return;
            }

            try
            { 
                var r = (Resource)Activator.CreateInstance(type.BaseType, new UriRef("urn:"));

                baseTypes.AddRange(r.GetTypes());

                GetBaseTypes(type.BaseType, ref baseTypes);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Loads all mapped classes from the assembly calling this method.
        /// </summary>
        public static void RegisterCallingAssembly()
        {
            var a = Assembly.GetCallingAssembly();

            if (!RegisteredAssemblies.Contains(a.GetName().FullName))
            {
                RegisterAssembly(a);
            }
        }

        /// <summary>
        /// Register ALL THE THINGS!!
        /// from all assemblies currently loaded.
        /// </summary>
        public static void RegisterAllCurrentAssemblies()
        {
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!RegisteredAssemblies.Contains(a.GetName().FullName))
                {
                    RegisterAssembly(a);
                }
            }
        }

        /// <summary>
        /// Load all mapped classes from the given assembly.
        /// </summary>
        /// <param name="asm"></param>
        public static void RegisterAssembly(Assembly asm)
        {
            var name = asm.GetName().FullName;
            if (RegisteredAssemblies.Contains(name))
                return;
            RegisteredAssemblies.Add(name);

            var l = GetMappingClasses(asm);

            AddMappingClasses(l);
        }

        private static IList<Type> GetMappingClasses(Assembly asm)
        {
            try
            {
                return (from t in asm.GetTypes() where typeof(Resource).IsAssignableFrom(t) select t).ToList();
            }
            catch
            {
                return new List<Type>();
            }
        }

        /// <summary>
        /// Returns all types which match the given restrictions.
        /// </summary>
        /// <param name="classes">List of RDF classes</param>
        /// <param name="type">A c# type in a inheritence tree. Give Resource if you don't know what to do.</param>
        /// <param name="inferencingEnabled">Should inferencing be factored in.</param>
        public static Type[] GetMatchingTypes(IEnumerable<Class> classes, Type type, bool inferencingEnabled = false)
        {
            if (!inferencingEnabled)
            { 
                return (from t in MappingClasses
                                     where t.RdfClasses.Intersect(classes).Count() == t.RdfClasses.Length && type.IsAssignableFrom(t.MappingClassType)
                                     orderby t.BaseClassCount descending
                        select t.MappingClassType).ToArray();
            }
            else
            { 
                return (from t in MappingClasses
                        where t.RdfBaseClasses.Intersect(classes).Count() == t.RdfBaseClasses.Length && type.IsAssignableFrom(t.MappingClassType)
                        orderby t.RdfBaseClasses.Intersect(classes).Count() descending
                        select t.MappingClassType).ToArray();
            }
        }

        /// <summary>
        /// The the RDF class of a C# type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Class> GetRdfClasses(Type type)
        {
            return (from t in MappingClasses where t.MappingClassType == type select t.RdfClasses).First();
        }

        #endregion
    }
}
