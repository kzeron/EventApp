//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EventApp.DataFolder
{
    using System;
    using System.Collections.Generic;
    
    public partial class Notifications
    {
        public int IdNotification { get; set; }
        public Nullable<int> IdEmployee { get; set; }
        public Nullable<int> EventId { get; set; }
        public Nullable<int> MessageType { get; set; }
        public string Message { get; set; }
        public Nullable<System.DateTime> SentDate { get; set; }
    
        public virtual Employee Employee { get; set; }
    }
}
