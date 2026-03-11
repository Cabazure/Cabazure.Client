# Generator Enhancements: PATCH Verb, Incremental Optimization, and Validation

**Date:** 2026-03-11  
**Branch:** feat/optimization-backlog  
**Commit:** 255dbf6  
**Author:** Dallas (via Ricky Kaare Engelharth)

## Context

The generator and runtime library had several areas for improvement:
1. Missing PATCH HTTP verb support
2. Incremental generator predicate not filtering early enough
3. Duplicate naming logic across descriptors
4. No validation for nullable body parameters (would compile but fail at runtime)

## Decisions

### 1. Add PATCH Verb Support

**Decision:** Implement [Patch] attribute following the same pattern as existing HTTP verb attributes (Get, Post, Put, Delete).

**Rationale:**
- PATCH is a standard HTTP method for partial updates (RFC 5789)
- Users need PATCH for RESTful API patterns where partial resource updates are preferred over full PUT replacements
- Consistent with existing attribute design patterns

**Implementation:**
- Created `PatchAttribute.cs` in runtime library matching style of other verb attributes
- Added `PatchAttribute` constant to `TypeConstants.cs`
- Updated `EndpointMethodDescriptor.TryGetEndpointRoute()` to handle PATCH case
- Generator emits `new HttpMethod("PATCH")` instead of `HttpMethod.Patch` because HttpMethod.Patch doesn't exist in netstandard2.0
- Updated ECL003 diagnostic message to list Patch as valid verb

**Trade-offs:**
- ✅ Enables PATCH support for users
- ✅ Consistent with existing patterns
- ⚠️ Requires runtime `new HttpMethod("PATCH")` instantiation (HttpMethod.Patch not available in netstandard2.0), but this is standard practice for custom HTTP methods

### 2. Optimize Incremental Generator Predicate

**Decision:** Change ClientEndpointGenerator predicate from `static (_, _) => true` to `static (node, _) => node is InterfaceDeclarationSyntax`.

**Rationale:**
- The `ForAttributeWithMetadataName` API triggers semantic analysis on every syntax node that passes the predicate
- Filtering to only `InterfaceDeclarationSyntax` nodes avoids unnecessary semantic model queries on classes, structs, enums, methods, etc.
- [ClientEndpoint] is only valid on interfaces, so filtering early is safe and improves performance
- This is a standard incremental generator optimization pattern recommended by Roslyn team

**Implementation:**
- Changed predicate in `ClientEndpointGenerator.Initialize()`
- Added `using Microsoft.CodeAnalysis.CSharp.Syntax` for `InterfaceDeclarationSyntax`

**Trade-offs:**
- ✅ Improves generator performance by reducing unnecessary semantic analysis
- ✅ No behavioral change (attribute is still only valid on interfaces)
- ✅ Standard recommended practice

### 3. Extract Duplicate Naming Logic

**Decision:** Create `EndpointNaming` static helper class to centralize interface/class name computation logic shared by `EndpointDescriptor` and `EndpointReferenceDescriptor`.

**Rationale:**
- Both descriptors had identical logic for:
  - Computing class name from interface name (strip leading 'I' or append '_Implementation')
  - Walking nested class parents to build fully-qualified interface name
- Duplication creates maintenance burden and risk of divergence
- Static helper class is appropriate for pure logic with no dependencies

**Implementation:**
- Created `src/Cabazure.Client/Descriptors/EndpointNaming.cs`
- `GetNames(InterfaceDeclarationSyntax)` returns tuple `(string InterfaceName, string ClassName)`
- Updated both descriptors to call `EndpointNaming.GetNames(syntax)` instead of duplicating logic

**Trade-offs:**
- ✅ Eliminates code duplication
- ✅ Single source of truth for naming logic
- ✅ Easier to test and maintain
- ⚠️ Slight indirection, but benefit outweighs cost

### 4. Add Nullable Body Parameter Validation

**Decision:** Emit ECL008 diagnostic error when a parameter is marked with [Body] but is nullable.

**Rationale:**
- Nullable body parameters will compile successfully but fail at runtime when serialization/deserialization occurs
- Better to fail at compile-time with clear diagnostic than at runtime with obscure serialization error
- Aligns with general C# practice of using nullable reference types for optional parameters and non-nullable for required parameters
- Body parameters are conceptually required (if body is optional, the parameter shouldn't exist)

**Implementation:**
- Added `DiagnosticDescriptors.BodyParameterCannotBeNullable` (ECL008) to `DiagnosticDescriptors.cs`
- Modified `EndpointMethodDescriptor` body parameter handling to check:
  - `isNullable` (syntax-level check: `parameter.Type.IsKind(NullableType)`)
  - `parameterType.NullableAnnotation == NullableAnnotation.Annotated` (semantic-level check)
- Emit ECL008 and return null (fail descriptor creation) if body parameter is nullable

**Trade-offs:**
- ✅ Prevents runtime errors by catching issue at compile-time
- ✅ Clear diagnostic message guides user to fix
- ✅ Aligns with C# nullable reference type semantics
- ⚠️ Breaking change if users had nullable body parameters (but those would fail at runtime anyway)

## Related Files

**Runtime:**
- `src/Cabazure.Client.Runtime/PatchAttribute.cs` (new)

**Generator:**
- `src/Cabazure.Client/TypeConstants.cs`
- `src/Cabazure.Client/ClientEndpointGenerator.cs`
- `src/Cabazure.Client/Descriptors/EndpointMethodDescriptor.cs`
- `src/Cabazure.Client/Descriptors/EndpointDescriptor.cs`
- `src/Cabazure.Client/Descriptors/EndpointReferenceDescriptor.cs`
- `src/Cabazure.Client/Descriptors/EndpointNaming.cs` (new)
- `src/Cabazure.Client/Diagnostics/DiagnosticDescriptors.cs`

## Testing

All changes verified with:
- `dotnet build src/Cabazure.Client.Runtime -c Release` ✅
- `dotnet build src/Cabazure.Client -c Release` ✅

No test updates required (these are generator infrastructure changes, not behavioral changes to existing features).

## Future Considerations

1. **Additional HTTP verbs**: Could add HEAD, OPTIONS, TRACE if users request them (same pattern as PATCH)
2. **Path parameter validation**: Currently no validation that path parameter names are valid C# identifiers (e.g., `{user.id}` would fail at runtime)
3. **Generator diagnostics tests**: Consider adding snapshot tests for all ECL00X diagnostics to ensure messages stay consistent
4. **EndpointNaming unit tests**: Consider adding direct tests for naming logic since it's now extracted to testable helper
