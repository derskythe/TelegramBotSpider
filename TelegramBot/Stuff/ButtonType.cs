using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Stuff
{
    class ButtonType
    {
        public String Name { get; set; }
        public bool Location { get; set; }
        public bool PhoneNumber { get; set; }

        public ButtonType()
        {
        }

        public ButtonType(string name, bool location, bool phoneNumber)
        {
            Name = name;
            Location = location;
            PhoneNumber = phoneNumber;
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Location: {1}, PhoneNumber: {2}", Name, Location, PhoneNumber);
        }
    }
}
