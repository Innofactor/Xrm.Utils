using System.Collections.Generic;

namespace Innofactor.Xrm.Utils.Common.Misc
{
    /// <summary>General constants used Xrm Utils code</summary>
    public static class Constants
    {
        /// <summary>Entities that do not have attribute StateCode</summary>
        public static readonly List<string> StatecodelessEntities = new List<string>(new string[] {
            "activityparty",
            "activitymimeattachment",
            "annotation",
            "annualfiscalcalendar",
            "attachment",
            "customeraddress",
            "invoicedetail",
            "listmember",
            "notification",
            "opportunityproduct",
            "post",
            "postcomment",
            "postfollow",
            "postlike",
            "quotedetail",
            "report",
            "resource",
            "role",
            "salesorderdetail",
            "site",
            "subject",
            "systemuser",
            "team"
        });
    }
}