using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
namespace MrCMS.Web.Apps.Core.Utility
{
    [Serializable]
    public class DdListItem
    {
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
    }
    public enum MaritalStatus
    {
        Single = 0,
        Married = 1,
        Divorced = 2,
    }

    public enum ClaimsBillStatus
    {
        Captured = 0,
        Vetted = 1,
        ReVetted = 2,
    }


    public enum evsvisittype
    {
        All = 0,
        NewVisit = 1,
        FollowUp = 2
    }
    public enum ClaimsTAGS
    {
        OP = 0,
        MA = 1,
        AD = 2,
        SR = 3,
        AI = 4,
        CD = 5,
        OOP = 6,
        DOP = 7,
        CS = 8,
        ND = 9,
        ANC = 10,
        Unknown = 11,

    }

    public enum ClaimBatch
    {
        All = -1,
        BatchA = 0,
        BatchB = 1,

    }
    public enum ClaimBatchStatus
    {
        Default = 0,
        Capturing = 1,
        Vetting = 2,
        Reviewing = 3,
        AwaitingApproval = 4,
        AwaitingPayment = 5,
        Paid = 6,
        Disapproved = 7,
        //this was added because of the claim batch analysis
        ReviewingANDAwaitingApproval = 8,
        All = 9,


    }
    public enum EnrolleesStatus
    {
        Inactive = 0,
        Active = 1,

    }
    public enum SubscriptionType
    {

    }
    public enum Sponsorshiptype
    {
        [Display(Name = "Organization / Company")]
        Company = 0,
        [Display(Name = "Group / Union")]
        Group = 1,
        [Display(Name = "Other")]
        Other = 2


    }
    public enum Sex
    {
        Male = 0,
        Female = 1
    }
    public enum PaymentStatus
    {
        Default = 0,
        Pending = 1,
        Paying = 2,
        Paid = 3



    }

    public enum PaymentMethod
    {
        none = 0,
        [Display(Name = "Cash Deposit")]
        cashdeposit = 1,
        [Display(Name = "Bank Transfer")]
        banktransfer = 2,
        [Display(Name = "Cheque")]
        cheque = 3,
        [Display(Name = "Other")]
        other = 4,


    }

    public enum IssuesStatus
    {
        Pending = 0,
        Resolving = 1,
        Resolved = 2,
        Closed = 3
    }
    public enum priority
    {
        normal = 0,
        medium = 1,
        urgent = 2

    }
    public enum NotificationType
    {
        Transient = 0,
        Persistent = 1,
        Email = 1,
    };
    public enum NotificationTarget
    {
        User = 0,
        Role = 1
    };
    public enum AuthorizationStatus
    {
        Default = 0,
        Pending = 1,
        Authorized = 2,
        Disapproved = 3
    };
    public enum SubscriptionStatus
    {
        Default = 0,
        Active = 1,
        Terminated = 2,
        Expired = 3
    };

    public enum Relationship
    {
        Spouse = 0,
        Child = 1,
        Mother = 2,
        Father = 3,
        Brother = 4,
        Sister = 5,
    };

    public enum ProviderCategory
    {
        Primary = 0,
        Secondary = 1,
        Tertiary = 2

    }

    public enum admissionStatus
    {
        Admitted = 0,
        Discharged = 1,

    };
    public enum SubscriptionDuration
    {
        [Display(Name = "3 Months")]
        Month3 = 0,
        [Display(Name = "6 Months")]
        Month6 = 1,
        [Display(Name = "1 year")]
        Month12

    }

    public enum SmsStatus
    {
        Pending = 0,
        Delivered = 1,
        Failed = 2,
        Cancelled = 3

    }
    public enum SmsType
    {
        Quick = 0,
        Birthday = 1,
        Verification = 2,
        Signup = 2,
        Others = 3

    }

    public enum EnrolleeVerificationCodeStatus
    {
        Pending = 0,
        Authenticated = 1,
        Expired = 3

    }

    public enum SmsSend
    {
        All = 0,
        ActiveSubscribersOnly = 1


    }


    public enum ConnectCareStatus
    {
        Pending = 0,
        Verified = 1,
        Rejected = 2,

    }

    public enum ChannelType
    {
        ShortCode = 0,
        MobileApp = 1,
        Web = 2

    }

    public enum PurposeOfVisit
    {
        Unknown = 0,
        Consultation = 1,
        Optical = 2,
        Dental = 3

    }

    public enum JobStatus
    {
        Uploaded = 0,
        Started = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4

    }

    public enum JobExpungeMode
    {
        Renewal = 0,
        Additions = 1,


    }

    public enum SubscriptionMode
    {
        Company = 0,
        Subsidiary = 1,
    }

    public enum ReceivedClaimStatus
    {

        Received = 0,
        Capturing = 1,
        Completed = 2

    }

    public enum SMSLeads
    {
        [Display(Name = "Principal and Dependents")]
        principalandDependent = 0,
        [Display(Name = "Principal online")]
        OnlyPrincipal = 1,


    }
}