using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Serialization;

namespace Utilities.Type
{
    public class RentalContainer : IComparable<RentalInformation>
    {
        public DateTime DeliveryTime { get; set; }

        public RentalInformation RentalInfo { get; set; }

        public RentalContainer(DateTime deliveryTime, RentalInformation rentalInfo)
        {
            DeliveryTime = deliveryTime;
            RentalInfo = rentalInfo;
        }

        public RentalContainer()
        {
        }

        public int CompareTo(RentalInformation other)
        {
            return DeliveryTime.CompareTo(other.DeliveryTime);
        }
    }
}
