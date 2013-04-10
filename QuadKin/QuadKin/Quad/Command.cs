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
        private static readonly float WIGGLE_ROOM = 0.25f;
        public static readonly float ANGLE_ERROR_TOLERANCE = 4.0f;
        
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
            this.setUD(rightArm, leftArm);
            this.setRL(rightArm, leftArm);
            this.setFB(rightArm, leftArm);
            this.setTRL(rightArm, leftArm);

            //float max = Math.Max(Math.Max(this.UD, this.RL), Math.Max(this.FB, this.TRL));
            //if (this.UD < max) this.UD = 0;
            //if (this.RL < max) this.RL = 0;
            //if (this.FB < max) this.FB = 0;
            //if (this.TRL < max) this.TRL = 0;
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
            else if ((rightArm.Y < -0.8 && leftArm.Y < -0.8))
                return false;

            return true;
        }

        private void setUD(BodyPartData rightArm, BodyPartData leftArm)
        {
            float tempUD = (rightArm.Y + leftArm.Y) / 2;
            int sign = tempUD > 0 ? 1 : -1;
            this.UD = sign * (float) Math.Round(Math.Max(tempUD * sign - WIGGLE_ROOM, 0) / (1 - WIGGLE_ROOM), 1);
        }

        private void setRL(BodyPartData rightArm, BodyPartData leftArm)
        {
            float tempRL = (leftArm.Y - rightArm.Y) / 2;
            int sign = tempRL > 0 ? 1 : -1;
            this.RL = sign * (float) Math.Round(Math.Max(tempRL * sign - WIGGLE_ROOM, 0) / (1 - WIGGLE_ROOM), 1);
        }

        private void setFB(BodyPartData rightArm, BodyPartData leftArm)
        {
            float tempFB = (rightArm.Z + leftArm.Z) / 2;
            int sign = tempFB > 0 ? 1 : -1;
            this.FB = sign * (float) Math.Round(Math.Max(tempFB * sign - WIGGLE_ROOM, 0) / (1 - WIGGLE_ROOM), 1);
        }

        private void setTRL(BodyPartData rightArm, BodyPartData leftArm)
        {
            float tempTRL = (rightArm.Z - leftArm.Z) / 2;
            int sign = tempTRL > 0 ? 1 : -1;
            this.TRL = sign * (float) Math.Round(Math.Max(tempTRL * sign - WIGGLE_ROOM, 0) / (1 - WIGGLE_ROOM), 1);
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
            //JointType.HandRight
        };

        private static readonly List<JointType> LeftArm = new List<JointType>(){
            JointType.ShoulderLeft,
            JointType.ElbowLeft,
            JointType.WristLeft,
            //JointType.HandLeft
        };

        public bool straight { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

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
                float tempDX = skel.Joints[joints[i]].Position.X - skel.Joints[joints[i - 1]].Position.X;
                float tempDY = skel.Joints[joints[i]].Position.Y - skel.Joints[joints[i - 1]].Position.Y;
                float tempDZ = skel.Joints[joints[i]].Position.Z - skel.Joints[joints[i - 1]].Position.Z;
                float tempL = (float) Math.Pow(Math.Pow(tempDX, 2) + Math.Pow(tempDY, 2) + Math.Pow(tempDZ, 2), 0.5);

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

            // Adjust to more comfortable positions of the human body.
            this.Y += 0.05f;
            this.Z = (this.Z + 0.15f) / 0.8f;

            float length = (float)Math.Pow(Math.Pow(this.X, 2) + Math.Pow(this.Y, 2) + Math.Pow(this.Z, 2), 0.5);
            this.X /= length;
            this.Y /= length;
            this.Z /= length;

            this.straight = angleError < Command.ANGLE_ERROR_TOLERANCE;
        }
    }
}