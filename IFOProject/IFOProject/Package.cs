using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace IFOProject
{
    public class Package
    {
        /// <summary>
        /// Interference patterns list
        /// </summary>
        private List<Pattern> patterns;
        /// <summary>
        /// Current pattern index
        /// </summary>
        private int current;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Package()
        {
            patterns = new List<Pattern>();
        }

        /// <summary>
        /// Currently selected pattern
        /// </summary>
        public Pattern CurrentPattern
        {
            get
            {
                if (patterns.Count == 0) return null;
                return patterns[current];
            }
        }

        /// <summary>
        /// Current pattern index
        /// </summary>
        public int CurrentIndex { get { return current; } }

        /// <summary>
        /// Count of patterns in package
        /// </summary>
        public int PatternsCount { get { return patterns.Count; } }

        /// <summary>
        /// Adds pattern from image file and sets focus on it
        /// </summary>
        /// <param name="fileName">Full path to image file</param>
        public void Add(string fileName)
        {
            patterns.Add(new Pattern(fileName));
            current = patterns.Count - 1;
        }

        /// <summary>
        /// Adds multiple patterns from image files
        /// </summary>
        /// <param name="fileNames">Full pathes to image files</param>
        public void Add(string[] fileNames)
        {
            foreach (string name in fileNames) Add(name);
            current = 0;
        }

        /// <summary>
        /// Removes current pattern from package
        /// </summary>
        public bool Remove()
        {
            if (patterns.Count == 0) return false;
            else patterns.RemoveAt(current);
            if (current >= patterns.Count) current = patterns.Count - 1;
            return true;
        }

        /// <summary>
        /// Removes all patterns from package
        /// </summary>
        public bool Clear()
        {
            if (patterns.Count == 0) return false;
            else patterns.Clear();
            current = 0;
            return true;
        }

        /// <summary>
        /// Moves to next pattern
        /// </summary>
        public bool MoveNext()
        {
            if (current + 1 < patterns.Count)
            {
                current++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Moves to previous pattern
        /// </summary>
        public bool MovePrevious()
        {
            if (current - 1 >= 0)
            {
                current--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calculates and shows results for all patterns in one form
        /// </summary>
        /// <param name="area">Calculation rectangle</param>
        /// <param name="step">Take every step-row</param>
        /// <param name="smooth">Smoothing radius</param>
        public void CalculateAndShow(Rectangle area, int step, int smooth)
        {
            throw new System.NotImplementedException();
        }
    }
}
