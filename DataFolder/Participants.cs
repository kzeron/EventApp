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
    
    public partial class Participants
    {
        public int IdParticipants { get; set; }
        public Nullable<int> IdEvent { get; set; }
        public Nullable<int> IdUser { get; set; }
        public Nullable<int> IdStatus { get; set; }
        public Nullable<System.DateTime> RegistrationDate { get; set; }
    
        public virtual Events Events { get; set; }
        public virtual Status Status { get; set; }
        public virtual User User { get; set; }
    }
}
