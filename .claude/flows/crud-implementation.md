# CRUD Implementation Flow

Complete workflow for implementing a CRUD feature end-to-end in ASP.NET Zero.

## Flow Overview

```
Entity → EF Config → DTOs → Mappers → Permissions → Localization → AppService → Tests → Build → UI
```

## Steps

### Step 1: Create Entity (Core)

**Skill**: `aspnetzero-entity-patterns`
**Location**: `aspnet-core/src/CadentManagement.Core/{Feature}/`
**Output**: `{EntityName}.cs`

- Inherit from `FullAuditedEntity` (default)
- Add `IMustHaveTenant` for tenant-scoped entities
- Define properties with appropriate types

### Step 2: Configure EF Core (EntityFrameworkCore)

**Skill**: `efcore-patterns`
**Location**: `aspnet-core/src/CadentManagement.EntityFrameworkCore/EntityFrameworkCore/`
**Output**: DbSet in `CadentManagementDbContext.cs`, entity config in `OnModelCreating`

- Add `DbSet<{Entity}>` property
- Configure table name, indexes, precision in `OnModelCreating`

### Step 3: Create Migration

**Command**: `dotnet ef migrations add Added_{Entity}_Entity`
**Location**: Run from EntityFrameworkCore project directory

### Step 4: Create DTOs (Application.Shared)

**Skill**: `aspnetzero-dto-patterns`
**Location**: `aspnet-core/src/CadentManagement.Application.Shared/{Feature}/Dto/`
**Output**:
- `{Entity}ListDto.cs` — for table display
- `{Entity}EditDto.cs` — for edit form
- `Create{Entity}Dto.cs` — for create input
- `Get{Entity}ForEditOutput.cs` — wrapper for edit
- `Get{Entity}sInput.cs` — paged/filtered input

### Step 5: Create Mappers (Application)

**Skill**: `mapperly-patterns`
**Location**: `aspnet-core/src/CadentManagement.Application/Mappers/`
**Output**: `{Feature}Mappers.cs`

- `{Entity}To{Entity}ListDtoMapper`
- `{Entity}To{Entity}EditDtoMapper`
- `{Entity}EditDtoTo{Entity}Mapper`
- `Create{Entity}DtoTo{Entity}Mapper`

### Step 6: Add Permissions (Core.Shared + Core)

**Skill**: `aspnetzero-permission-patterns`
**Locations**:
- `Core.Shared/Authorization/AppPermissions.cs` — constants
- `Core/Authorization/AppAuthorizationProvider.cs` — registration

**Output**: CRUD permission set (View, Create, Edit, Delete)

### Step 7: Add Localization (Core)

**Skill**: `aspnetzero-localization-patterns`
**Location**: `Core/Localization/CadentManagement/CadentManagement.xml`
**Output**: Localization entries for feature name, CRUD actions, permission names

**CRITICAL**: Search for existing keys first!

### Step 8: Create App Service Interface (Application.Shared)

**Skill**: `aspnetzero-appservice-patterns`
**Location**: `aspnet-core/src/CadentManagement.Application.Shared/{Feature}/`
**Output**: `I{Feature}AppService.cs`

### Step 9: Create App Service Implementation (Application)

**Skill**: `aspnetzero-appservice-patterns`
**Location**: `aspnet-core/src/CadentManagement.Application/{Feature}/`
**Output**: `{Feature}AppService.cs`

- Inherit `CadentManagementAppServiceBase`
- `[AbpAuthorize]` on all methods
- Use `ObjectMapper` for mapping
- Set `TenantId` after DTO → Entity mapping

### Step 10: Build & Verify

```bash
dotnet build aspnet-core/CadentManagement.All.sln
```

### Step 11: Create Tests

**Skill**: `xunit-testing-patterns`
**Location**: `aspnet-core/test/CadentManagement.Tests/{Feature}/`
**Output**: `{Feature}AppService_Tests.cs`

### Step 12: Run Tests

```bash
dotnet test aspnet-core/test/CadentManagement.Tests/
```

### Step 13: Angular UI (Optional)

**Agent**: `angular-developer`
**Skills**: `angular-component-patterns`, `angular-primeng-table-patterns`, `angular-modal-patterns`
**Output**: List component + CRUD modal + routing + menu entry

### Step 14: React UI (Optional)

**Agent**: `react-developer`
**Skills**: `react-component-patterns`, `react-state-patterns`
**Output**: CRUD page + API integration

### Step 15: Generate Service Proxies (Angular only)

Run Web.Host, then: `npm run nswag` in `angular/` directory

## Skill Chain Visualization

```
┌──────────────────────┐     ┌──────────────────────┐
│ aspnetzero-entity    │────>│ efcore-patterns       │
│ patterns             │     │                       │
└──────────────────────┘     └──────────────────────┘
         │                            │
         v                            v
┌──────────────────────┐     ┌──────────────────────┐
│ aspnetzero-dto       │────>│ mapperly-patterns     │
│ patterns             │     │                       │
└──────────────────────┘     └──────────────────────┘
         │                            │
         v                            v
┌──────────────────────┐     ┌──────────────────────┐
│ aspnetzero-permission│────>│ aspnetzero-           │
│ patterns             │     │ localization-patterns  │
└──────────────────────┘     └──────────────────────┘
         │
         v
┌──────────────────────┐     ┌──────────────────────┐
│ aspnetzero-appservice│────>│ xunit-testing         │
│ patterns             │     │ patterns              │
└──────────────────────┘     └──────────────────────┘
```

## Final Checklist

- [ ] Entity in Core with correct base class
- [ ] DbSet in DbContext
- [ ] Migration created
- [ ] DTOs in Application.Shared with Data Annotations
- [ ] Mappers in Application/Mappers
- [ ] Permissions in AppPermissions + AppAuthorizationProvider
- [ ] Localization keys in XML (no duplicates)
- [ ] App service interface in Application.Shared
- [ ] App service implementation in Application
- [ ] `dotnet build` passes
- [ ] Tests in Tests/{Feature}/
- [ ] `dotnet test` passes
- [ ] Angular/React UI (if applicable)
