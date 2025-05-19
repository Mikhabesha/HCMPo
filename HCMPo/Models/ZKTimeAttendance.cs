using System;

namespace HCMPo.Models
{
    public class ZKTimeAttendance
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CheckTime { get; set; }
        public string CheckType { get; set; } // "I" for Check-In, "O" for Check-Out
        public string VerifyMode { get; set; } // Verification method (Fingerprint, Card, etc.)
        public string WorkCode { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
    }
} 