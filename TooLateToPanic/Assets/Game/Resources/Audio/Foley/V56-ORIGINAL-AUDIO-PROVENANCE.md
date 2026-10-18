# Home V5.6 original local audio

All `v56_*.wav` files in this folder are new deterministic local procedural sound designs authored for Too Late to Panic! on 18 October 2026. The reproducible generator and sample-by-sample source PCM measurements are retained in `Evidence/GamejamOrchestra/TLTP-V56-20261018/staging/audio/`. They contain no external recordings, downloaded samples, synthesized speech, paid-provider output or new credit usage. Existing ElevenLabs asset provenance remains separate and unchanged.

Semantic mechanical designs cover accepted hammer impacts, gas/water valve grip/motion/final-seat/release, power breaker grip/motion/latch/release, committed gas-flare/pipe-leak/electrical-short events, cloth movement, outboard motor and water displacement. These are intentionally original stylized game cues; they do not certify acoustic realism or human listening quality.

Every source is mono PCM16 at 22050 Hz, peak capped at 0.52 full scale. Local source gains are bounded separately; master/SFX preferences multiply once, and no source ignores AudioListener volume or pause. Zodiac loops are two seconds and built from exact periodic integer-frequency sums. One-shot envelopes return to zero; 19 generated assets total. Measurements certify source PCM only; Unity import and actual output require the root's current-build checks.
