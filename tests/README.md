Run from the repository/source root:

```
python3 tests/check_gmcm_contract.py
python3 tests/check_sword_contract.py
python3 tests/generate_native_hook_test.py
dotnet run --project tests/DebugProgressionTests.csproj -c Release
```

Tests compile the production progression, animation, WOL compatibility and roll-guard code against minimal game stand-ins. The native sword hook is extracted verbatim from ModEntry for guard tests. These are not actual game combat/kill tests. Full SMAPI compilation requires game/SMAPI references; the root `.github/workflows/patch44.yml` provisions them using SMAPI-ModBuildWorkflow.
