using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicBot.Common.Models.MedicalAppoiment
{
    public class MedicalAppoimentModel
    {
        public string id { get; set; }
        public string idUser { get; set; }
        public DateTime date { get; set; }
        public int time { get; set; }
    }
}
