using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Serialization;

namespace Utilities.Type
{
    [XmlRoot("SchedulingRentalMaster")]
    public class SchedulingRentalMaster
    {
        /// <summary>
        /// DateTime should be the delivery time
        /// </summary>
        public RentalContainers UndeliveredRentals { get; set; }


        /// <summary>
        /// DateTime should be the delivery time
        /// </summary>
        public RentalContainers DeliveriesInProgress { get; set; }

        /// <summary>
        /// DateTime should be the completion time
        /// </summary>
        public RentalContainers DeliveredRentals { get; set; }

        public SchedulingRentalMaster()
        {
            UndeliveredRentals = new RentalContainers();
            DeliveriesInProgress = new RentalContainers();
            DeliveredRentals = new RentalContainers();
        }

        public bool NoRentalsRecorded()
        {
            return UndeliveredRentals.RentalContainerList.Count == 0 &&
                DeliveriesInProgress.RentalContainerList.Count == 0 &&
                DeliveredRentals.RentalContainerList.Count == 0;
        }
    }
}
