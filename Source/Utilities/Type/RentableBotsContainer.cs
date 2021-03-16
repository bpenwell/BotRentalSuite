using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Utilities.Type
{
    public class RentableBotsContainer
    {
        public List<BotNumberPair> RentableBots;

        public RentableBotsContainer()
        {
            RentableBots = new List<BotNumberPair>();
        }
    }
}
