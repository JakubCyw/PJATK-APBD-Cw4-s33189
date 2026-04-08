using System;

namespace LegacyRenewalApp;

public class InvoiceCalculator
{
        public RenewalInvoice Calculate(
            Customer customer, 
            SubscriptionPlan plan, 
            int seatCount, 
            string paymentMethod, 
            bool includePremiumSupport, 
            bool useLoyaltyPoints)
        {
            string normalizedPlanCode = plan.Code;
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            // --- LOGIKA BAZOWA ---
            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            decimal discountAmount = 0m;
            string notes = string.Empty;

            // --- LOGIKA ZNIŻEK ---
            if (customer.Segment == "Silver")
            {
                discountAmount += baseAmount * 0.05m;
                notes += "silver discount; ";
            }
            else if (customer.Segment == "Gold")
            {
                discountAmount += baseAmount * 0.10m;
                notes += "gold discount; ";
            }
            else if (customer.Segment == "Platinum")
            {
                discountAmount += baseAmount * 0.15m;
                notes += "platinum discount; ";
            }
            else if (customer.Segment == "Education" && plan.IsEducationEligible)
            {
                discountAmount += baseAmount * 0.20m;
                notes += "education discount; ";
            }

            if (customer.YearsWithCompany >= 5)
            {
                discountAmount += baseAmount * 0.07m;
                notes += "long-term loyalty discount; ";
            }
            else if (customer.YearsWithCompany >= 2)
            {
                discountAmount += baseAmount * 0.03m;
                notes += "basic loyalty discount; ";
            }

            if (seatCount >= 50)
            {
                discountAmount += baseAmount * 0.12m;
                notes += "large team discount; ";
            }
            else if (seatCount >= 20)
            {
                discountAmount += baseAmount * 0.08m;
                notes += "medium team discount; ";
            }
            else if (seatCount >= 10)
            {
                discountAmount += baseAmount * 0.04m;
                notes += "small team discount; ";
            }

            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                discountAmount += pointsToUse;
                notes += $"loyalty points used: {pointsToUse}; ";
            }

            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            // --- LOGIKA WSPARCIA PREMIUM ---
            decimal supportFee = 0m;
            if (includePremiumSupport)
            {
                if (normalizedPlanCode == "START") supportFee = 250m;
                else if (normalizedPlanCode == "PRO") supportFee = 400m;
                else if (normalizedPlanCode == "ENTERPRISE") supportFee = 700m;

                notes += "premium support included; ";
            }

            // --- LOGIKA OPŁAT PŁATNICZYCH ---
            decimal paymentFee = 0m;
            if (normalizedPaymentMethod == "CARD") paymentFee = (subtotalAfterDiscount + supportFee) * 0.02m;
            else if (normalizedPaymentMethod == "BANK_TRANSFER") paymentFee = (subtotalAfterDiscount + supportFee) * 0.01m;
            else if (normalizedPaymentMethod == "PAYPAL") paymentFee = (subtotalAfterDiscount + supportFee) * 0.035m;
            else if (normalizedPaymentMethod == "INVOICE") paymentFee = 0m;
            else throw new ArgumentException("Unsupported payment method");

            if (normalizedPaymentMethod != "INVOICE") notes += $"{normalizedPaymentMethod.ToLower()} fee; ";
            else notes += "invoice payment; ";

            // --- LOGIKA PODATKOWA ---
            decimal taxRate = customer.Country switch
            {
                "Poland" => 0.23m,
                "Germany" => 0.19m,
                "Czech Republic" => 0.21m,
                "Norway" => 0.25m,
                _ => 0.20m
            };

            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal taxAmount = taxBase * taxRate;
            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes += "minimum invoice amount applied; ";
            }

            // --- BUDOWANIE OBIEKTU ---
            return new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customer.Id}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };
        }
    }