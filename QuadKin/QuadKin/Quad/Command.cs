using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadKin
{
    public class Command
    {
        private static readonly double WIGGLE_ROOM = 0.2;
        public static readonly double ANGLE_ERROR_TOLERANCE = Math.PI / 4;
        
        public float UD { get; private set; }
        public float RL { get; private set; }
        public float FB { get; private set; }
        public float TRL { get; private set; }

        public bool valid { get; private set; }

        public Command(Skeleton skel)
        {
            BodyPartData rightArm = new BodyPartData(skel, BodyPart.RightArm);
            BodyPartData leftArm = new BodyPartData(skel, BodyPart.LeftArm);

            this.valid = this.isValidCommand(rightArm, leftArm);
            if (this.valid)
            {
                this.setUD(rightArm, leftArm);
                this.setRL(rightArm, leftArm);
                this.setFB(rightArm, leftArm);
                this.setTRL(rightArm, leftArm);
            }
            else
            {
                this.UD = this.RL = this.FB = this.TRL = 0;
            }
        }

        /// <summary>
        /// Returns true if both arms are more or less straight, and they dont point straight down.
        /// </summary>
        /// <param name="skel"></param>
        /// <returns></returns>
        private bool isValidCommand(BodyPartData rightArm, BodyPartData leftArm)
        {
            // Check for straightness.
            if (!rightArm.straight || !leftArm.straight)
                return false;
            else if ((rightArm.X < rightArm.Y && rightArm.Y < 0) && (leftArm.X < leftArm.Y && leftArm.Y < 0))
                return false;

            return true;
        }

        private void setUD(BodyPartData rightArm, BodyPartData leftArm)
        {
            double tempUD = (rightArm.Y + leftArm.Y) / 2;
            int sign = tempUD > 0 ? 1 : -1;
            this.UD = sign * (int)Math.Round(Math.Max(tempUD * sign - WIGGLE_ROOM, 0) / (1 - WIGGLE_ROOM) * 100);
        }

        private void setRL(BodyPartData rightArm, BodyPartData leftArm)
        {
            double tempRL = (leftArm.Y - rightArm.Y) / 2;
            int sign = tempRL > 0 ? 1 : -1;
            this.RL = sign * (int)Math.Round(Math.Max(tempRL * sign - WIGGLE_ROOM, 0) / (1 - WIGGLE_ROOM) * 100);
        }

        private void setFB(BodyPartData rightArm, BodyPartData leftArm)
        {
            double tempFB = -(rightArm.Z + leftArm.Z) / 2;
            int sign = tempFB > 0 ? 1 : -1;
            this.FB = sign * (int)Math.Round(Math.Max(tempFB * sign - WIGGLE_ROOM, 0) / (1 - WIGGLE_ROOM) * 100);
        }

        private void setTRL(BodyPartData rightArm, BodyPartData leftArm)
        {
            double tempTRL = (rightArm.Z - leftArm.Z) / 2;
            int sign = tempTRL > 0 ? 1 : -1;
            this.TRL = sign * (int)Math.Round(Math.Max(tempTRL * sign - WIGGLE_ROOM, 0) / (1 - WIGGLE_ROOM) * 100);
        }
        
    }

    enum BodyPart
    {
        RightArm = 0,
        LeftArm = 1
    }

    class BodyPartData
    {
        private static readonly List<JointType> RightArm = new List<JointType>(){
            JointType.ShoulderRight,
            JointType.ElbowRight,
            JointType.WristRight,
            JointType.HandRight
        };

        private static readonly List<JointType> LeftArm = new List<JointType>(){
            JointType.ShoulderLeft,
            JointType.ElbowLeft,
            JointType.WristLeft,
            JointType.HandLeft
        };

        public bool straight { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }

        public BodyPartData(Skeleton skel, BodyPart bodyPart)
        {
            List<JointType> joints;

            switch (bodyPart)
            {
		        case BodyPart.RightArm:
                    joints = RightArm;
                    break;
                case BodyPart.LeftArm:
                    joints = LeftArm;
                    break;
                default:
                    joints = RightArm;
                    break;
	        }
            
            this.X = skel.Joints[joints[1]].Position.X - skel.Joints[joints[0]].Position.X;
            this.Y = skel.Joints[joints[1]].Position.Y - skel.Joints[joints[0]].Position.Y;
            this.Z = skel.Joints[joints[1]].Position.Z - skel.Joints[joints[0]].Position.Z;

            // The angle error is the sum of the angle between the difference vector between the first two joints
            // and all the other difference vectors. It is used to see how straight the bodypart is.
            double angleError = 0;

            double previousDX = this.X;
            double previousDY = this.Y;
            double previousDZ = this.Z;
            double previousL = Math.Pow(Math.Pow(this.X, 2) + Math.Pow(this.Y, 2) + Math.Pow(this.Z, 2), 0.5);

            for (int i = 2; i < joints.Count; i++)
            {
                double tempDX = skel.Joints[joints[i]].Position.X - skel.Joints[joints[i - 1]].Position.X;
                double tempDY = skel.Joints[joints[i]].Position.Y - skel.Joints[joints[i - 1]].Position.Y;
                double tempDZ = skel.Joints[joints[i]].Position.Z - skel.Joints[joints[i - 1]].Position.Z;
                double tempL = Math.Pow(Math.Pow(tempDX, 2) + Math.Pow(tempDY, 2) + Math.Pow(tempDZ, 2), 0.5);

                double tempAngleError = Math.Acos((previousDX * tempDX + previousDY * tempDY + previousDZ * tempDZ) / (previousL * tempL));

                angleError += tempAngleError > Math.PI / 9 ? Math.PI : tempAngleError;

                this.X += tempDX;
                this.Y += tempDY;
                this.Z += tempDZ;

                previousDX = tempDX;
                previousDY = tempDY;
                previousDZ = tempDZ;
                previousL = tempL;
            }

            double length = Math.Pow(Math.Pow(this.X, 2) + Math.Pow(this.Y, 2) + Math.Pow(this.Z, 2), 0.5);
            this.X /= length;
            this.Y /= length;
            this.Z /= length;

            this.X = angleError;

            this.straight = angleError < Command.ANGLE_ERROR_TOLERANCE;
        }
    }
}