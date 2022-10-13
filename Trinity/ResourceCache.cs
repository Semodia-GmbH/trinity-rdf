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
using System.Collections;

namespace Semiodesk.Trinity
{
    class ResourceCache
    {
        #region Members

        public IModel Model;

        protected Dictionary<IPropertyMapping, HashSet<Uri>> Cache = new Dictionary<IPropertyMapping, HashSet<Uri>>();

        #endregion

        #region Constructors

        public ResourceCache() {}

        #endregion

        #region Methods

        public void Clear()
        {
            Cache.Clear();
        }

        public void CacheValues(IPropertyMapping mapping, IEnumerable<Uri> values)
        {
            if (!Cache.ContainsKey(mapping))
            {
                Cache[mapping] = new HashSet<Uri>(values);
            }
            else
            {
                var cache = Cache[mapping];

                foreach (var value in values)
                {
                    cache.Add(value);
                }
            }
        }

        public void CacheValue(IPropertyMapping mapping, Uri value)
        {
            if (!Cache.ContainsKey(mapping))
            {
                Cache[mapping] = new HashSet<Uri>() { value };
            }
            else
            {
                var cache = Cache[mapping];

                cache.Add(value);
            }
        }

        /// <summary>
        /// This method loads the cached Resources for the given MappingProperty from the Storage and returns them.
        /// They are instantiated as the defined type. The cache for this mapping property is emptied.
        /// </summary>
        /// <param name="mapping">Mapping property which should be loaded from cache.</param>
        /// <returns>List of formerly cached resources.</returns>
        public void LoadCachedValues(IPropertyMapping mapping)
        {
            if (!Cache.ContainsKey(mapping))
            {
                return;
            }

            var baseType = (mapping.IsList) ? mapping.GenericType : mapping.DataType;

            var cachedUris = Cache[mapping];

            if (!mapping.IsList && cachedUris.Count > 1)
            {
                throw new Exception(
                    $"An error occured while loading the cached resources for property {mapping.PropertyName}. Found {cachedUris.Count} elements but it is mapped to a non-list property. Try to map to a list of objects.");
            }

            var res = Model.GetResources(cachedUris, baseType);

            foreach (IResource resource in res)
            {
                cachedUris.Remove(resource.Uri);
                AddToMapping(mapping, resource);
            }

            foreach( var uri in cachedUris)
            {
                var resource = Activator.CreateInstance(baseType, uri) as IResource;
                AddToMapping(mapping, resource);
            }

            Cache.Remove(mapping);
        }

        private void AddToMapping(IPropertyMapping mapping, IResource resource)
        {
            if (mapping.IsList)
            {
                // Getting the reference to the mapped list object
                var list = mapping.GetValueObject() as IList;

                if (list != null)
                {
                    // Make sure the resource exits only one time
                    if (list.Contains(resource))
                        list.Remove(resource);

                    // Add their resource to the mapped list
                    list.Add(resource);
                }
            }
            else
            {
                mapping.SetOrAddMappedValue(resource);
            }
        }

        /// <summary>
        /// Tests if the mapping has cached values.
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public bool HasCachedValues(IPropertyMapping mapping)
        {
            return Cache.ContainsKey(mapping);
        }

        /// <summary>
        /// Tests if the mapping has a certain cached values.
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public bool HasCachedValues(IPropertyMapping mapping, Uri uri)
        {
            return Cache.ContainsKey(mapping) ? Cache[mapping].Contains(uri) : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public bool HasCachedValue(Uri uri)
        {
            return Cache.Values.Any(set => set.Contains(uri));
        }

        public IEnumerable<Uri> ListCachedValues(IPropertyMapping mapping)
        {
            return Cache[mapping];
        }

        #endregion
    }
}
