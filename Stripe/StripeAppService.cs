using Stripe;
using Stripe.Checkout;

namespace backend.Stripe;

public class StripeAppService : IStripeAppService { 
    public Account CreateExpressAccount(string email)
    {
        AccountCreateOptions options = new AccountCreateOptions { Type = "express", Country = "NL", Email = email, Capabilities = new AccountCapabilitiesOptions {
            CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
            IdealPayments = new AccountCapabilitiesIdealPaymentsOptions { Requested = true },
            Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
        }, BusinessType = "individual" };
        AccountService service = new AccountService();
        return service.Create(options);
    }
    public Account GetAccount(string id) {
        AccountService service = new AccountService();
        return service.Get(id);
    }
    public AccountLink CreateAccountLink(string stripeId, string userId, string token) {
        AccountLinkCreateOptions options = new AccountLinkCreateOptions {
            
            Account = stripeId,
            RefreshUrl = "https://presale.discount/onboard-refresh",
            ReturnUrl = string.Format("https://presale.discount/onboard-complete/{0}/{1}", userId, token),
            Type = "account_onboarding"
        };
        AccountLinkService service = new AccountLinkService();
        return service.Create(options);
    }
    public Session CreateAccountPayment(string userId, string token) {
        SessionCreateOptions options = new SessionCreateOptions {
            LineItems = new List<SessionLineItemOptions> {
                new SessionLineItemOptions {
                    PriceData = new SessionLineItemPriceDataOptions {
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions {
                            Name = "Registratiekosten"
                        },
                        UnitAmount = 2500
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = string.Format("https://thuisverzorgers.nl/confirm-careconsumer/{0}/{1}", userId, token),
            CancelUrl =  string.Format("https://thuisverzorgers.nl/register-pay/{0}/{1}", userId, token)
        };
        SessionService service = new SessionService();
        return service.Create(options);
    }
    public Session CreatePaymentRequest(int price, string destination, string id, int cmid) {
        SessionCreateOptions options = new SessionCreateOptions {
            LineItems = new List<SessionLineItemOptions> {
                new SessionLineItemOptions {
                    PriceData = new SessionLineItemPriceDataOptions {
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions {
                            Name = "Vooruitbetaling aan verzorger"
                        },
                        UnitAmount = price * 100
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            PaymentIntentData = new SessionPaymentIntentDataOptions {
                ApplicationFeeAmount = 100,
                TransferData = new SessionPaymentIntentDataTransferDataOptions {
                    Destination = destination
                }
            },
            SuccessUrl = "https://thuisverzorgers.nl/payment-success/" + id + '/' + cmid,
            CancelUrl = "https://thuisverzorgers.nl/cancel"
        };
        SessionService service = new SessionService();
        return service.Create(options);
    }
    public Session GetSession(string id) {
        SessionService service = new SessionService();
        return service.Get(id);
    }
}
