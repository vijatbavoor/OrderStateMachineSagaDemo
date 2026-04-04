using MassTransit;
using OrderStateMachineSagaDemo.Contracts;
using OrderStateMachineSagaDemo.Models;
using Xunit;
using System;

namespace OrderStateMachineSagaDemo.IntegrationTests;

public class UnhappyPathTests : SagaTestBase
{
    // ─────────────────────────────────────────────────────────────────────────
    // Existing: 3 consecutive failures → Cancelled (via PaymentPending cancel branch)
    // Covers: StockChecked→PaymentPending (lines 72-74), PaymentPending→Cancelled (lines 101-108)
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task PaymentFailureMaxAttempts_CancelsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Start saga
        await PublishEvent(new MockOrderCreated { OrderId = orderId, CreatedAt = DateTime.UtcNow });
        await WaitForState(orderId, "Created");

        // Stock OK
        await PublishEvent(new MockStockChecked { OrderId = orderId, StockAvailable = true, CheckedAt = DateTime.UtcNow });
        await WaitForState(orderId, "StockChecked");

        // Trigger 3 payment failures to hit max attempts
        for (int attempt = 1; attempt <= 2; attempt++)
        {
            await PublishEvent(new MockPaymentFailed { OrderId = orderId, Reason = $"Payment fail #{attempt}", FailedAt = DateTime.UtcNow });
            await Task.Delay(1000); // allow state update
        }
        await PublishEvent(new MockPaymentFailed { OrderId = orderId, Reason = "Payment fail #3", FailedAt = DateTime.UtcNow });

        // Assert: saga cancelled after 3rd failure
        await WaitForState(orderId, "Cancelled", 15000);
        var saga = await GetSagaState(orderId);
        Assert.Equal(3, saga!.PaymentAttempts);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // New test 1: 2 failures then payment succeeds → order completes to Delivered
    // Covers: StockChecked→PaymentPending retry (lines 72-74)
    //         PaymentPending→PaymentPending retry (lines 97-99)
    //         PaymentPending→PaymentSucceeded→PaymentCompleted (lines 88-90)  ← previously uncovered
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task PaymentRetryThenSuccess_CompletesOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        await PublishEvent(new MockOrderCreated { OrderId = orderId, CreatedAt = DateTime.UtcNow });
        await WaitForState(orderId, "Created");

        await PublishEvent(new MockStockChecked { OrderId = orderId, StockAvailable = true, CheckedAt = DateTime.UtcNow });
        await WaitForState(orderId, "StockChecked");

        // Fail attempt 1 → StockChecked retry branch → PaymentPending (PaymentAttempts=1)
        await PublishEvent(new MockPaymentFailed { OrderId = orderId, Reason = "Insufficient funds #1", FailedAt = DateTime.UtcNow });
        await WaitForState(orderId, "PaymentPending");

        // Fail attempt 2 → PaymentPending retry branch → stays PaymentPending (PaymentAttempts=2)
        await PublishEvent(new MockPaymentFailed { OrderId = orderId, Reason = "Insufficient funds #2", FailedAt = DateTime.UtcNow });
        await Task.Delay(1000); // let state settle

        // Payment succeeds on 3rd try → PaymentPending succeed branch → PaymentCompleted
        await PublishEvent(new MockPaymentProcessed { OrderId = orderId, TransactionId = "txn-retry-ok", ProcessedAt = DateTime.UtcNow });
        await WaitForState(orderId, "PaymentCompleted");

        // Continue happy path to Delivered
        await PublishEvent(new MockAddressValidated { OrderId = orderId, Address = "42 Retry Road", ValidatedAt = DateTime.UtcNow });
        await WaitForState(orderId, "AddressValidated");

        await PublishEvent(new MockOrderShipped { OrderId = orderId, TrackingNumber = "TRK-RETRY-001", ShippedAt = DateTime.UtcNow });
        await WaitForState(orderId, "Shipped");

        await PublishEvent(new MockOrderDelivered { OrderId = orderId, DeliveredAt = DateTime.UtcNow });
        await WaitForState(orderId, "Delivered");

        // Assert
        var saga = await GetSagaState(orderId);
        Assert.NotNull(saga);
        Assert.Equal("Delivered", saga!.CurrentState);
        Assert.Equal(2, saga.PaymentAttempts);   // 2 failures before success
        Assert.Equal("42 Retry Road", saga.Address);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // New test 2: 3 rapid failures all received while still in StockChecked state
    // Covers: StockChecked→PaymentFailed cancel branch (lines 76-83) ← previously uncovered
    //         This branch only fires when PaymentAttempts reaches 3 while still in StockChecked.
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task PaymentFailure_DirectCancel_FromStockChecked_CancelsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        await PublishEvent(new MockOrderCreated { OrderId = orderId, CreatedAt = DateTime.UtcNow });
        await WaitForState(orderId, "Created");

        await PublishEvent(new MockStockChecked { OrderId = orderId, StockAvailable = true, CheckedAt = DateTime.UtcNow });
        await WaitForState(orderId, "StockChecked");

        // Fire all 3 failures rapidly without waiting for state transitions.
        // MassTransit in-memory bus processes messages sequentially per saga, so each
        // failure increments PaymentAttempts within StockChecked until ≥ 3 triggers Cancelled.
        await PublishEvent(new MockPaymentFailed { OrderId = orderId, Reason = "Rapid fail #1", FailedAt = DateTime.UtcNow });
        await PublishEvent(new MockPaymentFailed { OrderId = orderId, Reason = "Rapid fail #2", FailedAt = DateTime.UtcNow });
        await PublishEvent(new MockPaymentFailed { OrderId = orderId, Reason = "Rapid fail #3", FailedAt = DateTime.UtcNow });

        // Assert: saga must reach Cancelled with exactly 3 attempts recorded
        await WaitForState(orderId, "Cancelled", 15000);
        var saga = await GetSagaState(orderId);
        Assert.NotNull(saga);
        Assert.Equal("Cancelled", saga!.CurrentState);
        Assert.Equal(3, saga.PaymentAttempts);
    }
}




class MockPaymentFailed : IPaymentFailed
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime FailedAt { get; set; }
}

