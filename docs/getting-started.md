# Getting started

Repository scaffold from [novolis-template-dotnet](https://github.com/Novolis-Platform/novolis-template-dotnet).

## Documentation defaults

New packable projects should:

1. Import `build/Novolis.Documentation.props` (or a repo-specific `build/*.Documentation.props` that imports [Novolis.Documentation.props](https://github.com/Novolis-Platform/novolis-governance/blob/main/build/Novolis.Documentation.props)).
2. Add `README.md` next to each packable `.csproj` and set `PackageReadmeFile`.
3. Document all public API with XML comments before removing transitional `CS1591` suppressions.

See [documentation-policy.md](https://github.com/Novolis-Platform/novolis-governance/blob/main/docs/documentation-policy.md).

