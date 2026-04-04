# Order State Machine Saga Demo

This project demonstrates a robust, event-driven order processing workflow built with **.NET 8** and **MassTransit Saga State Machine**. It uses a distributed saga pattern to manage the lifecycle of an order across multiple states and external messages.

## 🚀 Overview

The application simulates a real-world order fulfillment process, including:
- **Order Initiation**: Capturing the initial order creation.
- **Inventory Check**: Transitioning based on stock availability.
- **Payment Processing**: Handling successful payments and implementing **retry logic** for failed attempts.
- **Fulfillment**: Moving through address validation, shipping, and finally, delivery.
- **Cancellation**: Gracefully handling failures (e.g., out of stock or max payment retries).

## 🏗️ Architecture

- **MassTransit**: Orchestrates the state machine and event messaging.
- **Entity Framework Core**: Persists the saga state in a SQLite database (`sagas.db`).
- **RabbitMQ**: Acts as the message broker for asynchronous event delivery.
- **Dependency Injection**: Services and policies are decoupled via interfaces (segregated into `Contracts/Services` and `Services` folders).

## 📊 State Machine Workflow

The `OrderStateMachine` manages the following lifecycle:

1.  **Initially**: `OrderCreated` → **Created**
2.  **During Created**: `StockCheckedEvent`
    - `StockAvailable` == true → **StockChecked**
    - `StockAvailable` == false → **Cancelled**
3.  **During StockChecked**:
    - `PaymentSucceeded` → **PaymentCompleted**
    - `PaymentFailed` → Increments attempts & transitions to **PaymentPending**
4.  **During PaymentPending** (Retry Path):
    - `PaymentSucceeded` → **PaymentCompleted**
    - `PaymentFailed`:
        - If attempts < `MaxAttempts` (defined in `IPaymentRetryPolicy`) → Stays in **PaymentPending** for retry.
        - If attempts >= `MaxAttempts` → **Cancelled**.
5.  **During PaymentCompleted**: `AddressValidatedEvent` → **AddressValidated**
6.  **During AddressValidated**: `OrderShipped` → **Shipped**
7.  **During Shipped**: `OrderDelivered` → **Delivered**
8.  **DuringAny**: `OrderCancelled` → **Cancelled**

## 🧩 Key Components

### State Machine
- **OrderStateMachine**: The core logic defining how events transition the `OrderState`.

### Services & Policies
- **IPaymentRetryPolicy**: Calculates if a retry should be allowed based on saga state.
- **IPaymentRetryHandler**: Encapsulates the actual logic for retry notifications or cancellation actions.
- **IOrderInitializService**: Handles the initial saga state assignment and logging.

### Contracts
- **Events**: Interfaces representing domain events (e.g., `IOrderCreated`, `IPaymentFailed`).

## 🛠️ Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [RabbitMQ Server](https://www.rabbitmq.com/download.html) (running locally at `localhost:5672`).

### Installation
1.  Clone the repository.
2.  Restore dependencies:
    ```bash
    dotnet restore
    ```
3.  Build the project:
    ```bash
    dotnet build
    ```

### Running the Application
The `Program.cs` is configured to run as a host, connecting to RabbitMQ and initializing the saga repository.
```bash
dotnet run --project OrderStateMachineSagaDemo/OrderStateMachineSagaDemo.csproj
```
*Note: The database (`sagas.db`) will be automatically created on startup.*

### Running Tests
A comprehensive set of integration tests covers both happy and unhappy paths (payment failures, retries, stock issues).
```bash
dotnet test OrderStateMachineSagaDemo.IntegrationTests/OrderStateMachineSagaDemo.IntegrationTests.csproj
```

## 📝 Testing Scenarios
- **Happy Path**: Successful order → stock check → payment → delivery.
- **Unhappy Path (Payment Failure)**: Retries payment until it succeeds or reaches the maximum 3 attempts before cancelling.
- **Stock Failure**: Immediate cancellation if inventory is unavailable.
- **Rapid Failures**: Verifies concurrency and state consistency under high-frequency failure events.
