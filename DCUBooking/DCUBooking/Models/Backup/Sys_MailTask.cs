//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DCUBooking.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Sys_MailTask
    {
        public long ID { get; set; }
        public long RecordID { get; set; }
        public int RequestType { get; set; }
        public int EmailType { get; set; }
        public System.DateTime CreateTime { get; set; }
        public System.DateTime SendTime { get; set; }
        public byte Status { get; set; }
    }
}
