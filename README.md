# Order State Machine Saga Demo (.NET 8 + MassTransit)

[![Build](https://github.com/yourusername/OrderStateMachineSagaDemo/workflows/CI/badge.svg)](https://github.com/yourusername/OrderStateMachineSagaDemo/actions)

A production-ready demonstration of **MassTransit Saga State Machine** for resilient, event-driven order processing workflows. Features payment retry logic, inventory checks, and full lifecycle management with SQLite persistence.

## ✨ Features

- ✅ **Saga State Machine**: Full order lifecycle (`Created` → `StockChecked` → `PaymentCompleted` → `Delivered` | `Cancelled`)
- ✅ **Payment Retry Policy**: Configurable retries (max 3 attempts) before cancellation
- ✅ **In-Memory/Integration Tests**: Happy path, unhappy path (stock failure, payment failure x3), auto-generated tests
- ✅ **Refactored Mocks**: Clean separation in `IntegrationTests/Mocks/` folder
- ✅ **SQLite Persistence**: Saga state durable storage (`sagas.db`)
- ✅ **RabbitMQ Ready**: Production message broker integration
- ✅ **Dependency Injection**: Clean service boundaries via interfaces

## 🏗️ Architecture Diagram

```
OrderCreated ──(Stock ✓)──> StockChecked ──(Payment ✓)──> PaymentCompleted ──> Delivered
    │                           │
    │                      (Payment ✗) ──[Retry ≤3]──> PaymentPending ──[Retry >3]──> Cancelled
    │
(Stock ✗)
    ↓
Cancelled
```

## 🚀 Quick Start

### Prerequisites
```bash
# .NET 8 SDK
winget install Microsoft.DotNet.SDK.8

# RabbitMQ (optional for production)
winget install RabbitMQ.RabbitMQ
```

### Clone & Run
```bash
git clone <repo> OrderStateMachineSagaDemo
cd OrderStateMachineSagaDemo
dotnet restore
dotnet build
dotnet run --project OrderStateMachineSagaDemo
```

**Database**: `sagas.db` auto-created in bin/Debug/net8.0

### Run Tests
```bash
# All tests
dotnet test OrderStateMachineSagaDemo.IntegrationTests

# Specific test
dotnet test OrderStateMachineSagaDemo.IntegrationTests --filter HappyPath_OrderDelivered
```

## 📁 Project Structure

```
OrderStateMachineSagaDemo/
├── OrderStateMachineSagaDemo/           # Main app
│   ├── StateMachines/OrderStateMachine.cs
│   ├── Services/PaymentRetryPolicy.cs   # Retry logic
│   └── Data/AppDbContext.cs            # EF Core + SQLite
├── OrderStateMachineSagaDemo.IntegrationTests/
│   ├── HappyPathTests.cs              # Manual happy path
│   ├── UnhappyPathTests.cs            # Payment retry tests
│   ├── Mocks/                        # ✅ Refactored mock events
│   │   ├── MockOrderCreated.cs
│   │   └── MockPaymentFailed.cs
│   └── SagaTestBase.cs               # Test infrastructure
└── README.md
```

## 🧪 Test Coverage

| Scenario | Test File | Status |
|----------|-----------|--------|
| Order Delivered (Happy) | `HappyPathTests.HappyPath_OrderDelivered` | ✅ PASS |
| Max Payment Failures → Cancel | `UnhappyPathTests.PaymentFailureMaxAttempts_CancelsOrder` | ✅ PASS |
| Payment Retry Success | `UnhappyPathTests.PaymentRetryThenSuccess_CompletesOrder` | ✅ PASS |
| Stock Unavailable | `AutoUnhappyPathTests.CancelsOrder_When_StockIsNotAvailable` | ✅ PASS |
| Rapid Payment Failures | `AutoUnhappyPathTests.CancelsOrder_When_PaymentFails_For_3_Times` | ✅ PASS |

## 🔧 Recent Improvements

1. **Mock Class Extraction** 🎉: All inline mocks moved to dedicated `Mocks/` folder
2. **Consistent Namespaces**: `OrderStateMachineSagaDemo.IntegrationTests.Mocks`
3. **Fixed Cross-File References**: Updated all test files with proper `using` directives
4. **Test Infrastructure**: `SagaTestBase` provides `PublishEvent`, `WaitForState`, `GetSagaState`

## 🤝 Contributing

1. Fork & clone
2. `dotnet restore && dotnet build`
3. Add tests for new scenarios
4. Submit PR with `HappyPath_OrderDelivered` still passing ✅

## 📄 License

MIT - See [LICENSE](LICENSE) file.

---

⭐ **Star if useful!** Questions? Open an issue.
