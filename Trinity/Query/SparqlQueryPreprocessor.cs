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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;

namespace Semiodesk.Trinity
{
    /// <summary>
    /// A preprocsesor for SPARQL queries.
    /// </summary>
    public class SparqlQueryPreprocessor : SparqlPreprocessor
    {
        #region Members

        /// <summary>
        /// The SPARQL query form, i.e. ASK, DESCRIBE, SELECT, CONSTRUCT.
        /// </summary>
        public SparqlQueryType QueryType { get; protected set; }

        private int _nestingLevel;

        // STATEMENT VARIABLES

        private bool _parseVariables;

        private SparqlQueryVariableScope _variableScope = SparqlQueryVariableScope.Default;

        /// <summary>
        /// Indicates if the query returns triples.
        /// </summary>
        public bool QueryProvidesStatements { get; protected set; }

        /// <summary>
        /// Variables visible in the query root scope.
        /// </summary>
        public readonly List<string> GlobalScopeVariables = new List<string>();

        /// <summary>
        /// Variables only visible in local scope.
        /// </summary>
        public readonly List<string> InScopeVariables = new List<string>();

        // SOLUTION MODIFIERS

        private IToken _offsetValueToken;

        private IToken _limitValueToken;

        /// <summary>
        /// Indicates if the query has an ORDER BY solution modifier.
        /// </summary>
        public bool IsOrdered { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance of the <c>SparqlQueryPreprocessor</c> class.
        /// </summary>
        /// <param name="input">A text reader.</param>
        /// <param name="syntax">SPARQL syntax level.</param>
        public SparqlQueryPreprocessor(TextReader input, SparqlQuerySyntax syntax)
            : base(input, syntax)
        {
            QueryType = SparqlQueryType.Unknown;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the next token in the query and advance the reader position.
        /// </summary>
        /// <returns>A SPARQL token.</returns>
        public override IToken GetNextToken()
        {
            var token = base.GetNextToken();

            switch (token.TokenType)
            {
                case Token.ASK:
                    {
                        QueryType = SparqlQueryType.Ask;

                        if (_nestingLevel == 0)
                        {
                            _parseVariables = false;
                            QueryProvidesStatements = false;
                        }

                        break;
                    }
                case Token.DESCRIBE:
                    {
                        QueryType = SparqlQueryType.Describe;

                        if (_nestingLevel == 0)
                        {
                            _parseVariables = false;
                            QueryProvidesStatements = true;
                        }

                        break;
                    }
                case Token.SELECT:
                    {
                        QueryType = SparqlQueryType.Select;

                        if (_nestingLevel == 0)
                        {
                            _parseVariables = true;
                            QueryProvidesStatements = false;
                        }

                        break;
                    }
                case Token.CONSTRUCT:
                    {
                        QueryType = SparqlQueryType.Construct;

                        if (_nestingLevel == 0)
                        {
                            _parseVariables = false;
                            QueryProvidesStatements = true;
                        }

                        break;
                    }
                case Token.EOF:
                    {
                        if (_variableScope == SparqlQueryVariableScope.Global && GlobalScopeVariables.Count == 3)
                        {
                            // NOTE: This does not yet take into account that all variables need to be in S|P|O positions.
                            QueryProvidesStatements = true;
                        }

                        break;
                    }
                case Token.LEFTCURLYBRACKET:
                    {
                        _nestingLevel += 1;

                        if (_parseVariables)
                        {
                            ProcessInScopeVariables(token);
                        }

                        break;
                    }
                case Token.RIGHTCURLYBRACKET:
                    {
                        _nestingLevel -= 1;

                        // Do not parse variables which are being used in solution modifiers.
                        _parseVariables &= _nestingLevel > 0;

                        if (_parseVariables)
                        {
                            ProcessInScopeVariables(token);
                        }

                        break;
                    }
                case Token.DOT:
                    {
                        if (_parseVariables)
                        {
                            ProcessInScopeVariables(token);
                        }

                        break;
                    }
                case Token.ALL:
                    {
                        // The query has a wild-card selector. We collect all in-scope variables of
                        // the query to determine if it provides triples/statements.
                        _variableScope = SparqlQueryVariableScope.Global;

                        break;
                    }
                case Token.VARIABLE:
                    {
                        if(_parseVariables)
                        {
                            if (_nestingLevel == 0)
                            {
                                if (!GlobalScopeVariables.Contains(token.Value))
                                {
                                    // Do not add global scope variables twice.
                                    GlobalScopeVariables.Add(token.Value);
                                }
                            }
                            else
                            {
                                InScopeVariables.Add(token.Value);

                                if (_variableScope == SparqlQueryVariableScope.Global)
                                {
                                    // If we have a wildcard selector '*', we accumulate all variables of 
                                    // the query as global variables. After parsing, there must only be 
                                    // three for providing triples.
                                    if (!GlobalScopeVariables.Contains(token.Value))
                                    {
                                        // Do not add global scope variables twice.
                                        GlobalScopeVariables.Add(token.Value);
                                    }
                                }
                            }
                        }

                        break;
                    }
                case Token.ORDERBY:
                    {
                        IsOrdered = true;

                        break;
                    }
                case Token.LITERAL:
                    {
                        if (_nestingLevel == 0)
                        {
                            switch (PreviousTokenType)
                            {
                                case Token.OFFSET:
                                    {
                                        _offsetValueToken = token;
                                        break;
                                    }
                                case Token.LIMIT:
                                    {
                                        _limitValueToken = token;
                                        break;
                                    }
                            }
                        }

                        break;
                    }
            }

            return token;
        }

        private void ProcessInScopeVariables(IToken token)
        {
            if(GlobalScopeVariables.Count == 3)
            {
                if (!QueryProvidesStatements)
                {
                    // We compare the in-scope variables with the global variables. The query only
                    // provides statements if there is one triple pattern that contains the global
                    // variables in excactly the same order.
                    QueryProvidesStatements = Enumerable.SequenceEqual(GlobalScopeVariables, InScopeVariables);
                }
            }
            else
            {
                QueryProvidesStatements = false;
            }

            // After parsing a triple pattern, clear the in-scope variable cache.
            InScopeVariables.Clear();
        }

        /// <summary>
        /// Adds a LIMIT clause to the query in order to restrict it to put an upper bound on the number of solutions returned. 
        /// </summary>
        /// <param name="limit">The number of return values.</param>
        public void SetLimit(int limit)
        {
            var value = limit.ToString();

            if (_limitValueToken == null)
            {
                Tokens.Insert(Tokens.Count - 1, new OffsetKeywordToken(-1, -1));
                Tokens.Insert(Tokens.Count - 1, new PlainLiteralToken(value, -1, -1, -1));
            }
            else
            {
                var i = Tokens.IndexOf(_limitValueToken);

                _limitValueToken = new PlainLiteralToken(value, -1, -1, -1);

                Tokens[i] = _limitValueToken;
            }
        }

        /// <summary>
        /// Adds an OFFSET clause to the query which causes the solutions generated to start after the specified number of solutions. 
        /// </summary>
        /// <param name="offset">The number of return values.</param>
        public void SetOffset(int offset)
        {
            var value = offset.ToString();

            if (_offsetValueToken == null)
            {
                Tokens.Insert(Tokens.Count - 1, new OffsetKeywordToken(-1, -1));
                Tokens.Insert(Tokens.Count - 1, new PlainLiteralToken(value, -1, -1, -1));
            }
            else
            {
                var i = Tokens.IndexOf(_offsetValueToken);

                _offsetValueToken = new PlainLiteralToken(value, -1, -1, -1);

                Tokens[i] = _offsetValueToken;
            }
        }

        /// <summary>
        /// Get the entire SPARQL query string.
        /// </summary>
        /// <returns>A SPARQL query string.</returns>
        public string GetRootGraphPattern()
        {
            return Serialize(1);
        }

        /// <summary>
        /// Get the ORDER BY clause.
        /// </summary>
        /// <returns>A string.</returns>
        public string GetOrderByClause()
        {
            // We want the next token after the last closing curly bracket.
            var i = Tokens.FindLastIndex(t => t.TokenType == Token.RIGHTCURLYBRACKET) + 1;

            // If i == 0, then FindLastIndex returned -1;
            if (i == 0)
            {
                return "";
            }

            var resultBuilder = new StringBuilder();

            var orderby = false;

            while (i < Tokens.Count)
            {
                var token = Tokens[i];

                orderby = token.TokenType == Token.ORDERBY || orderby && token.TokenType == Token.VARIABLE;

                if (orderby)
                {
                    resultBuilder.Append(token.Value);
                    resultBuilder.Append(' ');
                }
                else if (resultBuilder.Length > 0)
                {
                    break;
                }

                i++;
            }

            return resultBuilder.ToString();
        }

        #endregion
    }
}
