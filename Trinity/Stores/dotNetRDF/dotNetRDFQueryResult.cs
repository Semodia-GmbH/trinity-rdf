// LICENSE:
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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Query;

namespace Semiodesk.Trinity.Store
{
    public class dotNetRDFQueryResult : ISparqlQueryResult
    {
        #region Members

        private IModel _model;

        private ITripleProvider _tripleProvider;

        private ISparqlQuery _query;

        private SparqlResultSet _queryResults;

        private StoreBase _store;

        #endregion

        #region Constructor

        public dotNetRDFQueryResult(StoreBase store, ISparqlQuery query, SparqlResultSet resultSet)
        {
            string s = null;
            string p = null;
            string o = null;

            if(query.ProvidesStatements())
            {
                // A list of global scope variables without the ?. Used to access the
                // subject, predicate and object variable in statement providing queries.
                var vars = query.GetGlobalScopeVariableNames();

                s = vars[0];
                p = vars[1];
                o = vars[2];
            }

            _query = query;
            _tripleProvider = new SparqlResultSetTripleProvider(resultSet, s, p, o);
            _model = query.Model;
            _queryResults = resultSet;
            _store = store;
        }

        public dotNetRDFQueryResult(StoreBase store, ISparqlQuery query, IGraph graph)
        {
            _query = query;
            _tripleProvider = new GraphTripleProvider(graph);
            _model = query.Model;
            _store = store;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public bool GetAnswer()
        {
            if (_query.QueryType == SparqlQueryType.Ask)
            {
                return _queryResults.Result;
            }
            else
            {
                throw new Exception();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<BindingSet> GetBindings()
        {
            if (_query.QueryType == SparqlQueryType.Select)
            {
                foreach (var result in _queryResults)
                {
                    var b = new BindingSet();

                    foreach (var r in result)
                    {
                        if (r.Value != null)
                        {
                            b.Add(r.Key, ParseCellValue(r.Value));
                        }
                    }

                    yield return b;
                }
            }
            else
            {
                throw new ArgumentException("Cannot return bindings for queries of type " + _query.QueryType.ToString());
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Resource> GetResources()
        {
            return GetResources<Resource>();
        }

        /// <inheritdoc/>
        public IEnumerable<Resource> GetResources(int offset = -1, int limit = -1)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<Resource> GetResources(Type type)
        {
            if (_query.ProvidesStatements())
            {
                return GenerateResources(type);
            }
            else
            {
                throw new ArgumentException("The given query cannot be resolved into statements.");
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetResources<T>() where T : Resource
        {
            if(_query.ProvidesStatements())
            {
                return GenerateResources(typeof(T)).Where(x => typeof(T).IsAssignableFrom(x.GetType())).Select(x => x as T);
            }
            else
            {
                throw new ArgumentException("The given query cannot be resolved into statements.");
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetResources<T>(int offset = -1, int limit = -1) where T : Resource
        {
            throw new NotImplementedException();
        }


        private IEnumerable<Resource> GenerateResources(Type type)
        {
            var result = new List<Resource>();

            if (0 < _tripleProvider.Count)
            {
                // A dictionary mapping URIs to the generated resource objects.
                var cache = new Dictionary<string, Resource>();
                var types = FindResourceTypes(_query.IsInferenceEnabled, type);

                _tripleProvider.Reset();

                foreach (var resourceType in types)
                {
                    if (resourceType.Value is Resource res)
                    {
                        cache.Add(resourceType.Key, res);
                    }
                }

                // A handle to the currently built resource which may spare the lookup in the dictionary.
                Resource currentResource = null;

                while (_tripleProvider.HasNext)
                {
                    var s = _tripleProvider.S;
                    var p = OntologyDiscovery.GetProperty(_tripleProvider.P);
                    var o = _tripleProvider.O;

                    _tripleProvider.SetNext();

                    if (s is IUriNode || s is BlankNode)
                    {
                        Uri subjectUri = null;

                        if (s is IUriNode uriNode)
                        {
                            subjectUri = uriNode.Uri;
                        }
                        else if (s is BlankNode blankNode)
                        {
                            subjectUri = new UriRef(blankNode.ToString(), true);
                        }

                        if (currentResource != null && currentResource.Uri.OriginalString == subjectUri.OriginalString)
                        {
                            // We already have the handle to the resource which the property should be added to.
                        }
                        else if (cache.ContainsKey(subjectUri.OriginalString))
                        {
                            currentResource = cache[subjectUri.OriginalString];

                            // In this case we may have encountered a resource which was 
                            // added to the cache by the object value handler below.
                            if (!result.Contains(currentResource))
                            {
                                result.Add(currentResource);
                            }
                        }
                        else
                        {
                            try
                            {
                                currentResource = (Resource)Activator.CreateInstance(type, subjectUri);
                                currentResource.IsNew = false;
                                currentResource.IsSynchronized = true;
                                currentResource.Model = _model;

                                cache.Add(subjectUri.OriginalString, currentResource);

                                result.Add(currentResource);
                            }
                            catch
                            {
#if DEBUG
                                Debug.WriteLine("[SparqlQueryResult] Error: Could not create resource:", subjectUri.OriginalString);
#endif

                                continue;
                            }
                        }
                    }
                   

                    if (o is IUriNode || o is BlankNode)
                    {
                        Uri uri = null;

                        if (o is IUriNode uriNode)
                        {
                            uri = uriNode.Uri;
                        }
                        else if (o is BlankNode blankNode)
                        {
                            uri = new UriRef("_:" + blankNode.InternalID, true);
                        }

                        if (currentResource.HasPropertyMapping(p, uri.GetType()))
                        {
                            currentResource.AddPropertyToMapping(p, uri, false);
                        }
                        else if (cache.ContainsKey(uri.OriginalString))
                        {
                            currentResource.AddPropertyToMapping(p, cache[uri.OriginalString], true);
                            currentResource.IsNew = false;
                            currentResource.IsSynchronized = false;
                            currentResource.Model = _model;
                        }
                        else
                        {
                            var r = new Resource(uri);
                            r.IsNew = false;

                            cache.Add(uri.OriginalString, r);
                            currentResource.AddPropertyToMapping(p, r, true);
                            currentResource.IsNew = false;
                            currentResource.IsSynchronized = false;
                            currentResource.Model = _model;
                        }
                    }
                    else
                    {
                        currentResource.AddPropertyToMapping(p, ParseCellValue(o), true);
                    }
                }
            }

            foreach (var r in result)
            {
                yield return r;
            }
        }

        private object ParseCellValue(INode p)
        {
            if (p.NodeType == NodeType.Uri)
            {
                var uriNode = p as IUriNode;

                return uriNode.Uri;
            }
            else if (p.NodeType == NodeType.Literal)
            {
                var literalNode = p as ILiteralNode;

                if (literalNode.DataType == null)
                {
                    if (string.IsNullOrEmpty(literalNode.Language))
                    {
                        return literalNode.Value;
                    }
                    else
                    {
                        return new Tuple<string, CultureInfo>(literalNode.Value, CultureInfo.GetCultureInfoByIetfLanguageTag(literalNode.Language));
                    }
                }

                return XsdTypeMapper.DeserializeString(literalNode.Value, literalNode.DataType);
            }
            else if(p.NodeType == NodeType.Blank)
            {
                var blankNode = p as IBlankNode;

                return new UriRef(blankNode.ToString(), true);
            }

            return null;
        }

        private Dictionary<string, object> FindResourceTypes(bool inferencingEnabled, Type type) 
        {
            var result = new Dictionary<string, object>();
            var types = new Dictionary<string, List<Class>>();

            INode s;
            string p;
            INode o;

            //_tripleProvider.Reset();
            // Collect all types for every resource in the types dictionary.
            // I was going to use _queryResults.Select(), but that doesn't work with Virtuoso.
            while (_tripleProvider.HasNext)
            {
                s = _tripleProvider.S;
                p = _tripleProvider.P.ToString();
                o = _tripleProvider.O;

                _tripleProvider.SetNext();

                if (o.NodeType == NodeType.Uri && p == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")
                {
                    if( s is IUriNode)
                    {
                        var suri = ((IUriNode)s).Uri.OriginalString;

                        var obj = ((IUriNode)o).Uri.OriginalString;

                        if (!types.ContainsKey(suri))
                        {
                            types.Add(suri, new List<Class>());
                        }

                        if (OntologyDiscovery.Classes.ContainsKey(obj))
                        {
                            types[suri].Add(OntologyDiscovery.Classes[obj]);
                        }
                        else
                        {
                            types[suri].Add(new Class(new Uri(obj)));
                        }
                    }
                }
            }
            
            // Iterate over all types and find the right class and instatiate it.
            foreach (var subject in types.Keys)
            {
                IList<Type> classType = MappingDiscovery.GetMatchingTypes(types[subject], type, inferencingEnabled);

                if (classType.Count > 0)
                {
#if DEBUG
                    if (classType.Count > 1)
                    {
                        var msg = "Info: There is more that one assignable type for <{0}>. It was initialized using the first.";
                        Debug.WriteLine(string.Format(msg, subject));
                    }
#endif

                    var resource = Activator.CreateInstance(classType[0], new Uri(subject));
                    if (resource is Resource res)
                    {
                        res.Model = _model;
                        res.IsNew = false;
                    }
                    result[subject] = resource;
                    
                }
#if DEBUG
                else if (type != typeof(Resource))
                {
                    var msg = "Info: No assignable type found for <{0}>.";

                    if (inferencingEnabled)
                    {
                        msg += " Try disabling inference.";
                    }

                    Debug.WriteLine(string.Format(msg, subject));
                }
#endif
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual int Count()
        {
            var countQuery = SparqlSerializer.SerializeCount(_model, _query);

            var query = new SparqlQuery(countQuery);

            var result = _store.ExecuteQuery(query.ToString());

            if (result is SparqlResultSet)
            {
                var set = result as SparqlResultSet;

                if (set.Count > 0 && set[0].Count > 0)
                {
                    var value = ParseCellValue(set[0][0]);

                    if (value.GetType() == typeof(int))
                    {
                        return (int)value;
                    }
                }
            }

            return -1;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
