Run from the repository/source root:

```
python3 tests/check_gmcm_contract.py
python3 tests/check_sword_contract.py
python3 tests/generate_native_hook_test.py
dotnet run --project tests/DebugProgressionTests.csproj -c Release
```

Tests compile the production progression, animation, WOL compatibility and roll-guard code against minimal game stand-ins. The native sword hook is extracted verbatim from ModEntry for guard tests. These are not actual game combat/kill tests. Full SMAPI compilation requires game/SMAPI references; the root `.github/workflows/patch44.yml` provisions them using SMAPI-ModBuildWorkflow.

Tower consumer: `dotnet run --project tests/TowerContracts -c Release` links the production API4 adapter. For Harmony matrix use `-p:HarmonyTestVersion=2.3.3` or `2.4.2` on DebugProgressionTests. Older Harmony requires the pinned CI Linux runner; Debian13 rejects its native executable-stack helper before patch installation.
