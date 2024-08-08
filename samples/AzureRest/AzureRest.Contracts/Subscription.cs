namespace AzureRest.Contracts
{
    public class Subscription
    {
        public string AuthorizationSource { get; set; }

        public string DisplayName { get; set; }

        public string Id { get; set; }

        public ManagedByTenant[] ManagedByTenants { get; set; }

        public string State { get; set; }

        public string SubscriptionId { get; set; }

        public SubscriptionPolicies SubscriptionPolicies { get; set; }

        public string TenantId { get; set; }
    }
}