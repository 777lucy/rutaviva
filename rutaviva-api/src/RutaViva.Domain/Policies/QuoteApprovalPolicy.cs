using RutaViva.Domain.ValueObjects;

namespace RutaViva.Domain.Policies;

public static class QuoteApprovalPolicy
{
    public static bool RequiresManualApproval(Money amount) => amount.Amount > Money.ManualApprovalThreshold;
}
