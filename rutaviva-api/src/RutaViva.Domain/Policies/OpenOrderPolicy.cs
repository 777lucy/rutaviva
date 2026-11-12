namespace RutaViva.Domain.Policies;

public static class OpenOrderPolicy
{
    public static bool CanOpen(bool hasOpenOrder) => !hasOpenOrder;
}
