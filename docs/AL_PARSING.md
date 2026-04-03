# AL File Parsing

This document describes the AL parsing specifics relevant to this project. It does not cover the full AL language specification — only what is needed to generate DBML output.

## Supported file types

| AL type | Detection keyword | Handled by |
|---|---|---|
| Table | `table` | `ParseTable` |
| Table extension | `tableextension` | `ParseTableExtension` |
| Enum | `enum` | `ParseEnum` |
| Enum extension | `enumextension` | `ParseEnumExtension` |
| Other (page, report, codeunit...) | — | Silently skipped |

## File type detection

Detection is done by regex on the **file content**, not the filename or extension. The detection order matters:

1. `enumextension` before `enum` — an extension file could partially match the enum pattern
2. `tableextension` before `table` — same reason

## Object identifiers

Every AL object has a numeric ID: `table 50100 "Customer"`. The ID is **ignored** — only the name is extracted and used in the DBML output.

## Name conventions

AL identifiers can be quoted or unquoted:
- `"My Field"` → quoted, typically used for names with spaces or special characters
- `MyField` → unquoted

Names containing `/` must be re-quoted in DBML output (e.g. `"No./Name"`) since `/` is not valid in an unquoted DBML identifier. This is handled by `AlSyntaxHelper.CleanName`.

## Table parsing

### Fields

AL field syntax:
```al
field(50; "My Field"; Text[100]) { ... }
```

The field body `{ ... }` is parsed for:
- `FieldClass = FlowField` → marks the column as a FlowField
- `CalcFormula = ...` → extracts the formula as a string

### Primary keys

Primary keys are detected from `key(...)` declarations:
- A key named `PK` is treated as the primary key
- A key with `Clustered = true` in its body is treated as the primary key
- Composite keys (comma-separated fields) are supported

```al
key(PK; "No.") { Clustered = true; }
```

## Table extensions

A `tableextension` extends an existing base table:
```al
tableextension 50100 "My Extension" extends "Customer" { ... }
```

Fields from the extension are **merged** into the base table's `DBMLTable`. If the base table has not been parsed yet, a new `DBMLTable` is created for it and will be enriched when the base table file is processed. Parse order across files therefore does not matter.

## Enum parsing

An enum can optionally implement one or more interfaces via the `implements` clause:

```al
enum 50100 "My Status" implements "IStatus", "ILabel"
{
    value(0; Open) { Caption = 'Open'; }
    value(1; Closed) { Caption = 'Closed'; }
}
```

The `implements` clause is handled by the detection regex but the interface names are **not captured** in the output — only the enum name and its values matter for DBML. The corresponding interface definition files (`.al` files containing `interface "IStatus" { ... }`) are `Unknown` type and silently skipped.

### Detection order caveat

The `Enum` detection pattern starts with `^\s*enum\s+`. Since `enumextension` also starts with `enum`, checking `Enum` before `EnumExtension` would misclassify extension files. The correct order is always `EnumExtension` → `Enum`.

## Enum extensions

```al
enumextension 50101 "My Status Ext" extends "My Status" { value(2; Pending) }
```

Values are merged into the base enum. Duplicate values are ignored.

## TableRelation

`TableRelation` defines a foreign key lookup to another table. It is parsed from the field body and stored in `DBMLColumn.References` as a two-element array `[tableName, fieldName]`.

### Supported syntaxes

| AL syntax | Result |
|---|---|
| `TableRelation = Vendor;` | `["Vendor", "UnknownField"]` |
| `TableRelation = Vendor."No.";` | `["Vendor", "No."]` |
| `TableRelation = Vendor.Code;` | `["Vendor", "Code"]` |
| `TableRelation = "Salesperson/Purchaser";` | `["Salesperson/Purchaser", "UnknownField"]` |
| `TableRelation = "Salesperson/Purchaser"."Code";` | `["Salesperson/Purchaser", "Code"]` |
| `TableRelation = "Salesperson/Purchaser".Code;` | `["Salesperson/Purchaser", "Code"]` |

When no field is specified, the sentinel value `"UnknownField"` is used. This is resolved at post-processing time (see ARCHITECTURE.md — Schema post-processing).

### WHERE filters

`where` clauses are silently ignored — the table (and field, if specified) are still extracted:

```al
TableRelation = Vendor where ("Blocked" = const(false));        // → ["Vendor", "UnknownField"]
TableRelation = Vendor."No." where ("No." = field("No."));      // → ["Vendor", "No."]
```

### Unsupported syntaxes

Conditional `TableRelation` using `if/else` is not parsed — the target table depends on a runtime field value and is therefore ambiguous. The relation is silently ignored.

```al
TableRelation = if (Type = const(Customer)) Customer else Vendor;  // → no reference generated
```

### PK assumption

In AL, `TableRelation` almost always points to the primary key of the target table. When a referenced field is explicit (e.g. `.Code`), it is marked as `IsPrimaryKey = true` in the generated stub or partial table. This is a heuristic — in the rare case where the referenced field is not actually the PK, the generated schema will be incorrect for that relation.

## FlowFields

AL FlowFields are calculated fields with no physical storage:
```al
field(20; "Balance"; Decimal) {
    FieldClass = FlowField;
    CalcFormula = sum("Ledger Entry".Amount where("Account No." = field("No.")));
}
```

FlowFields are included in the DBML output as regular columns. The `CalcFormula` is captured and can be used as a note. DBML has no native equivalent for this concept.

## State accumulation

`IAlParser` accumulates parsed tables and enums in internal state across multiple `Parse*` calls. This allows multi-file parsing (folder, workspace) to produce a single unified `OutputSchema`. State is cleared between command executions via `ClearOutputSchema()`.
