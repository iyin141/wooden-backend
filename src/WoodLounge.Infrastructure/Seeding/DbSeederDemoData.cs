using Microsoft.EntityFrameworkCore;
using WoodLounge.Domain.Catalog;
using WoodLounge.Domain.CustomOrders;
using WoodLounge.Domain.Identity;
using WoodLounge.Domain.Messaging;
using WoodLounge.Domain.Orders;
using WoodLounge.Domain.Payments;
using WoodLounge.Domain.Visits;

namespace WoodLounge.Infrastructure.Seeding;

/// <summary>
/// Development-only demo data (spec §4: "Demo customers, visits, custom requests with message
/// threads, quotes, orders and payments in each status so every admin screen renders realistic
/// data in Development"). Idempotent: skipped entirely if any demo customer already exists.
/// </summary>
public partial class DbSeeder
{
    private const string DemoPhonePrefix = "+23480300";

    private async Task SeedDemoDataAsync(Dictionary<string, Product> products, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Phone != null && u.Phone.StartsWith(DemoPhonePrefix), ct))
            return;

        var admin = await db.Users.FirstOrDefaultAsync(u => u.Role == "super_admin", ct);
        var enugu = await db.Showrooms.FirstOrDefaultAsync(s => s.Slug == "enugu", ct);

        // ---- demo customers ----
        var customers = new (string Name, string Phone, string Email)[]
        {
            ("Chidiebere Okonkwo", DemoPhonePrefix + "00001", "chidiebere@example.test"),
            ("Ngozi Adeleke", DemoPhonePrefix + "00002", "ngozi@example.test"),
            ("Farouk Danjuma", DemoPhonePrefix + "00003", "farouk@example.test"),
            ("Ibe Kalu", DemoPhonePrefix + "00004", "ibe@example.test"),
            ("Amara Nwosu", DemoPhonePrefix + "00005", "amara@example.test"),
        };
        var userByPhone = new Dictionary<string, User>();
        foreach (var (name, phone, email) in customers)
        {
            var u = new User
            {
                FullName = name,
                Phone = phone,
                Email = email,
                Role = "customer",
                PhoneVerifiedAt = DateTimeOffset.UtcNow,
                IsActive = true,
            };
            db.Users.Add(u);
            userByPhone[phone] = u;
        }
        await db.SaveChangesAsync(ct); // assign ids

        var chidiebere = userByPhone[DemoPhonePrefix + "00001"];
        var ngozi = userByPhone[DemoPhonePrefix + "00002"];
        var farouk = userByPhone[DemoPhonePrefix + "00003"];

        // ---- site visit bookings across statuses (with threads) ----
        await SeedDemoVisitsAsync(chidiebere, ngozi, farouk, enugu, admin, ct);

        // ---- custom requests across statuses (with threads + quotes) ----
        var quotedRequest = await SeedDemoCustomRequestsAsync(chidiebere, farouk, products, admin, ct);

        // ---- orders + payments across both status chains ----
        await SeedDemoOrdersAsync(chidiebere, ngozi, farouk, products, quotedRequest, admin, ct);

        await db.SaveChangesAsync(ct);
    }

    private async Task SeedDemoVisitsAsync(
        User chidiebere, User ngozi, User farouk, Showroom? enugu, User? admin, CancellationToken ct)
    {
        var visits = new (User User, string Ref, string Type, string Status, string Name, string Phone)[]
        {
            (chidiebere, "VB-20001", "site_inspection", "pending", "Chidiebere Okonkwo", "+2348030000001"),
            (ngozi, "VB-20002", "showroom_visit", "confirmed", "Ngozi Adeleke", "+2348030000002"),
            (farouk, "VB-20003", "online_visit", "reschedule_proposed", "Farouk Danjuma", "+2348030000003"),
        };

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var (user, refNo, type, status, name, phone) in visits)
        {
            var booking = new SiteVisitBooking
            {
                Reference = refNo,
                UserId = user.Id,
                VisitType = type,
                ShowroomId = type == "showroom_visit" ? enugu?.Id : null,
                RequestedDate = today.AddDays(3),
                RequestedTime = new TimeOnly(14, 0),
                ContactName = name,
                ContactPhone = phone,
                ContactEmail = user.Email,
                SiteAddress = type == "site_inspection" ? "Plot 14 Independence Layout, Enugu" : null,
                SiteCity = type == "site_inspection" ? "Enugu" : null,
                SiteState = type == "site_inspection" ? "Enugu" : null,
                ProjectNotes = "Full living-room fit-out; would like a site survey before quoting.",
                Status = status,
                ConfirmedDate = status == "confirmed" ? today.AddDays(3) : null,
                ConfirmedTime = status == "confirmed" ? new TimeOnly(14, 0) : null,
                ProposedDate = status == "reschedule_proposed" ? today.AddDays(5) : null,
                ProposedTime = status == "reschedule_proposed" ? new TimeOnly(11, 0) : null,
                MeetingLink = type == "online_visit" ? "https://meet.woodlounge.test/demo" : null,
                HandledById = admin?.Id,
            };
            db.SiteVisitBookings.Add(booking);
            await db.SaveChangesAsync(ct);

            var thread = new MessageThread
            {
                Kind = "site_visit",
                SiteVisitId = booking.Id,
                UserId = user.Id,
                LastMessageAt = DateTimeOffset.UtcNow,
                CustomerUnreadCount = status == "reschedule_proposed" ? 1 : 0,
            };
            db.MessageThreads.Add(thread);
            await db.SaveChangesAsync(ct);

            db.Messages.Add(new Message
            {
                ThreadId = thread.Id,
                SenderUserId = user.Id,
                SenderRole = "customer",
                Kind = "text",
                Body = "Hi, I'd like to book this visit. Please confirm availability.",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),
            });
            if (status != "pending" && admin is not null)
            {
                db.Messages.Add(new Message
                {
                    ThreadId = thread.Id,
                    SenderUserId = admin.Id,
                    SenderRole = "admin",
                    Kind = status == "reschedule_proposed" ? "status_change" : "text",
                    Body = status == "reschedule_proposed"
                        ? "We'd like to propose a new time for your visit."
                        : "Your visit is confirmed. We look forward to seeing you.",
                    CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
                });
            }
        }
        await db.SaveChangesAsync(ct);
    }

    private async Task<CustomRequest?> SeedDemoCustomRequestsAsync(
        User chidiebere, User farouk, Dictionary<string, Product> products, User? admin, CancellationToken ct)
    {
        CustomRequest? quoted = null;

        // 1) Quoted request with a thread + a sent quote
        var req1 = new CustomRequest
        {
            Reference = "CR-30001",
            UserId = chidiebere.Id,
            Tier = "fully_bespoke",
            BaseProductId = products.TryGetValue("executive-wall-credenza", out var cred) ? cred.Id : null,
            Title = "Bespoke Executive Wall Credenza",
            Description = "Wall-spanning credenza in Cross River Walnut, smoked finish, brass + Blum hardware.",
            DimensionsText = "3200 × 550 × 780 mm",
            PreferredMaterials = "Cross River Walnut, smoked",
            BudgetMinKobo = 1_200_000,
            BudgetMaxKobo = 1_500_000,
            DeliveryCity = "Enugu",
            DeliveryState = "Enugu",
            ContactName = "Chidiebere Okonkwo",
            ContactPhone = "+2348030000001",
            ContactEmail = chidiebere.Email,
            Status = "quoted",
            AssignedAdminId = admin?.Id,
        };
        db.CustomRequests.Add(req1);
        await db.SaveChangesAsync(ct);
        quoted = req1;

        var thread1 = new MessageThread
        {
            Kind = "custom_request",
            CustomRequestId = req1.Id,
            UserId = chidiebere.Id,
            LastMessageAt = DateTimeOffset.UtcNow,
            CustomerUnreadCount = 1,
        };
        db.MessageThreads.Add(thread1);
        await db.SaveChangesAsync(ct);

        db.Messages.Add(new Message
        {
            ThreadId = thread1.Id, SenderUserId = chidiebere.Id, SenderRole = "customer", Kind = "text",
            Body = "Please see the attached DWG for the credenza. Walnut, smoked, brass pulls.",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
        });
        db.CustomRequestAttachments.Add(new CustomRequestAttachment
        {
            RequestId = req1.Id,
            FileKey = "demo/WL-DWG-04_Credenza_Rev3.dwg",
            FileName = "WL-DWG-04_Credenza_Rev3.dwg",
            ContentType = "application/acad",
            SizeBytes = 4_404_019,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
        });
        if (admin is not null)
        {
            db.Messages.Add(new Message
            {
                ThreadId = thread1.Id, SenderUserId = admin.Id, SenderRole = "admin", Kind = "text",
                Body = "Received — our master joiner is reviewing the drawings.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            });
        }

        // sent quote
        var quote1 = new Quote
        {
            CustomRequestId = req1.Id,
            Version = 1,
            AmountKobo = 1_250_000,
            DepositKobo = 400_000,
            TimelineWeeks = 5,
            Notes = "Scope: carcass in Cross River Walnut, smoked; brass pulls + Blum runners.",
            ValidUntil = DateTimeOffset.UtcNow.AddDays(21),
            Status = "sent",
            SentById = admin?.Id ?? chidiebere.Id,
            SentAt = DateTimeOffset.UtcNow.AddHours(-6),
        };
        db.Quotes.Add(quote1);
        await db.SaveChangesAsync(ct);

        db.Messages.Add(new Message
        {
            ThreadId = thread1.Id, SenderUserId = admin?.Id, SenderRole = "admin", Kind = "quote",
            Body = "We've sent a formal quote for your approval.", QuoteId = quote1.Id,
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-6),
        });

        // 2) A "submitted / more info needed" request
        var req2 = new CustomRequest
        {
            Reference = "CR-30004",
            UserId = farouk.Id,
            Tier = "modify_existing",
            Title = "Fluted Dining Table 10-Seater",
            Description = "Extend the fluted dining table to seat 10; matching bench.",
            DeliveryCity = "Abuja",
            DeliveryState = "Federal Capital Territory",
            ContactName = "Farouk Danjuma",
            ContactPhone = "+2348030000003",
            ContactEmail = farouk.Email,
            Status = "in_discussion",
            AssignedAdminId = admin?.Id,
        };
        db.CustomRequests.Add(req2);
        await db.SaveChangesAsync(ct);

        var thread2 = new MessageThread
        {
            Kind = "custom_request", CustomRequestId = req2.Id, UserId = farouk.Id,
            LastMessageAt = DateTimeOffset.UtcNow, AdminUnreadCount = 1,
        };
        db.MessageThreads.Add(thread2);
        await db.SaveChangesAsync(ct);
        db.Messages.Add(new Message
        {
            ThreadId = thread2.Id, SenderUserId = farouk.Id, SenderRole = "customer", Kind = "text",
            Body = "Can this be widened to seat 10 comfortably? What's the lead time?",
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-5),
        });

        await db.SaveChangesAsync(ct);
        return quoted;
    }

    private async Task SeedDemoOrdersAsync(
        User chidiebere, User ngozi, User farouk, Dictionary<string, Product> products,
        CustomRequest? quotedRequest, User? admin, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        // ---- Premade order: PAID (Nsukka + Asata) ----
        var premade = await CreateOrderAsync(
            chidiebere, "premade", OrderStatusTransitions.Paid, "Chidiebere Okonkwo",
            "chidiebere@example.test", "+2348030000001", "Enugu", now.AddDays(-2), ct);
        AddItem(premade, products["nsukka-curved-sofa"], 1, 229_000);
        AddItem(premade, products["asata-coffee-table"], 1, 229_000);
        FinalizeTotals(premade, deliveryFeeKobo: 15_000);
        premade.AmountPaidKobo = premade.TotalKobo;
        await db.SaveChangesAsync(ct);
        AddPayment(premade, "full", premade.TotalKobo, "success", "WL-PAY-DEMO-0001", 99201482, "card", now.AddDays(-2));
        AddStatusHistory(premade, "pending_payment", null, admin, now.AddDays(-2));
        AddStatusHistory(premade, "paid", "Payment confirmed via Paystack.", null, now.AddDays(-2));

        // ---- Premade order: PROCESSING ----
        var processing = await CreateOrderAsync(
            ngozi, "premade", OrderStatusTransitions.Processing, "Ngozi Adeleke",
            "ngozi@example.test", "+2348030000002", "Federal Capital Territory", now.AddDays(-4), ct);
        AddItem(processing, products["lobito-accent-chair"], 2, 285_000);
        FinalizeTotals(processing, deliveryFeeKobo: 15_000);
        processing.AmountPaidKobo = processing.TotalKobo;
        await db.SaveChangesAsync(ct);
        AddPayment(processing, "full", processing.TotalKobo, "success", "WL-PAY-DEMO-0002", 99182301, "bank_transfer", now.AddDays(-4));
        AddStatusHistory(processing, "paid", null, null, now.AddDays(-4));
        AddStatusHistory(processing, "processing", "In the workshop queue.", admin, now.AddDays(-3));

        // ---- Premade order: DELIVERED ----
        var delivered = await CreateOrderAsync(
            farouk, "premade", OrderStatusTransitions.Delivered, "Farouk Danjuma",
            "farouk@example.test", "+2348030000003", "Lagos", now.AddDays(-20), ct);
        AddItem(delivered, products["ogui-modular-sofa"], 1, 680_000);
        FinalizeTotals(delivered, deliveryFeeKobo: 35_000);
        delivered.AmountPaidKobo = delivered.TotalKobo;
        delivered.EstimatedDeliveryDate = DateOnly.FromDateTime(now.AddDays(-12).DateTime);
        await db.SaveChangesAsync(ct);
        AddPayment(delivered, "full", delivered.TotalKobo, "success", "WL-PAY-DEMO-0003", 99175411, "ussd", now.AddDays(-20));
        foreach (var s in new[] { "paid", "processing", "ready", "out_for_delivery", "delivered" })
            AddStatusHistory(delivered, s, null, admin, now.AddDays(-18));

        // ---- Custom order: DEPOSIT_PAID (from the quoted request) ----
        if (quotedRequest is not null)
        {
            var quote = await db.Quotes.FirstAsync(q => q.CustomRequestId == quotedRequest.Id, ct);
            var custom = await CreateOrderAsync(
                chidiebere, "custom", OrderStatusTransitions.DepositPaid, "Chidiebere Okonkwo",
                "chidiebere@example.test", "+2348030000001", "Enugu", now.AddDays(-1), ct,
                quotedRequest.Id, quote.Id);
            AddItem(custom, products["executive-wall-credenza"], 1, quote.AmountKobo);
            FinalizeTotals(custom, deliveryFeeKobo: 15_000);
            custom.AmountPaidKobo = quote.DepositKobo; // deposit only; balance outstanding
            await db.SaveChangesAsync(ct);
            AddPayment(custom, "deposit", quote.DepositKobo, "success", "WL-PAY-DEMO-0004", 99150824, "card", now.AddDays(-1));
            AddStatusHistory(custom, "quote_accepted", null, null, now.AddDays(-1));
            AddStatusHistory(custom, "deposit_paid", "Deposit received.", null, now.AddDays(-1));
        }

        // ---- Premade order: PENDING_PAYMENT (unpaid, for the admin "orders to process" tile) ----
        var pending = await CreateOrderAsync(
            ngozi, "premade", OrderStatusTransitions.PendingPayment, "Ngozi Adeleke",
            "ngozi@example.test", "+2348030000002", "Enugu", now.AddHours(-3), ct);
        AddItem(pending, products["zik-console"], 1, 195_000);
        FinalizeTotals(pending, deliveryFeeKobo: 15_000);
        await db.SaveChangesAsync(ct);
        AddPayment(pending, "full", pending.TotalKobo, "initialized", "WL-PAY-DEMO-0005", null, null, now.AddHours(-3));
        AddStatusHistory(pending, "pending_payment", null, null, now.AddHours(-3));

        await db.SaveChangesAsync(ct);
    }

    private async Task<Order> CreateOrderAsync(
        User user, string kind, string status, string customerName, string email, string phone,
        string state, DateTimeOffset createdAt, CancellationToken ct,
        Guid? customRequestId = null, Guid? quoteId = null)
    {
        var order = new Order
        {
            UserId = user.Id,
            CustomerName = customerName,
            Kind = kind,
            Status = status,
            ShipName = customerName,
            ShipPhone = phone,
            ShipLine1 = "160 Ogui Rd",
            ShipCity = state == "Federal Capital Territory" ? "Abuja" : state,
            ShipState = state,
            CustomRequestId = customRequestId,
            QuoteId = quoteId,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return order;
    }

    private static void AddItem(Order order, Product product, int qty, long unitPriceKobo)
    {
        order.Items.Add(new OrderItem
        {
            ProductId = product.Id,
            NameSnapshot = product.Name,
            SkuSnapshot = product.Sku,
            ImageUrlSnapshot = $"/images/products/{product.Slug}.webp",
            UnitPriceKobo = unitPriceKobo,
            Quantity = qty,
            SelectedOptionsJson = "[]",
            LineTotalKobo = unitPriceKobo * qty,
        });
    }

    private static void FinalizeTotals(Order order, long deliveryFeeKobo, long discountKobo = 0)
    {
        order.SubtotalKobo = order.Items.Sum(i => i.LineTotalKobo);
        order.DeliveryFeeKobo = deliveryFeeKobo;
        order.DiscountKobo = discountKobo;
        order.TotalKobo = order.SubtotalKobo + deliveryFeeKobo - discountKobo;
    }

    private void AddPayment(
        Order order, string kind, long amountKobo, string status, string reference,
        long? paystackTxnId, string? channel, DateTimeOffset at)
    {
        db.Payments.Add(new Payment
        {
            OrderId = order.Id,
            Kind = kind,
            Reference = reference,
            PaystackTransactionId = paystackTxnId,
            AmountKobo = amountKobo,
            Currency = "NGN",
            Status = status,
            Channel = channel,
            FeesKobo = status == "success" ? 2_000 : null,
            CustomerEmail = order.User?.Email ?? "guest@example.test",
            PaidAt = status == "success" ? at : null,
            CreatedAt = at,
            UpdatedAt = at,
        });
    }

    private void AddStatusHistory(Order order, string status, string? note, User? by, DateTimeOffset at)
    {
        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = status,
            Note = note,
            NotifyCustomer = true,
            ChangedById = by?.Id,
            CreatedAt = at,
        });
    }
}
