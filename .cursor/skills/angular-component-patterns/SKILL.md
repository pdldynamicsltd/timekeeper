---
name: angular-component-patterns
description: "Angular 21 standalone component patterns for ASP.NET Zero with signals, inject(), and AppComponentBase"
---

# Angular Component Patterns

## Quick Start

All Angular components in ASP.NET Zero use **standalone components** with **signal-based state**:

```typescript
import { Component, OnInit, inject, viewChild, signal } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { LocalizePipe } from '@shared/common/pipes/localize.pipe';
import { PermissionPipe } from '@shared/common/pipes/permission.pipe';
import { BusyIfDirective } from '@shared/utils/busy-if.directive';

@Component({
    templateUrl: './products.component.html',
    animations: [appModuleAnimation()],
    imports: [
        BusyIfDirective,
        LocalizePipe,
        PermissionPipe,
        // ... other imports
    ],
})
export class ProductsComponent extends AppComponentBase implements OnInit {
    private _productService = inject(ProductServiceProxy);

    // Signals for state
    readonly filter = signal<string>('');

    ngOnInit(): void {
        // Setup logic (but NOT data fetching if using lazy-loaded table)
    }

    getProducts(): void {
        // Data fetching logic
    }
}
```

## Key Conventions

| Pattern | Old (Decorator) | New (Signal-based) |
|---------|-----------------|---------------------|
| Dependency Injection | `constructor(private svc: Service)` | `private _svc = inject(Service)` |
| ViewChild | `@ViewChild('ref') ref: Type` | `readonly ref = viewChild<Type>('ref')` |
| Output | `@Output() event = new EventEmitter()` | `readonly event = output<void>()` |
| State | `property: Type` | `readonly property = signal<Type>(value)` |
| Read signal | `this.property` | `this.property()` |
| Set signal | `this.property = value` | `this.property.set(value)` |

## AppComponentBase Utilities

Extends `AppComponentBase` to get:

| Utility | Usage | Purpose |
|---------|-------|---------|
| `this.l('Key')` | Localization | Translate text |
| `this.permission` | Permission checking | `.isGranted('Permission.Name')` |
| `this.notify` | Notifications | `.info()`, `.success()`, `.error()` |
| `this.message` | Dialogs | `.confirm()`, `.info()` |
| `this.primengTableHelper` | Table helper | PrimeNG table state management |

## Common Imports

```typescript
// Always needed
import { LocalizePipe } from '@shared/common/pipes/localize.pipe';
import { BusyIfDirective } from '@shared/utils/busy-if.directive';

// For permissions
import { PermissionPipe } from '@shared/common/pipes/permission.pipe';
import { PermissionAnyPipe } from '@shared/common/pipes/permission-any.pipe';

// For tables
import { TableModule } from 'primeng/table';
import { TieredMenu } from 'primeng/tieredmenu';
import { Tooltip } from 'primeng/tooltip';

// For forms
import { FormsModule } from '@angular/forms';
import { ButtonBusyDirective } from '@shared/utils/button-busy.directive';
import { ValidationMessagesComponent } from '@shared/utils/validation-messages.component';

// For Angular built-in pipes used in templates — MUST be imported explicitly in standalone components
import { CurrencyPipe, DatePipe, DecimalPipe, PercentPipe } from '@angular/common';
// Use CurrencyPipe when template uses | currency
// Use DatePipe when template uses | date
// Use DecimalPipe when template uses | number

// For service proxies
import { ProductServiceProxy } from '@shared/service-proxies/service-proxies';
```

## ⚠️ Critical: Angular Common Pipes in Standalone Components

In standalone components, **every pipe used in the template must be explicitly listed in the `imports` array** — including Angular built-ins from `@angular/common`. They are NOT auto-included.

| Template usage | Import required |
|---|---|
| `\| currency` | `CurrencyPipe` from `@angular/common` |
| `\| date:'yyyy-MM-dd'` | `DatePipe` from `@angular/common` |
| `\| number` | `DecimalPipe` from `@angular/common` |
| `\| percent` | `PercentPipe` from `@angular/common` |
| `\| async` | `AsyncPipe` from `@angular/common` |
| `\| json` | `JsonPipe` from `@angular/common` |

```typescript
// Example: component that formats currency and dates
@Component({
    imports: [
        CurrencyPipe,   // for | currency in template
        DatePipe,       // for | date in template
        LocalizePipe,
        // ...
    ]
})
```

## ⚠️ Critical: DateTime Fields from NSwag Proxies

ASP.NET Zero API dates come as **Luxon `DateTime` objects** (not native JS `Date`) in all NSwag-generated DTOs. Angular's `DatePipe` does **NOT** accept Luxon `DateTime` — it only accepts `string | number | Date`. Using `| date` on a Luxon field causes a `TS2769` compile error.

### ABP DateTime Infrastructure

The framework provides three built-in helpers — always use these instead of raw Luxon or Angular pipes:

| Class / Pipe | Import | Purpose |
|---|---|---|
| `DateTimeService` | `@app/shared/common/timing/date-time.service` | Injectable service for creating, converting, formatting dates (timezone-aware) |
| `ChainableDateTime` | `@app/shared/common/timing/ChainableDateTime` | Fluent wrapper: `.plusDays()`, `.startOf('day')`, `.toJSDate()`, etc. |
| `AbpDateTimePickerComponent` | `@app/shared/common/timing/abp-datetime-picker/abp-datetime-picker.component` | PrimeNG-backed date/datetime input for forms (binds directly to `DateTime`) |
| `LuxonFormatPipe` | `@shared/utils/luxon-format.pipe` | Pipe for displaying `DateTime` values in templates |

### Displaying dates in templates → `LuxonFormatPipe`

```typescript
import { LuxonFormatPipe } from '@shared/utils/luxon-format.pipe';
// Add to imports array: LuxonFormatPipe
```

```html
{{ record.createdAt | luxonFormat: 'D' }}    <!-- short date:        1/15/2024          -->
{{ record.createdAt | luxonFormat: 'DD' }}   <!-- medium date:       January 15, 2024   -->
{{ record.createdAt | luxonFormat: 'f' }}    <!-- short date+time:   1/15/2024, 1:30 PM -->
{{ record.createdAt | luxonFormat: 'F' }}    <!-- full date+time:    Monday, January 15, 2024, 1:30 PM -->
{{ record.createdAt | luxonFormat: 't' }}    <!-- time only:         1:30 PM            -->
```

These are **Luxon preset tokens** (single uppercase/lowercase letter), not custom format strings.

### Date input fields in forms → `<abp-datetime-picker>`

Never use `<input type="date">` with Luxon `DateTime` fields. Use the built-in `<abp-datetime-picker>` which accepts and emits `DateTime` directly:

```typescript
import { AbpDateTimePickerComponent } from '@app/shared/common/timing/abp-datetime-picker/abp-datetime-picker.component';
// Add to imports array: AbpDateTimePickerComponent
```

```html
<abp-datetime-picker
    id="SaleDate"
    #saleDateInput="ngModel"
    name="SaleDate"
    [ngModel]="sale().saleDate"
    (ngModelChange)="updateField('saleDate', $event)"
    required />
<validation-messages [formCtrl]="saleDateInput" />
```

For date+time fields, add `[withTimepicker]="true"`.

### Using `DateTimeService` in components

```typescript
import { DateTimeService } from '@app/shared/common/timing/date-time.service';

private _dateTimeService = inject(DateTimeService);

// Get current date (timezone-aware):
const today = this._dateTimeService.getDate();           // ChainableDateTime
const startOfDay = this._dateTimeService.getStartOfDay(); // ChainableDateTime
const luxonDt = this._dateTimeService.fromISODateString('2024-01-15'); // DateTime

// Format for display:
const formatted = this._dateTimeService.formatDate(someDateTime, 'DD'); // string
```

**Rule summary:**
- Display: `| luxonFormat: 'D'` (never `| date`)
- Form input: `<abp-datetime-picker>` (never `<input type="date">`)
- Service operations: `inject(DateTimeService)`
- `AppConsts` does NOT define date format constants — use Luxon preset tokens directly

## ⚠️ Critical: NSwag Input DTO Required Fields

NSwag generates TypeScript interfaces from C# DTOs. When a C# property is **non-nullable** (e.g., `long? ProductId`), NSwag may generate it as **required** (no `?`) in the TypeScript interface. Passing an object literal to an NSwag-generated input constructor will fail TypeScript compilation if any required field is omitted — even if it's semantically optional.

**Always pass all fields explicitly**, using `undefined` for optional ones:

```typescript
// BAD — TypeScript error if productId is required in IGetSalesInput
new GetSalesInput({
    filter: this.filter(),
    sorting: '...',
    maxResultCount: 10,
    skipCount: 0,
    // productId missing → TS2345 error
})

// GOOD — all interface fields provided
new GetSalesInput({
    filter: this.filter(),
    productId: undefined,   // optional filter, pass explicitly
    sorting: '...',
    maxResultCount: 10,
    skipCount: 0,
})
```

To discover all required fields, check the generated `IGet{Feature}Input` interface in `service-proxies.ts` before writing the component.

## File Location

```
angular/src/app/{area}/{feature}/
  {feature}.component.ts             # List component
  {feature}.component.html           # List template
  create-or-edit-{feature}-modal.component.ts    # Modal
  create-or-edit-{feature}-modal.component.html  # Modal template
```

## Reference Files

- **List component**: `angular/src/app/admin/roles/roles.component.ts`
- **Modal component**: `angular/src/app/admin/roles/create-or-edit-role-modal.component.ts`

## Checklist

- [ ] Standalone component (no NgModule)
- [ ] `imports` array in `@Component`
- [ ] `inject()` for DI (not constructor injection)
- [ ] `signal()` for state, `viewChild()` for refs, `output()` for events
- [ ] Extends `AppComponentBase`
- [ ] `animations: [appModuleAnimation()]`
- [ ] Import `LocalizePipe`, `PermissionPipe`, `BusyIfDirective`
- [ ] Import `CurrencyPipe` if template uses `| currency`
- [ ] Import `DecimalPipe` if template uses `| number`
- [ ] **Never** use `| date` on Luxon `DateTime` fields — use `| luxonFormat: 'D'` instead
- [ ] Use `LuxonFormatPipe` for displaying dates from the API
- [ ] Use `<abp-datetime-picker>` for date inputs in forms (not `<input type="date">`)
- [ ] Use `inject(DateTimeService)` for date operations in component logic
- [ ] Check `IGet{Feature}Input` in `service-proxies.ts` and pass all required fields (use `undefined` for optional ones)
