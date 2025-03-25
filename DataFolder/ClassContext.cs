using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventApp.DataFolder
{
    public partial class EventEntities : DbContext
    {
        public static EventEntities context;

        public static EventEntities GetContext()
        {
            if (context == null)
            {
                context = new EventEntities();
            }
            return new EventEntities();
        }
    }
}
