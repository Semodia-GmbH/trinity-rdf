

using Remotion.Linq;
using Remotion.Linq.Clauses.ResultOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Semiodesk.Trinity.Query
{
    internal class SparqlQueryExecutor : IQueryExecutor
    {
        #region Members
        
        protected IModel Model { get; private set; }

        // A handle to the generic version of the GetResources method which is being used
        // for implementing the ExecuteCollection(QueryModel) method that supports runtime type specification.
        private MethodInfo _getResourceMethod;

        private bool _inferenceEnabled;

        #endregion

        #region Constructors

        public SparqlQueryExecutor(IModel model, bool inferenceEnabled)
        {
            Model = model;

            _inferenceEnabled = inferenceEnabled;

            // Searches for the generic method IEnumerable<T> GetResources<T>(ResourceQuery) and saves a handle
            // for later use within ExecuteCollection(QueryModel);
            _getResourceMethod = model.GetType().GetMethods().FirstOrDefault(m => m.IsGenericMethod && m.Name == "GetResources" && m.GetParameters().Any(p => p.ParameterType == typeof(ISparqlQuery)));
        }
        
        #endregion

        #region Methods

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var t = queryModel.SelectClause.Selector.Type;

            if(typeof(Resource).IsAssignableFrom(t))
            {
                // Handle queries which return instances of resources.
                var visitor = new SparqlQueryModelVisitor<T>(new SelectTriplesQueryGenerator());
                visitor.VisitQueryModel(queryModel);

                var getResources = _getResourceMethod.MakeGenericMethod(typeof(T));
                var args = new object[] { visitor.GetQuery(), _inferenceEnabled, null };

                foreach (var value in getResources.Invoke(Model, args) as IEnumerable<T>)
                {
                    yield return value;
                }
            }
            else
            {
                // Handle queries which return value type objects.
                var visitor = new SparqlQueryModelVisitor<T>(new SelectBindingsQueryGenerator());
                visitor.VisitQueryModel(queryModel);

                var query = visitor.GetQuery();
                var result = Model.ExecuteQuery(query, _inferenceEnabled);

                // TODO: This works correctly for single bindings, check with multiple bindings.
                foreach(var bindings in result.GetBindings())
                {
                    foreach(var value in bindings.Values.OfType<T>())
                    {
                        yield return value;
                    }
                }
            }
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            var sequence = ExecuteCollection<T>(queryModel);

            return returnDefaultWhenEmpty ? sequence.SingleOrDefault() : sequence.Single();
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            var t = typeof(T);

            if(t == typeof(bool))
            {
                // Generate and execute ASK query.
                var visitor = new SparqlQueryModelVisitor<T>(new AskQueryGenerator());
                visitor.VisitQueryModel(queryModel);

                var query = visitor.GetQuery();
                var result = Model.ExecuteQuery(query, _inferenceEnabled);

                return new object[] { result.GetAnwser() }.OfType<T>().First();
            }
            else if(queryModel.ResultOperators.Any(o => o is CountResultOperator))
            {
                var visitor = new SparqlQueryModelVisitor<T>(new SelectBindingsQueryGenerator());
                visitor.VisitQueryModel(queryModel);

                var query = visitor.GetQuery();
                var result = Model.ExecuteQuery(query, _inferenceEnabled);

                var b = result.GetBindings().FirstOrDefault();

                if(b != null && b.Any())
                {
                    return new object[] { b.First().Value }.OfType<T>().First();
                }
                else
                {
                    return new object[] { 0 }.OfType<T>().First();
                }
            }
            else
            {
                // Unknown scalar type.
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
