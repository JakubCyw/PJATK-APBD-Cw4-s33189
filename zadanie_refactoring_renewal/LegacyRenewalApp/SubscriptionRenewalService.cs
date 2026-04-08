using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IBillingService _billingService;
        private readonly InvoiceCalculator _invoiceCalculator;

        public SubscriptionRenewalService() : this(
            new CustomerRepository(),
            new SubscriptionPlanRepository(),
            new LegacyBillingServiceWrapper(),
            new InvoiceCalculator())
        {
        }

        public SubscriptionRenewalService(
            ICustomerRepository customerRepository,
            IPlanRepository planRepository,
            IBillingService billingService,
            InvoiceCalculator invoiceCalculator)
        {
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _billingService = billingService;
            _invoiceCalculator = invoiceCalculator;
        }
        
        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {

            ValidateRequest(customerId, planCode, seatCount, paymentMethod);
            
            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            var invoice = _invoiceCalculator.Calculate(
                customer,
                plan,
                seatCount,
                paymentMethod,
                includePremiumSupport,
                useLoyaltyPoints);
            
            _billingService.SaveInvoice(invoice);
            NotifyCustomer(customer, invoice);
            
            return invoice;
        }

        private void ValidateRequest(int customerId, string planCode, int seatCount, string paymentMethod)
        {
            if (customerId <= 0) throw new ArgumentException("Customer id must be positive");
            if (string.IsNullOrWhiteSpace(planCode)) throw new ArgumentException("Plan Code is required");
            if (seatCount <= 0) throw new ArgumentException("Seat count must be positive");
            if (string.IsNullOrWhiteSpace(paymentMethod)) throw new ArgumentException("Payment Method is required");
        }

        private void NotifyCustomer(Customer customer, RenewalInvoice invoice)
        {
            if (string.IsNullOrWhiteSpace(customer.Email)) return;

            string subject = "Subscription renewal invoice";
            string body = $"Hello {customer.FullName}, your renewal for plan {invoice.PlanCode} " +
                          $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";
            
            _billingService.SendEmail(customer.Email, subject, body);
        }
    }
}
