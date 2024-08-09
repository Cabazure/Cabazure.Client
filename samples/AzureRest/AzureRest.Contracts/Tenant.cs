namespace AzureRest.Contracts
{
    public class Tenant
    {
        public string CountryCode { get; set; }

        public string DisplayName { get; set; }

        public string DefaultDomain { get; set; }

        public string[] Domains { get; set; }

        public string Id { get; set; }

        public string TenantCategory { get; set; }

        public string TenantId { get; set; }

        public string TenantType { get; set; }
    }
}
