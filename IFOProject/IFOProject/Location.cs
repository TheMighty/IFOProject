using System;
using System.Drawing;

namespace IFOProject
{
    public struct Location
    {
        /// <summary>
        /// X - coordinate
        /// </summary>
        public int x;
        /// <summary>
        /// Y - coordinate
        /// </summary>
        public int y;
        /// <summary>
        /// Value indicating whether the point was initialized
        /// </summary>
        public bool exists;

        /// <summary>
        /// Coordinates constructor
        /// </summary>
        /// <param name="x">X - coordinate</param>
        /// <param name="y">Y - coordinate</param>
        public Location(int x, int y)
        {
            this.x = x;
            this.y = y;
            exists = true;
        }

        /// <summary>
        /// Point constructor
        /// </summary>
        /// <param name="point">Point to construct from</param>
        public Location(Point point)
        {
            x = point.X;
            y = point.Y;
            exists = true;
        }

        /// <summary>
        /// Location constructor
        /// </summary>
        /// <param name="location">Location to construct from</param>
        public Location(Location location)
        {
            x = location.x;
            y = location.y;
            exists = location.exists;
        }

        /// <summary>
        /// Subtracts two locations
        /// </summary>
        /// <param name="first">Left operand</param>
        /// <param name="second">Right operand</param>
        /// <returns>Subtracted location</returns>
        public static Location operator - (Location first, Location second)
        {
            return new Location(first.x - second.x, first.y - second.y);
        }
    }
}
