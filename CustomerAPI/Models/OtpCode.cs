using System;
using System.Collections.Generic;

namespace CustomerAPI.Models;

public partial class OtpCode
{
    public Guid OtpId { get; set; }

    public Guid? CustomerId { get; set; }

    public string? VerificationCode { get; set; }

    public DateTime? VerificationExpire { get; set; }

    public DateTime? LastOtpSentAt { get; set; }

    public int? OtpAttemptCount { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetExpire { get; set; }

    public virtual Customer? Customer { get; set; }
}
