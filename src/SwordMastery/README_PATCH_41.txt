PATCH 41 — TOWER-SCALING DAMAGE / 1% PASSIVE ENERGY

Based on PATCH40 cumulative source, retaining PATCH39 mining drops, vanilla combat animations, and scoped 4-language CJK UI.

* All active sword skills already scale from the actual equipped sword's min/max average: Swordsman's Step, Slash, Calico Style, Sword Wave, Pinnacle, and Ultimate Footwork. Those original damage/SP multipliers are preserved.
* Issen now guarantees its original minimum 99,999 pre-critical damage but grows to max(99,999, current weapon average attack x100), so Tower reinforcement is not stuck at a fixed value.
* Swordsmanship Apex now uses max(90, actual weapon average attack) instead of fixed Infinity Blade reference 90. Its existing skill stage/SP damage multiplier and guaranteed critical behavior are unchanged.
* Sword Wave Stage III gains its own GMCM ON/OFF switch and optional rebindable hotkey (default unbound). The existing global Sword Wave F11 toggle remains in place.
* Stage III Sword Wave costs 1.00% of CURRENT maximum Energy per created wave (configurable 0.00%-5.00%, step 0.05%). Stage I/II keep zero extra cost. When energy is insufficient the wave is skipped, but normal sword attacks work normally.
* Swordsmanship Apex defaults to 1.00% of CURRENT maximum Energy per automatic attack cycle (configurable 0.00%-5.00%, step 0.05%). No enemies, no cost. F12 toggles it.
* Apex automatically STOPS attacking when current Energy is at or below 25% of current maximum; it RESUMES above 25% (subject to its normal attack interval). This 25% threshold is fixed and applies only to Apex; no Energy is spent during suspension.
* Old config.json values explicitly saved by players are respected. If prior Apex cost was set to 0.15%, reset/change in GMCM to 1.00% manually. New installs default to 1.00%.
* All four i18n files updated; glyph texture coverage checked/repaired.

Testing notes: Verify 4-language GMCM text and no effects on other mod settings; verify energy at various maximums and +50..+250 strengthened weapon damage in Tower; Issen and Apex are now weapon-scaled.
