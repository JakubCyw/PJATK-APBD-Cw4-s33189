namespace LegacyRenewalApp;

public interface IBillingService
{
    void SaveInvoice(RenewalInvoice invoice);
    void SendEmail(string email, string subject, string body);
}

public class LegacyBillingServiceWrapper : IBillingService
{
    public void  SaveInvoice(RenewalInvoice invoice) => LegacyBillingGateway.SaveInvoice(invoice);
    public void SendEmail(string email, string subject, string body) => LegacyBillingGateway.SendEmail(email, subject, body);
}