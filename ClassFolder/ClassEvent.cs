﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventApp.ClassFolder
{
    internal class ClassEvent
    {
        public int IdEvents { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime EndDate { get; set; }
        public int LocationId { get; set; }
        public int OrganizerId { get; set; }
        public int EventTrainerID { get; set; }
        public int EventID { get; set; }
        public int TrainerID { get; set; }
        public int UserId { get; set; }

    }
}
