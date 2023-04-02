using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stefan_Popovic_PZ1.Model
{
    public class SwitchEntity : PowerEntity
    {
        private string status;

        public string Status
        {
            get
            {
                return status;
            }

            set
            {
                status = value;
            }
        }
    }
}
