namespace LegacyRenewalApp;

public interface ICustomerRepository
{
    Customer GetByID(int id);
}

public interface IPlanRepository
{
    SubscriptionPlan GetByCode(string code);
}