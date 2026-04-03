# Schema Post-Processing

After all AL files are parsed, `IDBMLWriter.WriteDBMLAsync` passes the `OutputSchema` through `ISchemaPostProcessor` before generating DBML output. The post-processor works on a **deep copy** of the schema and never mutates the original.

## Pipeline

The steps run in this order:

```
1. InferSinglePkWhenOnlyUnknownPlusOneColumn
2. RemoveEmptyTables
3. CreateStubsForMissingReferencedTables
4. AddMissingReferencedFieldsToExistingTables
5. Build singlePkByTable
6. ResolveUnknownFieldReferences
7. RemoveUnknownFieldColumns
```

## UnknownField convention

When a `TableRelation` specifies no field (e.g. `TableRelation = Vendor;`), the field name is set to the sentinel value `"UnknownField"`. This signals to the post-processor that the actual target field is unknown and should be resolved from the target table's primary key if possible.

## Step details

**1. InferSinglePkWhenOnlyUnknownPlusOneColumn**
If a table has exactly one real field and one `UnknownField` placeholder, and no PK is declared, the real field is inferred as the primary key. This handles partially parsed tables that lack an explicit `key(PK; ...)` declaration.

**2. RemoveEmptyTables**
Tables with no fields are removed. These are typically side effects of a `tableextension` file being parsed when the base table AL file is not in the input — the base table gets created as an empty shell and is not useful in the output.

**3. CreateStubsForMissingReferencedTables**
For each `TableRelation` reference pointing to a table absent from the schema, a stub table is created:
- If the reference includes an explicit field name: the stub contains that field as PK.
- If only `UnknownField` references exist: the stub contains an `UnknownField` column as PK (typed from the referencing field). The relation is kept as-is in the output — the schema is technically incomplete but the reference is not broken.

**4. AddMissingReferencedFieldsToExistingTables**
For tables that exist (e.g. from a `tableextension`) but are missing a field that is explicitly referenced by a `TableRelation`, the missing field is added as PK at position 0. This covers the case where the base table AL file is absent but the extension file was parsed.

**5–7. Resolve and clean**
- `singlePkByTable` indexes all tables with exactly one primary key field.
- `ResolveUnknownFieldReferences` replaces `UnknownField` with the actual PK name for any table in that index. It also aligns the type of the referencing field with the PK's type.
- `RemoveUnknownFieldColumns` removes `UnknownField` placeholder columns from resolved tables, unless the column is itself the PK (stub tables where the target PK remains unknown).

## Risks and limitations

| Situation | Behaviour |
|---|---|
| Target table has multiple PKs | `UnknownField` is not resolved and remains in output |
| Target table absent, no explicit field in `TableRelation` | Stub created with `UnknownField [pk]`; schema is incomplete |
| Referenced field is not the actual PK (edge case) | Stub or partial table marks a non-PK field as PK; schema is incorrect for that relation |
| Conditional `TableRelation` (`if/else`) | Relation not parsed; no reference generated |
