---
name: aspnetzero-dto-patterns
description: "DTO creation patterns with Data Annotations for ASP.NET Zero"
---

# ASP.NET Zero DTO Patterns

## Quick Start

DTOs go in **Application.Shared** under a feature `Dto` folder:

```
aspnet-core/src/CadentManagement.Application.Shared/
  {Feature}/
    Dto/
      {Feature}ListDto.cs           # For list/table display
      {Feature}EditDto.cs           # For edit form data
      Create{Feature}Dto.cs         # For create input
      Get{Feature}ForEditOutput.cs  # Wrapper for edit form
      Get{Feature}Input.cs          # Paged/filtered list input
    I{Feature}AppService.cs         # Service interface
```

## DTO Types

### List DTO (for tables)

```csharp
namespace CadentManagement.{Feature}.Dto;

public class ProductListDto : EntityDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime CreationTime { get; set; }
}
```

### Edit DTO (for forms)

```csharp
public class ProductEditDto : EntityDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; }

    public string Description { get; set; }

    [Range(0, 999999)]
    public decimal Price { get; set; }
}
```

### Create DTO (input)

```csharp
public class CreateProductDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; }

    public string Description { get; set; }

    [Range(0, 999999)]
    public decimal Price { get; set; }
}
```

### GetForEdit Output (wrapper with lookup data)

```csharp
public class GetProductForEditOutput
{
    public ProductEditDto Product { get; set; }

    // Include List<ComboboxItemDto> for each enum/dropdown field
    public List<ComboboxItemDto> Categories { get; set; }
    public List<ComboboxItemDto> Statuses { get; set; }
}
```

> **Pattern**: `GetForEdit` uses `NullableIdDto` as input (not a custom DTO). When `Id` is null (create mode), the app service returns a new DTO with default values plus lookup data. When `Id` has a value (edit mode), it loads the entity and returns with the same lookup data. All UIs always call `GetForEdit` — even for create — and use server-returned dropdown options.

### Paged Input

```csharp
public class GetProductsInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}
```

## Validation Rules

- Use `System.ComponentModel.DataAnnotations` (NOT FluentValidation)
- **Never** use `double.MaxValue` as a max attribute
- Common attributes: `[Required]`, `[StringLength(max)]`, `[Range(min, max)]`, `[EmailAddress]`
- DTOs must be serializable

## Key Conventions

| Rule | Description |
|------|-------------|
| Separate DTOs per operation | Don't use one DTO for both create and list |
| `NullableIdDto` for GetForEdit | Null = create mode (defaults), has value = edit mode |
| `EntityDto` for list | Non-nullable Id since it always comes from DB |
| Inherit `PagedAndSortedResultRequestDto` | For any list input that supports paging |
| `Filter` property on list inputs | Standard name for text search |

## Reference Files

- **Edit DTO**: `aspnet-core/src/CadentManagement.Application.Shared/Editions/Dto/EditionEditDto.cs`
- **Create DTO**: `aspnet-core/src/CadentManagement.Application.Shared/Editions/Dto/CreateEditionDto.cs`
- **GetForEdit output**: `aspnet-core/src/CadentManagement.Application.Shared/Editions/Dto/GetEditionEditOutput.cs`

## Checklist

- [ ] DTOs in Application.Shared/{Feature}/Dto/
- [ ] Validation with Data Annotations
- [ ] Separate DTOs per operation (List, Edit, Create, GetForEditOutput, Input)
- [ ] PagedAndSortedResultRequestDto for paged inputs
- [ ] No business logic in DTOs
- [ ] Namespace: `CadentManagement.{Feature}.Dto`
