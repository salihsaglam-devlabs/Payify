using System;


namespace LinkPara.SharedModels.BusModels.Commands.Scheduler
{
    public class SendIysPermission
    {
        public Guid UserId { get; set; }
        public bool IsIysApproved { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsCorporate { get; set; }
        public string Channel { get; set; }
    }
}
