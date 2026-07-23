using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blunatic.Core
{
    public struct VecRay
    {
        public Vec GridPosition;
        public Vector2 RelativePosition;
        public Vector2 CastVector;
        public float DistanceTravelled;

        public VecRay(Vec gridPosition, Vector2 relativePosition, Vector2 castVector, float distanceTravelled = 0)
        {
            if (castVector.X == 0 && castVector.Y == 0) throw new ArgumentException($"A VecRay cannot be cast in the direction {castVector}");

            GridPosition = gridPosition;
            RelativePosition = relativePosition;
            CastVector = castVector;
            DistanceTravelled = distanceTravelled;
        }

        /// <summary>
        /// Can be used to tell if a VecRay passing through a tile will just graze a corner, for potential diagonal casting transparency.
        /// Only reliable when travelling directly between edges.
        /// </summary>
        public readonly bool WillGrazeCorner()
        {
            // Distance from each face
            float distanceNorth = RelativePosition.Y;
            float distanceSouth = 1 - RelativePosition.Y;
            float distanceEast = 1 - RelativePosition.X;
            float distanceWest = RelativePosition.X;

            // Unit length cast vector
            Vector2 oneLongCastVector = CastVector / (float)Math.Sqrt(CastVector.X * CastVector.X + CastVector.Y * CastVector.Y);

            // Arrival "time"
            float arrivesNorthAt = distanceNorth / -oneLongCastVector.Y;
            float arrivesEastAt = distanceEast / oneLongCastVector.X;
            float arrivesSouthAt = distanceSouth / oneLongCastVector.Y;
            float arrivesWestAt = distanceWest / -oneLongCastVector.X;

            // Which arrival is in the future
            float primaryX = Math.Max(arrivesEastAt, arrivesWestAt);

            float primaryY = Math.Max(arrivesNorthAt, arrivesSouthAt);

            float primaryChange;

            if (primaryX < primaryY)
            {
                primaryChange = primaryX;
            }
            else
            {
                primaryChange = primaryY;
            }

            Vector2 travelled = primaryChange * oneLongCastVector;

            Vector2 newRelativePosition = RelativePosition + travelled;

            byte getQuadrant(Vector2 pos)
            {
                byte returner = 0;

                if (pos.X < 0.5) returner += 1;
                if (pos.X == 0.5) returner += 2;
                if (pos.Y < 0.5) returner += 4;
                if (pos.Y == 0.5) returner += 8;

                return returner;
            }

            return getQuadrant(RelativePosition) == getQuadrant(newRelativePosition);
        }
        public readonly VecRay Next()
        {
            // Distance from each face
            float distanceNorth = RelativePosition.Y;
            float distanceSouth = 1 - RelativePosition.Y;
            float distanceEast = 1 - RelativePosition.X;
            float distanceWest = RelativePosition.X;

            // Unit length cast vector
            Vector2 oneLongCastVector = CastVector / (float)Math.Sqrt(CastVector.X * CastVector.X + CastVector.Y * CastVector.Y);

            // Arrival "time"
            float arrivesNorthAt = distanceNorth / -oneLongCastVector.Y;
            float arrivesEastAt = distanceEast / oneLongCastVector.X;
            float arrivesSouthAt = distanceSouth / oneLongCastVector.Y;
            float arrivesWestAt = distanceWest / -oneLongCastVector.X;

            // Which arrival is in the future
            float primaryX = Math.Max(arrivesEastAt, arrivesWestAt);
            int xChange = Math.Sign(CastVector.X);

            float primaryY = Math.Max(arrivesNorthAt, arrivesSouthAt);
            int yChange = Math.Sign(CastVector.Y);

            float primaryChange;
            Vec gridPositionChange;

            Vec newGridPosition = GridPosition;
            float newDistanceTravelled = DistanceTravelled;

            if (primaryX < primaryY)
            {
                gridPositionChange = new Vec(xChange, 0);
                primaryChange = primaryX;
            }
            else
            {
                gridPositionChange = new Vec(0, yChange);
                primaryChange = primaryY;
            }

            newGridPosition += gridPositionChange;

            Vector2 travelled = primaryChange * oneLongCastVector;

            float travelledDistance = (float)Math.Sqrt(travelled.X * travelled.X + travelled.Y * travelled.Y);
            newDistanceTravelled += travelledDistance;

            Vector2 newRelativePosition = RelativePosition + travelled;

            // Refocus on the new tile
            newRelativePosition -= gridPositionChange;

            return new VecRay(newGridPosition, newRelativePosition, CastVector, newDistanceTravelled);
        }
    }
}
