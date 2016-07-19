using System;

namespace mTSP_GA
{
    public class City
    {
        public City(string name, double xCoord, double yCoord)
        {
            Name = name;
            this.xCoord = xCoord;
            this.yCoord = yCoord;
        }

        public string Name { set; get; }
        public double xCoord { get; set; }
        public double yCoord { get; set; }

        public double GetDistanceFromPosition(double otherXCoord, double otherYCoord)
        {
            return Math.Sqrt((xCoord - otherXCoord) * (xCoord - otherXCoord) + (yCoord - otherYCoord) * (yCoord - otherYCoord));
        }

        private static double DegreesToRadians(double deg)
        {
            return deg * (System.Math.PI / 180);
        }

        public byte[] ToBinaryString()
        {
            var result = new byte[6];

            return result;
        }
    }
}