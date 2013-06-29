// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace ICSharpCode.NRefactory.PatternMatching
{
	/// <summary>
	/// Represents the result of a pattern matching operation.
	/// </summary>
	public struct Match
	{
		// TODO: maybe we should add an implicit Match->bool conversion? (implicit operator bool(Match m) { return m != null; })
		
		List<KeyValuePair<string, INode>> results;
		
		public bool Success {
			get { return results != null; }
		}
		
		internal static Match CreateNew()
		{
			Match m;
			m.results = new List<KeyValuePair<string, INode>>();
			return m;
		}
		
		internal int CheckPoint()
		{
			return results.Count;
		}
		
		internal void RestoreCheckPoint(int checkPoint)
		{
			results.RemoveRange(checkPoint, results.Count - checkPoint);
		}
		
		public IEnumerable<INode> Get(string groupName)
		{
			if (results == null)
				yield break;
			foreach (var pair in results) {
				if (pair.Key == groupName)
					yield return pair.Value;
			}
        }

        /// <summary>
        /// Gets last capture group or null if no group with given name found. 
        /// </summary>
        /// <param name="groupName">Name of the group for which has to be returned last occurence.</param>
        /// <returns>Last occurence of the capture group or null if no group with given name found.</returns>
        public INode LastOrDefault(string groupName)
        {
            var node = this.Get(groupName).LastOrDefault();
            return node;
        }

        /// <summary>
        /// Gets last capture group 
        /// </summary>
        /// <param name="groupName">Name of the group for which has to be returned last occurence.</param>
        /// <returns>Last occurence of the capture group.</returns>
        public INode Last(string groupName)
        {
            var backReferenceMatch = this.LastOrDefault(groupName);
            if (backReferenceMatch == null)
            {
                throw new InvalidOperationException(string.Format("Backreference {0} could not be found", groupName));
            }

            return backReferenceMatch;
        }
		
		public IEnumerable<T> Get<T>(string groupName) where T : INode
		{
			if (results == null)
				yield break;
			foreach (var pair in results) {
				if (pair.Key == groupName)
					yield return (T)pair.Value;
			}
		}

        /// <summary>
        /// Gets capture group as single entity or null if nothing captured.
        /// </summary>
        /// <typeparam name="T">Type of capture group.</typeparam>
        /// <param name="groupName">Name of the capture group.</param>
        /// <returns>Captured group if only one group with given name or null, if nothing captured.</returns>
        /// <exception cref="InvalidOperationException">The group does not exist, or more then one instance of it exists.</exception>
        public T SingleOrDefault<T>(string groupName) where T : INode
        {
            var groupNodes = this.Get<T>(groupName);
            var node = groupNodes.FirstOrDefault();
            if (node == null)
            {
                return default(T);
            }

            if (groupNodes.Skip(1).FirstOrDefault() != null)
            {
                throw new InvalidOperationException(string.Format("More the one capture of the group {0} exist.", groupName));
            }

            return node;
        }

        /// <summary>
        /// Gets capture group as single entity.
        /// </summary>
        /// <typeparam name="T">Type of capture group.</typeparam>
        /// <param name="groupName">Name of the capture group.</param>
        /// <returns>Captured group if only one group with given name.</returns>
        /// <exception cref="InvalidOperationException">The group does not exist, or more then one instance of it exists.</exception>
        public T Single<T>(string groupName) where T : INode
        {
            var node = this.SingleOrDefault<T>(groupName);
            if (node == null)
            {
                throw new InvalidOperationException(string.Format("Group name {0} could not be found", groupName));
            }

            return node;
        }

        /// <summary>
        /// Gets first capture group or null if no group with given name found. 
        /// </summary>
        /// <typeparam name="T">Type of capture group.</typeparam>
        /// <param name="groupName">Name of the group for which has to be returned first occurence.</param>
        /// <returns>First occurence of the capture group or null if no group with given name found.</returns>
        public T FirstOrDefault<T>(string groupName) where T : INode
        {
            var node = this.Get<T>(groupName).FirstOrDefault();
            return node;
        }

        /// <summary>
        /// Gets first capture group 
        /// </summary>
        /// <typeparam name="T">Type of capture group.</typeparam>
        /// <param name="groupName">Name of the group for which has to be returned first occurence.</param>
        /// <returns>First occurence of the capture group.</returns>
        public T First<T>(string groupName) where T : INode
        {
            var backReferenceMatch = this.FirstOrDefault<T>(groupName);
            if (backReferenceMatch == null)
            {
                throw new InvalidOperationException(string.Format("Group {0} could not be found", groupName));
            }

            return backReferenceMatch;
        }

        public bool Has(string groupName)
		{
			if (results == null)
				return false;
			foreach (var pair in results) {
				if (pair.Key == groupName)
					return true;
			}
			return false;
		}
		
		public void Add(string groupName, INode node)
		{
			if (groupName != null && node != null) {
				results.Add(new KeyValuePair<string, INode>(groupName, node));
			}
		}
	}
}
