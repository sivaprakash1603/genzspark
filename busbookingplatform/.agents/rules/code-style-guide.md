---
trigger: always_on
---

# OOP & C# Code Style Guide

A practical reference for writing clean, maintainable object-oriented code in C#. Based on core OOP principles and observed best practices.

---

## 1. Object-Oriented Design Principles

### Use OOP Appropriately

OOP is designed for complex, real-world problems — not trivial ones. Applying it to simple tasks (e.g., adding two numbers) adds unnecessary overhead.

> **Rule:** Reach for OOP when you need to model entities with properties, behaviors, and relationships. Use simpler approaches for isolated calculations.

### Map Code to Real-World Structures

Program structure should mirror real-life entities. Each object should be **wholesome** — containing all properties and behaviors relevant to its context.

```csharp
// ✅ Wholesome object - all relevant properties together
public class Customer
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
}

// ❌ Fragmented - forces awkward ID mapping across modules
public string GetCustomerName(int id) { ... }
public string GetCustomerEmail(int id) { ... }
```

---

## 2. The Four OOP Principles

### Inheritance

Use inheritance to reuse existing properties and behaviors, extending only what's needed.

**Allowed in C#:**
- **Single** — one base class, one derived class
- **Multi-level** — `Grandparent → Parent → Child`
- **Hierarchical** — one base class, multiple derived classes

**Not allowed:**
- **Multiple inheritance** — causes the diamond problem (ambiguity when two base classes share a method name). Use interfaces as a workaround.

```csharp
// ✅ Hierarchical inheritance
public class Account { /* shared properties */ }
public class SavingsAccount : Account { /* savings-specific */ }
public class CurrentAccount : Account { /* current-specific */ }
```

> **Rule:** Put shared properties in the base class. Only put specific properties in derived classes.

---

### Encapsulation

Hide internal workings. Expose only what consumers need.

```csharp
public class Account
{
    // Internal — not exposed
    private void ValidateBalance() { ... }

    // Exposed — consumers interact through this
    public decimal GetBalance() { ... }
}
```

**Access specifiers (C# / .NET):**

| Specifier | Scope |
|---|---|
| `public` | Accessible everywhere |
| `private` | Within the class only |
| `protected` | Class and derived classes (inheritance only) |
| `internal` | Within the same assembly |
| `protected internal` | Same assembly or derived class |
| `private protected` | Derived class within same assembly |

> **Rule:** Use `private` for internal logic. Only make methods `public` if external callers need them.

---

### Abstraction

Show only what's relevant for a given context. The same object can expose different behaviors to different consumers.

```csharp
// Different interfaces expose only the relevant methods
public interface IOperatorCustomer
{
    void BookBus();
    void MakePayment();
    void CancelBus();
}

public interface IAdminCustomer
{
    void ProvideFeedback();
    void RegisterOperator();
    void UnregisterOperator();
}

// Customer implements both — but each consumer only sees what they need
public class Customer : IOperatorCustomer, IAdminCustomer { ... }
```

> **Rule:** Use interfaces to expose context-appropriate behavior. All implementations live inside the class.

---

### Polymorphism

The same method behaves differently depending on context or type.

**Static (compile-time) polymorphism — Overloading:**

```csharp
// Compiler resolves the correct method before runtime
public bool Login(string username, string password) { ... }  // desktop
public bool Login(BiometricData biometrics) { ... }          // mobile
```

**Dynamic (runtime) polymorphism — Overriding:**

```csharp
public class Account
{
    public virtual decimal CalculateDiscount() => 0m;
}

public class GoldAccount : Account
{
    public override decimal CalculateDiscount() => 0.10m; // 10% discount
}
```

> **Rule:** Overloading = different parameter lists. Overriding = different behavior in a derived class. Overriding requires inheritance.

---

## 3. Properties

Prefer C# properties over explicit getter/setter methods.

```csharp
// ❌ Old style
private string _accountNumber;
public string GetAccountNumber() => _accountNumber;
public void SetAccountNumber(string value) => _accountNumber = value;

// ✅ Modern style
public string AccountNumber { get; set; }

// ✅ With custom logic (e.g., masking)
public string AccountNumber
{
    get => "****" + _accountNumber[^4..];
    set => _accountNumber = value;
}
```

**Default values:**

```csharp
public string AccountNumber { get; set; } = string.Empty;
public string Name { get; set; } = string.Empty;
```

**Required fields:**

```csharp
public required string Email { get; set; }
```

> **Rule:** Aim for zero warnings, not just zero errors. Unhandled nullable reference types should be resolved explicitly.

---

## 4. Constructors

Use constructor overloading (static polymorphism) to support flexible initialization.

```csharp
public class Account
{
    // Default constructor
    public Account() { }

    // Parameterized constructor
    public Account(string accountNumber, string name, DateTime dob)
    {
        AccountNumber = accountNumber;
        NameOnAccount = name;
        DateOfBirth = dob;
    }
}
```

**IDE shortcuts (Visual Studio / VS Code):**
- `prop` + Tab → property template
- `ctor` + Tab → default constructor
- `Ctrl+.` → generate parameterized constructor from selected properties

---

## 5. Methods

### Communication Pattern

Methods have two communication channels:
- **Parameters** — input to the method
- **Return type** — output from the method

```csharp
// ✅ Clear input/output contract
public Account OpenAccount(string name, DateTime dob, string email) { ... }

// ❌ Avoid void when a result can be returned
public void OpenAccount(...) { ... }
```

### Size and Responsibility

- Keep methods **under 15 executable lines**
- One method = one responsibility
- Break complex operations into private helper methods

```csharp
public Account OpenAccount(AccountRequest request)
{
    ValidateRequest(request);           // private helper
    var account = CreateAccount(request); // private helper
    _accounts.Add(account);
    return account;
}
```

### Scoping

- Method parameters are short-lived — they exist only within the method scope
- Avoid holding references in memory longer than necessary

---

## 6. Project Structure

Organize code into folders that reflect responsibility:

```
/Models
    Account.cs
    SavingsAccount.cs
    CurrentAccount.cs
/Services
    CustomerService.cs
/Interfaces
    ICustomerInteract.cs
```

**Interface naming:** Use verb-based names describing the functionality — e.g., `ICustomerInteract`, `IAccountManager`.

---

## 7. Data Types

| Use case | Recommended type |
|---|---|
| Account/ID numbers | `string` (no arithmetic; avoids size limits) |
| Currency | `decimal` (fewer rounding errors than `float`/`double`) |
| Dates | `DateTime` |
| Categories/types | `enum` |

```csharp
public enum AccountType
{
    SavingsAccount,
    CurrentAccount
}
```

---

## 8. Control Flow

Prefer `switch` over long `if-else` chains for readability:

```csharp
// ✅ Preferred
switch (choice)
{
    case 1: return new SavingsAccount();
    case 2: return new CurrentAccount();
    default: throw new ArgumentException("Invalid choice");
}

// ❌ Avoid for multiple branches
if (choice == 1) { ... }
else if (choice == 2) { ... }
```

Use validation loops to enforce correct input:

```csharp
int choice = 0;
while (choice < 1 || choice > 2)
{
    Console.Write("Enter 1 (Savings) or 2 (Current): ");
    choice = int.Parse(Console.ReadLine()!);
}
```

---

## 9. Collections and Data Management

Use `static` collections to share state across the application:

```csharp
public class CustomerService
{
    private static List<Account> _accounts = new();
    private static string _accountSeed = "ACC1000";
}
```

Use the base class type when a collection holds multiple derived types:

```csharp
// ✅ Parent reference holds both SavingsAccount and CurrentAccount
private static List<Account> _accounts = new();
```

---

## 10. UX and Output Standards

- **Currency:** Always prefix with currency code — `INR 5,000` not just `5000`
- **Phone numbers:** Include country code — `+91 98765 43210`
- **Salutations:** Use `Mr.`/`Ms.` when gender is captured
- **Interactions:** Maintain a polite, professional tone in all user-facing messages

---

## 11. ToString() Override

Always override `ToString()` to return meaningful output. The default returns only the class name.

```csharp
public override string ToString()
{
    return $"Account: {AccountNumber} | Name: {NameOnAccount} | Balance: INR {Balance:N2}";
}

// Derived class appends its own context
public override string ToString()
{
    return base.ToString() + $" | Type: Savings";
}
```

---

## 12. Code Quality Checklist

- [ ] Zero warnings (not just zero errors)
- [ ] No method exceeds 15 executable lines
- [ ] Private methods used for internal logic
- [ ] `switch` used over long `if-else` chains
- [ ] Return types used instead of `void` where possible
- [ ] All nullable reference types handled explicitly
- [ ] Default values set on string properties
- [ ] Base class holds shared properties; derived classes hold specific ones
- [ ] Interfaces used to expose only context-relevant behavior
- [ ] Collections typed to the base class when storing mixed derived types
