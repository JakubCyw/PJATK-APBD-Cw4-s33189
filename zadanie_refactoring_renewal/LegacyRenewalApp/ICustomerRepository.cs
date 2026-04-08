namespace LegacyRenewalApp;

public interface ICustomerRepository
{
    Customer GetById(int customerId);
}

public interface IPlanRepository
{
    SubscriptionPlan GetByCode(string code);
}