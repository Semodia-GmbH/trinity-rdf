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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;


namespace Semiodesk.Trinity
{
    /// <summary>
    /// Provides functionality to perform serialization of native .NET types into SPARQL strings.
    /// </summary>
    public class SparqlSerializer
    {
        #region Methods

        /// <summary>
        /// Serializes a string and excapes special characters.
        /// </summary>
        /// <param name="str">A string literal.</param>
        /// <returns></returns>
        public static string SerializeString(string str)
        {
            // We need to escape specrial characters: http://www.w3.org/TeamSubmission/turtle/#sec-strings
            var s = str.Replace(@"\", @"\\");

            if(s.Contains('\n'))
            {
                return $"'''{s}'''";
            }
            else
            {
                s = s.Replace("'", "\\'");

                return $"'{s}'";
            }
        }

        /// <summary>
        /// Serializes a string with a translation
        /// </summary>
        /// <param name="str">A string literal.</param>
        /// <param name="lang">A language tag.</param>
        /// <returns></returns>
        public static string SerializeTranslatedString(string str, string lang)
        {
            return $"{SerializeString(str)}@{lang}";
        }

        /// <summary>
        /// Serializes a typed literal.
        /// </summary>
        /// <param name="obj">A value.</param>
        /// <param name="typeUri">A type URI.</param>
        /// <returns></returns>
        public static string SerializeTypedLiteral(object obj, Uri typeUri)
        {
            return $"'{XsdTypeMapper.SerializeObject(obj)}'^^<{typeUri}>";
        }

        /// <summary>
        /// Serializes a value depending on its type.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns></returns>
        public static string SerializeValue(object obj)
        {
            try
            {
                switch (obj)
                {
                    case string s:
                        return SerializeString(s);
                    // string + language
                    case string[] strings:
                        return SerializeTranslatedString(strings[0], strings[1]);
                    // string + language
                    case Tuple<string, CultureInfo> tuple:
                        return SerializeTranslatedString(tuple.Item1, tuple.Item2.Name);
                    // string + language
                    case Tuple<string, string> array:
                        return SerializeTranslatedString(array.Item1, array.Item2);
                    // list of strings + language
                    case IEnumerable<Tuple<string, CultureInfo>> translatedString:
                        return SerializeTranslatedString(translatedString);
                    case Uri uri:
                        return SerializeUri(uri);
                    case IResource resource:
                        return SerializeUri(resource.Uri);
                    case IModel model:
                        return SerializeUri(model.Uri);
                    default:
                        return SerializeTypedLiteral(obj, XsdTypeMapper.GetXsdTypeUri(obj.GetType()));
                }
            }
            catch
            {
                var msg = $"No serializer available for object of type {obj.GetType()}.";
                throw new ArgumentException(msg);
            }
        }

        private static string SerializeTranslatedString(IEnumerable<Tuple<string, CultureInfo>> translatedString)
        {
            if (!translatedString.Any()) return string.Empty;
            
            var result = new StringBuilder();
            var count = translatedString.Count();
            foreach (var (translation, culture) in translatedString)
            {
                count--;
                result.Append(SerializeTranslatedString(translation, culture.Name));
                if (count > 0) result.Append(", ");
            }
            
            return result.ToString();
        }

        /// <summary>
        /// Serializes a URI.
        /// </summary>
        /// <param name="uri">A uniform resource identifier.</param>
        /// <returns></returns>
        public static string SerializeUri(Uri uri)
        {
            return uri.OriginalString.StartsWith("_") ? uri.OriginalString : $"<{uri.OriginalString}>";
        }

        /// <summary>
        /// Serializes a resource.
        /// </summary>
        /// <param name="resource">A resource.</param>
        /// <param name="ignoreUnmappedProperties">Ignores all unmapped properties for serialization.</param>
        /// <returns></returns>
        public static string SerializeResource(IResource resource, bool ignoreUnmappedProperties=false)
        {
            var valueList = resource.ListValues(ignoreUnmappedProperties);

            if (!valueList.Any())
            {
                return string.Empty;
            }

            var subject = SerializeUri(resource.Uri);

            var result = new StringBuilder(subject);
            result.Append(' ');

            foreach (var value in valueList)
            {
                switch (value.Item2)
                {
                    case null:
                    case IEnumerable list when !list.GetEnumerator().MoveNext():
                        continue;
                    default:
                        result.AppendFormat("{0} {1}; ", SerializeUri(value.Item1.Uri), SerializeValue(value.Item2));
                        break;
                }
            }

            result[result.Length - 2] = '.';

            return result.ToString();
        }

        /// <summary>
        /// Generate the dataset clause for a given model.
        /// </summary>
        /// <param name="model">A model.</param>
        /// <returns></returns>
        public static string GenerateDatasetClause(IModel model)
        {
            switch (model)
            {
                case null:
                    return "";
                case IModelGroup group:
                    return GenerateDatasetClause(group);
                default:
                    return $"FROM {SerializeUri(model.Uri)} ";
            }
        }

        /// <summary>
        /// Generate a dataset clause for a model group.
        /// </summary>
        /// <param name="modelGroup">A model group.</param>
        /// <returns></returns>
        public static string GenerateDatasetClause(IModelGroup modelGroup)
        {
            if (modelGroup is ModelGroup group)
            {
                return group.DatasetClause;
            }
            else
            {
                return GenerateDatasetClause(modelGroup as IEnumerable<IModel>);
            }
        }

        /// <summary>
        /// Generate a dataset clause for an enumeration of models.
        /// </summary>
        /// <param name="models">An enumeration of models.</param>
        /// <returns></returns>
        public static string GenerateDatasetClause(IEnumerable<IModel> models)
        {
            if (!models.Any())
            {
                return "";
            }

            var resultBuilder = new StringBuilder();

            foreach (var model in models)
            {
                resultBuilder.Append("FROM ");
                resultBuilder.Append(SparqlSerializer.SerializeUri(model.Uri));
                resultBuilder.Append(" ");
            }

            return resultBuilder.ToString();
        }

        /// <summary>
        /// Serialize a count query for the given SPARQL query.
        /// </summary>
        /// <param name="model">The model to be queried.</param>
        /// <param name="query">The query which results should be counted.</param>
        /// <returns></returns>
        public static string SerializeCount(IModel model, ISparqlQuery query)
        {
            var variable = "?" + query.GetGlobalScopeVariableNames()[0];
            var from = GenerateDatasetClause(model);
            var where = query.GetRootGraphPattern();

            var queryBuilder = new StringBuilder();

            queryBuilder.Append("SELECT ( COUNT(DISTINCT ");
            queryBuilder.Append(variable);
            queryBuilder.Append(") AS ?count )");
            queryBuilder.Append(from);
            queryBuilder.Append(" WHERE { ");
            queryBuilder.Append(where);
            queryBuilder.Append(" }");

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Generate a query which returns the URIs of all resources selected in a given query.
        /// </summary>
        /// <param name="model">The model to be queried.</param>
        /// <param name="query">The SPARQL query which provides resources.</param>
        /// <param name="offset">Offset solution modifier.</param>
        /// <param name="limit">Limit solution modifier.</param>
        /// <returns></returns>
        public static string SerializeFetchUris(IModel model, ISparqlQuery query, int offset = -1, int limit = -1)
        {
            var variable = "?" + query.GetGlobalScopeVariableNames()[0];
            var from = GenerateDatasetClause(model);
            var where = query.GetRootGraphPattern();
            var orderby = query.GetRootOrderByClause();

            var queryBuilder = new StringBuilder();
            
            foreach(var prefix in query.GetDeclaredPrefixes())
            {
                queryBuilder.Append($"PREFIX <{prefix}> ");
            }

            queryBuilder.Append("SELECT DISTINCT ");
            queryBuilder.Append(variable);
            queryBuilder.Append(from);
            queryBuilder.Append(" WHERE { ");
            queryBuilder.Append(where);
            queryBuilder.Append(" } ");
            queryBuilder.Append(orderby);

            if (offset != -1)
            {
                queryBuilder.Append(" OFFSET ");
                queryBuilder.Append(offset);
            }

            if (limit != -1)
            {
                queryBuilder.Append(" LIMIT ");
                queryBuilder.Append(limit);
            }

            return queryBuilder.ToString();
        }

        /// <summary>
        /// Add an offset or limit solution modifier to a given SPARQL query.
        /// </summary>
        /// <param name="model">The model to be queried.</param>
        /// <param name="query">The SPARQL query to be executed.</param>
        /// <param name="offset">Offset solution modifier.</param>
        /// <param name="limit">Limit solution modifier.</param>
        /// <returns></returns>
        public static string SerializeOffsetLimit(IModel model, ISparqlQuery query, int offset = -1, int limit = -1)
        {
            var variable = "?" + query.GetGlobalScopeVariableNames()[0];
            var from = GenerateDatasetClause(model);
            var where = query.GetRootGraphPattern();

            var resultBuilder = new StringBuilder();
            resultBuilder.AppendFormat("SELECT {0} ?p ?o {1} WHERE {{ {0} ?p ?o {{", variable, from);
            resultBuilder.AppendFormat("SELECT DISTINCT {0} WHERE {{ {1} }}", variable, where);

            if (offset != -1)
            {
                resultBuilder.Append(" OFFSET ");
                resultBuilder.Append(offset);
            }

            if (limit != -1)
            {
                resultBuilder.Append(" LIMIT ");
                resultBuilder.Append(limit);
            }

            resultBuilder.Append(" } }");

            return resultBuilder.ToString();
        }

        #endregion
    }
}
