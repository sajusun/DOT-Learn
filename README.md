# OrderPulse: Enterprise Clean Architecture Reference Engine
### ASP.NET Core Web API for Senior Laravel Backend Engineers

An enterprise-grade reference implementation demonstrating modern **ASP.NET Core (.NET 8/9/10)** designed specifically as a Rosetta Stone for professional **Laravel** engineers.

---

## 1. Architectural Mapping: Laravel to Modern .NET

| Concern | Laravel Idiom | Modern .NET Idiom (OrderPulse) | OrderPulse Implementation File |
| :--- | :--- | :--- | :--- |
| **Dependency Injection** | `AppServiceProvider::register()` (`$this->app->singleton(...)`) | `IServiceCollection` (`AddScoped`, `AddSingleton`, `AddTransient`) | [DependencyInjection.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Infrastructure/DependencyInjection.cs) |
| **Entity / ORM** | Eloquent Model (`class Order extends Model`) Active Record pattern | POCO (Plain Old CLR Object) Data Mapper pattern + Unit of Work | [Order.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Domain/Entities/Order.cs) |
| **Database Mapping** | Migrations (`Schema::create('orders', ...)`) | EF Core Fluent API Configurations (`IEntityTypeConfiguration<T>`) | [OrderConfiguration.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Infrastructure/Persistence/Configurations/OrderConfiguration.cs) |
| **Unit of Work** | `DB::transaction(fn() => ...)` | `DbContext` + `IUnitOfWork.SaveChangesAsync()` | [ApplicationDbContext.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Infrastructure/Persistence/ApplicationDbContext.cs) |
| **Request Validation** | Form Requests (`StoreOrderRequest::rules()`) | FluentValidation (`AbstractValidator<CreateOrderCommand>`) | [CreateOrderCommandValidator.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Application/Features/Orders/Commands/CreateOrder/CreateOrderCommandValidator.cs) |
| **Validation Lifecycle** | Automatic FormRequest validation before Controller | MediatR Pipeline Behavior (`ValidationBehavior<TRequest, TResponse>`) | [ValidationBehavior.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Application/Common/Behaviors/ValidationBehavior.cs) |
| **Use Cases / Actions** | Single Action Classes (`CreateOrderAction::execute()`) | CQRS Handlers via MediatR (`IRequestHandler<CreateOrderCommand, OrderDto>`) | [CreateOrderCommandHandler.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Application/Features/Orders/Commands/CreateOrder/CreateOrderCommandHandler.cs) |
| **API Transformations** | Eloquent API Resources (`OrderResource::toArray()`) | Data Transfer Objects (Records) | [OrderDtos.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Application/Common/DTOs/OrderDtos.cs) |
| **Domain Events** | `event(new OrderCreated($order))` | `AggregateRoot.RaiseDomainEvent()` dispatched upon DB commit | [OrderEvents.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Domain/Events/OrderEvents.cs) |
| **Event Listeners** | `App\Listeners\SendOrderNotification` | MediatR Notification Handlers (`INotificationHandler<OrderCreatedDomainEvent>`) | [OrderCreatedDomainEventHandler.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Application/Features/Orders/Events/OrderCreatedDomainEventHandler.cs) |
| **Authentication** | Laravel Sanctum / Passport | ASP.NET Core JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) | [AuthenticationExtensions.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Api/Extensions/AuthenticationExtensions.cs) |
| **Authorization** | Gates (`Gate::define(...)`) & Policies | Policy-based Authorization (`[Authorize(Policy = "AdminOnly")]`) | [AuthenticationExtensions.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Api/Extensions/AuthenticationExtensions.cs) |
| **Caching** | `Cache::remember('key', $ttl, fn() => ...)` | `IDistributedCache` (StackExchange.Redis) + `ICacheService.GetOrCreateAsync` | [DistributedCacheService.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Infrastructure/Caching/DistributedCacheService.cs) |
| **Queues & Jobs** | `dispatch(new SendEmailJob())` + `php artisan queue:work` | `System.Threading.Channels` + `BackgroundService` Hosted Worker | [QueuedHostedService.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Infrastructure/BackgroundJobs/QueuedHostedService.cs) |
| **Global Error Handling** | `bootstrap/app.php` (`withExceptions`) / `Handler.php` | Exception Middleware converting to RFC 7807 `ProblemDetails` | [ExceptionHandlingMiddleware.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Api/Middleware/ExceptionHandlingMiddleware.cs) |
| **Structured Logging** | Monolog (`Log::info(...)`) | Serilog (`ILogger<T>`) with JSON message templates & context enrichment | [Program.cs](file:///c:/Users/sakhawat/dotnet/learning_code/src/OrderPulse.Api/Program.cs) |

---

## 2. Solution Structure & Layer Responsibilities

```
OrderPulse/
├── OrderPulse.slnx
├── docker-compose.yml
├── Dockerfile
│
├── src/
│   ├── OrderPulse.Domain/            # Zero external dependencies
│   │   ├── Common/                   # Entity, AggregateRoot, ValueObject, IDomainEvent
│   │   ├── Entities/                 # Order, OrderItem, Product, Customer
│   │   ├── Enums/                    # OrderStatus, PaymentStatus
│   │   ├── Exceptions/               # InsufficientStockException, InvalidOrderStateTransitionException
│   │   ├── Events/                   # OrderCreatedDomainEvent, OrderCancelledDomainEvent
│   │   ├── Repositories/             # Interfaces: IOrderRepository, IProductRepository, IUnitOfWork
│   │   └── ValueObjects/             # Money, Address
│   │
│   ├── OrderPulse.Application/       # Use Cases, CQRS & Orchestration
│   │   ├── Common/                   # Behaviors (Validation, Logging), DTOs, Interfaces (ICacheService)
│   │   └── Features/
│   │       ├── Orders/
│   │       │   ├── Commands/CreateOrder/ (Command, Handler, Validator)
│   │       │   ├── Commands/CancelOrder/ (Command, Handler, Validator)
│   │       │   ├── Queries/GetOrderById/ (Query, Handler)
│   │       │   ├── Queries/GetCustomerOrders/ (Query, Handler)
│   │       │   └── Events/           # Domain event handlers (cache invalidation, async jobs)
│   │       └── Products/
│   │           └── Queries/GetProducts/ (Query with Redis cache-aside)
│   │
│   ├── OrderPulse.Infrastructure/    # Databases, Cache, Security, Background Jobs
│   │   ├── BackgroundJobs/           # Channel queue & QueuedHostedService
│   │   ├── Caching/                  # DistributedCacheService (Redis)
│   │   ├── Identity/                 # JwtTokenGenerator, JwtSettings
│   │   └── Persistence/              # ApplicationDbContext, EF Core Fluent Configurations, Migrations, Repositories
│   │
│   └── OrderPulse.Api/               # Presentation & HTTP Host
│       ├── Controllers/              # AuthController, OrdersController, ProductsController
│       ├── Extensions/               # SwaggerExtensions, AuthenticationExtensions
│       ├── Middleware/               # ExceptionHandlingMiddleware (RFC 7807)
│       └── Program.cs                # Entrypoint & DI Pipeline
│
└── tests/
    ├── OrderPulse.Domain.UnitTests/      # Unit tests for Domain Aggregates & Invariants
    └── OrderPulse.Application.UnitTests/ # Unit tests with NSubstitute for CQRS Handlers & Validators
```

---

## 3. How to Run & Verify

### Running Unit Tests
```bash
dotnet test
```
Runs 13 automated unit tests covering domain business logic, optimistic concurrency models, FluentValidation rules, and MediatR handlers.

### Running with Docker (PostgreSQL + Redis + API)
```bash
docker compose up --build
```
* **Swagger UI**: Accessible at `http://localhost:5000/`

### Running Locally without Docker
1. Ensure PostgreSQL is running on port `5432` and Redis on `6379` (or update `appsettings.json`).
2. Run the API:
```bash
dotnet run --project src/OrderPulse.Api
```
3. Open your browser to `http://localhost:5000/`.

---

## 4. Test Credentials & API Walkthrough

1. **Get JWT Token**:
   * `POST /api/auth/login`
   * Body:
     ```json
     {
       "email": "alex.engineer@enterprise.com",
       "password": "SecretPassword123!"
     }
     ```
   * Copy the returned `accessToken`.
2. **Authorize in Swagger**:
   * Click the **Authorize** button at the top right of Swagger UI.
   * Paste the token (no `Bearer ` prefix needed).
3. **Browse Cached Products**:
   * `GET /api/products` (Cached in Redis for 5 minutes).
4. **Create Order (Stock Reservation + Background Job)**:
   * `POST /api/orders`
   * Body:
     ```json
     {
       "customerId": "11111111-1111-1111-1111-111111111111",
       "street": "123 Technology Way",
       "city": "Austin",
       "state": "TX",
       "postalCode": "78701",
       "country": "USA",
       "items": [
         {
           "productId": "22222222-2222-2222-2222-222222222221",
           "quantity": 2
         }
       ]
     }
     ```
   * Notice: Product stock is deducted, Redis cache is automatically invalidated, and an async order confirmation background job is dispatched!
#   D O T - L e a r n  
 