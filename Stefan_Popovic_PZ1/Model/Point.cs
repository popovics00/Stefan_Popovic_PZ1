﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stefan_Popovic_PZ1.Model
{
    public class Point
    {
        private double x;
        private double y;

        public Point()
        {

        }

        public double X
        {
            get
            {
                return x;
            }

            set
            {
                x = value;
            }
        }

        public double Y
        {
            get
            {
                return y;
            }

            set
            {
                y = value;
            }
        }
    }
}