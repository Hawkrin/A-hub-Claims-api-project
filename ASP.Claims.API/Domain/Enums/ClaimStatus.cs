namespace ASP.Claims.API.Domain.Enums;

[Flags]
public enum ClaimStatus
{
    //[Display(ResourceType = typeof(ResourceStrings), Name = "ClaimStatus_None_SE")]
    None = 0,

    //[Display(ResourceType = typeof(ResourceStrings), Name = "ClaimStatus_RequiresManualReview_SE")]
    RequiresManualReview = 1 << 0,

    //[Display(ResourceType = typeof(ResourceStrings), Name = "ClaimStatus_Escalated_SE")]
    Escalated = 1 << 1,

    //[Display(ResourceType = typeof(ResourceStrings), Name = "ClaimStatus_FraudCheck_SE")]
    FraudCheck = 1 << 2
}
