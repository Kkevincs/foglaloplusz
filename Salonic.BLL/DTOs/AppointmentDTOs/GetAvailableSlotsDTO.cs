using System;
using System.Collections.Generic;

namespace Salonic.BLL.DTOs.AppointmentDTOs
{
    public class GetAvailableSlotsDTO
    {
        public int ArtistId { get; set; }
        public List<int> ServiceIds { get; set; } = new();
        public DateTime Date { get; set; }
    }
}

