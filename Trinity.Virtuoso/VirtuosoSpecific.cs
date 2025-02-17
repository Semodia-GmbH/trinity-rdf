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
//  Jan Funke <jan.funke@semodia.com>
//
// Copyright (c) Semiodesk GmbH 2018

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Semiodesk.Trinity.Configuration;

namespace Semiodesk.Trinity.Store.Virtuoso
{
    [XmlRoot(ElementName = "graph")]
    public class Graph
    {
        [XmlAttribute(AttributeName = "uri")]
        public string Uri { get; set; }
    }

    [XmlRoot(ElementName = "ruleset")]
    public class Ruleset
    {
        [XmlElement(ElementName = "graph")]
        public List<Graph> GraphCollection { get; set; }
        [XmlAttribute(AttributeName = "uri")]
        public string Uri { get; set; }
    }

    [XmlRoot(ElementName = "rulesets")]
    public class Rulesets
    {
        [XmlElement(ElementName = "ruleset")]
        public List<Ruleset> RulesetCollection { get; set; }
    }


    internal class VirtuosoSettings 
    {
        
        #region Members
        public IStoreConfiguration Settings { get; set; }
        public Rulesets Rulesets { get; set; }
        #endregion

        public VirtuosoSettings(IStoreConfiguration settings)
        {
            Settings = settings;
            var serializer = new XmlSerializer(typeof(Rulesets));
            Rulesets = (Rulesets)serializer.Deserialize(settings.Data.CreateReader());
        }

        public void Update(VirtuosoStore store)
        {
            var virtuosoStore = (store as VirtuosoStore);

            foreach (var set in Rulesets.RulesetCollection)
            {
                ClearRuleSet(new Uri(set.Uri), virtuosoStore);
                foreach (var item in set.GraphCollection)
                {
                    AddGraphToRuleSet(new Uri(set.Uri), new Uri(item.Uri), virtuosoStore);
                }
            }
        }

        private void ClearRuleSet(Uri ruleSet, VirtuosoStore store)
        {
            try
            {
                var query = $"delete * from DB.DBA.SYS_RDF_SCHEMA where RS_NAME='{ruleSet.OriginalString}';";
                store.ExecuteQuery(query);
            }catch(Exception)
            {
            }
        }

        private void RemoveGraphFromRuleSet(Uri ruleSet, Uri graph, VirtuosoStore store)
        {
            try
            {
                var query = $"rdfs_rule_set ('{ruleSet}', '{graph}', 1)";
                store.ExecuteQuery(query);
            }
            catch (Exception)
            {
            }
        }

        private void AddGraphToRuleSet(Uri ruleSet, Uri graph, VirtuosoStore store)
        {
            var query = $"rdfs_rule_set ('{ruleSet}', '{graph}')";
            store.ExecuteQuery(query);
        }
        
    }


}
